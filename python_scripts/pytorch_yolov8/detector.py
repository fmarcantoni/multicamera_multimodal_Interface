#!/usr/bin/env python3

import sys
import numpy as np

import argparse
import torch
import cv2
import pyzed.sl as sl
from ultralytics import YOLO

from threading import Lock, Thread
from time import sleep

import ogl_viewer.viewer as gl
import cv_viewer.tracking_viewer as cv_viewer
import socket
import threading
import struct

lock = Lock()
run_signal = False
exit_signal = False
most_relevant_camera = "zed_camera"  # Default camera
workspace_cams_relevance = {
    "zed_camera": 0,
    "left_camera": 0,
    "right_camera": 0
}

last_known_IONA_position = None

PORT = 51420  # Initial port to connect to Unity to receive the port number

def send_image_to_unity(image, port):
    try:
        # Create the socket
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        # Connect to the Unity server
        print(f"Attempting to connect to Unity on port {port}...")
        s.connect(('127.0.0.1', port))
        print("Connected to Unity successfully!")

        # Encode the image
        _, img_encoded = cv2.imencode('.jpg', image)
        img_bytes = img_encoded.tobytes()\
        
        # Send message type ("IMG") first
        s.sendall(b"IMG")  # Send the message type first

        # Send the length of the image data first (4 bytes)
        length = len(img_bytes)
        s.sendall(struct.pack('<I', length))  # Little-endian byte order
        #s.sendall(length.to_bytes(4, byteorder='big'))
        #print(f"Sending image of size: {length} bytes")
        # Send the actual image data
        s.sendall(img_bytes)    
        print("Image sent to Unity!")
        # Close the connection
        s.close()
        #print("Connection closed.")
    
    except Exception as e:
        print(f"Error in sending image: {e}")
        return

def send_camera_selection(selection, port):
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect(('127.0.0.1', port))
        
        # Send message type ("MSG") first
        s.sendall(b"MSG")  

        # Encode the string and send its length
        message = selection.encode('utf-8')
        s.sendall(struct.pack('<I', len(message)))
        # Send the actual string message
        s.sendall(message)
        print(f"Sent camera selection: {selection}")
        s.close()
    except Exception as e:
        print(f"Error sending camera selection: {e}")

def xywh2abcd(xywh, im_shape):
    output = np.zeros((4, 2))

    # Center / Width / Height -> BBox corners coordinates
    x_min = (xywh[0] - 0.5*xywh[2]) #* im_shape[1]
    x_max = (xywh[0] + 0.5*xywh[2]) #* im_shape[1]
    y_min = (xywh[1] - 0.5*xywh[3]) #* im_shape[0]
    y_max = (xywh[1] + 0.5*xywh[3]) #* im_shape[0]

    # A ------ B
    # | Object |
    # D ------ C

    output[0][0] = x_min
    output[0][1] = y_min

    output[1][0] = x_max
    output[1][1] = y_min

    output[2][0] = x_max
    output[2][1] = y_max

    output[3][0] = x_min
    output[3][1] = y_max
    return output

def detections_to_custom_box(detections, im0):
    output = []
    for i, det in enumerate(detections):
        xywh = det.xywh[0]

        # Creating ingestable objects for the ZED SDK
        obj = sl.CustomBoxObjectData()
        obj.bounding_box_2d = xywh2abcd(xywh, im0.shape)
        obj.label = det.cls
        obj.probability = det.conf
        obj.is_grounded = False
        output.append(obj)
    return output

def torch_thread(weights, img_size, conf_thres=0.8, iou_thres=0.45):
    global image_net, exit_signal, run_signal, detections, model

    print("Intializing Network...")

    model = YOLO(weights)

    while not exit_signal:
        if run_signal:
            lock.acquire()

            img = cv2.cvtColor(image_net, cv2.COLOR_RGBA2RGB)
            # https://docs.ultralytics.com/modes/predict/#video-suffixes
            det = model.predict(img, save=False, imgsz=img_size, conf=conf_thres, iou=iou_thres)[0].cpu().numpy().boxes
            # ZED CustomBox format (with inverse letterboxing tf applied)
            detections = detections_to_custom_box(det, image_net)
            lock.release()
            run_signal = False
        sleep(0.01)

def most_relevant_workspace_cam(objects, last_camera):
    """
    Selects the appropriate camera based on the detected position of the robot.
    
    Parameters:
        objects: List of detected objects from the ZED camera.
    
    Returns:
        String representing the chosen camera ("left_camera", "right_camera", "zed_camera").
    """
    #global most_relevant_camera  # Ensure we are modifying the global variable
    global last_known_IONA_position

    if not objects.object_list:
        #If not object is detected
        if last_known_IONA_position:
            last_x, last_y, last_z = last_known_IONA_position
            if last_x < -0.7 and last_z > (-2.2): 
                most_relevant_camera = "right_camera"  # Robot is on the left, use right camera
                workspace_cams_relevance[most_relevant_camera] += 1
                workspace_cams_relevance["left_camera"] = 0
                workspace_cams_relevance["zed_camera"] = 0
                return most_relevant_camera
            elif last_x > 0.4 and last_z > (-2.2):
                most_relevant_camera = "left_camera"  # Robot is on the left, use right camera
                workspace_cams_relevance[most_relevant_camera] += 1
                workspace_cams_relevance["right_camera"] = 0
                workspace_cams_relevance["zed_camera"] = 0
                return most_relevant_camera
            else:
                most_relevant_camera = "zed_camera"  # Robot is on the left, use right camera
                workspace_cams_relevance[most_relevant_camera] += 1
                workspace_cams_relevance["right_camera"] = 0
                workspace_cams_relevance["left_camera"] = 0
                return most_relevant_camera
        return last_camera  # Default if no object is detected

    # Assume we are detecting a single robot in the scene

    for obj in objects.object_list:
        if model.names[int(obj.raw_label)] == "IONA" and obj.tracking_state == sl.OBJECT_TRACKING_STATE.OK:
            position = obj.position  # Get the 3D position of the detected robot
            robot_x = position[0]
            robot_y = position[1]
            robot_z = position[2]
            last_known_IONA_position = (robot_x, robot_y, robot_z)
            print(f"Robot detected at (x: {robot_x}, y: {robot_y}, z: {robot_z})")
            # Decide which camera to use based on position
            if robot_x < (-0.7) and robot_z > (-2.2): #robot_x < (-0.4) and robot_z > (-2.2): 
                most_relevant_camera = "right_camera"  # Robot is on the left, use right camera
                workspace_cams_relevance[most_relevant_camera] += 1
                workspace_cams_relevance["left_camera"] = 0
                workspace_cams_relevance["zed_camera"] = 0
                return most_relevant_camera  # Robot is on the left, use right camera
            elif robot_x > 0.4 and robot_z > (-2.2):
                most_relevant_camera = "left_camera"  # Robot is on the right, use left camera
                workspace_cams_relevance[most_relevant_camera] += 1
                workspace_cams_relevance["right_camera"] = 0
                workspace_cams_relevance["zed_camera"] = 0
                return most_relevant_camera
            elif (-0.6 <= robot_x <= 0.3) or robot_z < -2.2:
                most_relevant_camera = "zed_camera"  # Robot is in the center or far away
                workspace_cams_relevance[most_relevant_camera] += 1
                workspace_cams_relevance["right_camera"] = 0
                workspace_cams_relevance["left_camera"] = 0
                return most_relevant_camera
            else:
                return last_camera  # Robot is in the center or far away
    return last_camera  # Default if no valid object is detected


def main():
    global image_net, exit_signal, run_signal, detections, most_relevant_camera, model
    
    capture_thread = Thread(target=torch_thread, kwargs={'weights': opt.weights, 'img_size': opt.img_size, "conf_thres": opt.conf_thres})
    capture_thread.start()

    print("Initializing Camera...")

    zed = sl.Camera()

    input_type = sl.InputType()
    if opt.svo is not None:
        input_type.set_from_svo_file(opt.svo)

    # Create a InitParameters object and set configuration parameters
    init_params = sl.InitParameters(input_t=input_type, svo_real_time_mode=True)
    init_params.coordinate_units = sl.UNIT.METER
    init_params.depth_mode = sl.DEPTH_MODE.ULTRA  # QUALITY
    init_params.coordinate_system = sl.COORDINATE_SYSTEM.RIGHT_HANDED_Y_UP
    init_params.depth_maximum_distance = 50

    runtime_params = sl.RuntimeParameters()
    status = zed.open(init_params)

    if status != sl.ERROR_CODE.SUCCESS:
        print(repr(status))
        exit()

    image_left_tmp = sl.Mat()

    print("Initialized Camera")

    positional_tracking_parameters = sl.PositionalTrackingParameters()
    # If the camera is static, uncomment the following line to have better performances and boxes sticked to the ground.
    # positional_tracking_parameters.set_as_static = True
    zed.enable_positional_tracking(positional_tracking_parameters)

    obj_param = sl.ObjectDetectionParameters()
    obj_param.detection_model = sl.OBJECT_DETECTION_MODEL.CUSTOM_BOX_OBJECTS
    obj_param.enable_tracking = True
    obj_param.enable_segmentation = False  # designed to give person pixel mask with internal OD
    zed.enable_object_detection(obj_param)

    objects = sl.Objects()
    obj_runtime_param = sl.ObjectDetectionRuntimeParameters()

    # Display
    camera_infos = zed.get_camera_information()
    camera_res = camera_infos.camera_configuration.resolution
    # Create OpenGL viewer
    viewer = gl.GLViewer()
    point_cloud_res = sl.Resolution(min(camera_res.width, 720), min(camera_res.height, 404))
    point_cloud_render = sl.Mat()
    viewer.init(camera_infos.camera_model, point_cloud_res, obj_param.enable_tracking)
    point_cloud = sl.Mat(point_cloud_res.width, point_cloud_res.height, sl.MAT_TYPE.F32_C4, sl.MEM.CPU)
    image_left = sl.Mat()
    # Utilities for 2D display
    display_resolution = sl.Resolution(min(camera_res.width, 1280), min(camera_res.height, 720))
    image_scale = [display_resolution.width / camera_res.width, display_resolution.height / camera_res.height]
    image_left_ocv = np.full((display_resolution.height, display_resolution.width, 4), [245, 239, 239, 255], np.uint8)

    # Utilities for tracks view
    camera_config = camera_infos.camera_configuration
    tracks_resolution = sl.Resolution(400, display_resolution.height)
    track_view_generator = cv_viewer.TrackingViewer(tracks_resolution, camera_config.fps, init_params.depth_maximum_distance)
    track_view_generator.set_camera_calibration(camera_config.calibration_parameters)
    image_track_ocv = np.zeros((tracks_resolution.height, tracks_resolution.width, 4), np.uint8)
    # Camera pose
    cam_w_pose = sl.Pose()

    while viewer.is_available() and not exit_signal:
        if zed.grab(runtime_params) == sl.ERROR_CODE.SUCCESS:
            # -- Get the image
            lock.acquire()
            zed.retrieve_image(image_left_tmp, sl.VIEW.LEFT)
            image_net = image_left_tmp.get_data()
            send_image_to_unity(image_net, PORT)
            lock.release()
            run_signal = True

            # -- Detection running on the other thread
            while run_signal:
                sleep(0.001)

            # Wait for detections
            lock.acquire()
            # -- Ingest detections
            zed.ingest_custom_box_objects(detections)
            lock.release()
            zed.retrieve_objects(objects, obj_runtime_param)
            last_camera = most_relevant_camera
            most_relevant_camera = most_relevant_workspace_cam(objects, last_camera)
            print(f"Most relevant workspace camera: {most_relevant_camera}")
            if workspace_cams_relevance[most_relevant_camera] > 5:
                send_camera_selection(most_relevant_camera, PORT)

            # -- Display
            # Retrieve display data
            zed.retrieve_measure(point_cloud, sl.MEASURE.XYZRGBA, sl.MEM.CPU, point_cloud_res)
            point_cloud.copy_to(point_cloud_render)
            zed.retrieve_image(image_left, sl.VIEW.LEFT, sl.MEM.CPU, display_resolution)
            zed.get_position(cam_w_pose, sl.REFERENCE_FRAME.WORLD)

            # 3D rendering
            viewer.updateData(point_cloud_render, objects)
            # 2D rendering
            np.copyto(image_left_ocv, image_left.get_data())
            cv_viewer.render_2D(image_left_ocv, image_scale, objects, obj_param.enable_tracking)
        

            global_image = cv2.hconcat([image_left_ocv, image_track_ocv])
            # Tracking view
            track_view_generator.generate_view(objects, cam_w_pose, image_track_ocv, objects.is_tracked)

            cv2.imshow("ZED Object Detection Output", global_image)
            key = cv2.waitKey(10)
            if key == 27 or key == ord('q') or key == ord('Q'):
                exit_signal = True
        else:
            exit_signal = True

    viewer.exit()
    exit_signal = True
    zed.close()


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--weights', type=str, default='best.pt', help='best.pt path(s)')
    parser.add_argument('--svo', type=str, default=None, help='optional svo file, if not passed, use the plugged camera instead')
    parser.add_argument('--img_size', type=int, default=416, help='inference size (pixels)')
    parser.add_argument('--conf_thres', type=float, default=0.8, help='object confidence threshold')
    opt = parser.parse_args()

    with torch.no_grad():
        main()
