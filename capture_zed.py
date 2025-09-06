
import pyzed.sl as sl
import cv2
import numpy as np
import os
import time


def main():
    # Create directory to save images
    output_folder = "zed_dataset"
    try:
        os.makedirs(output_folder, exist_ok=True)
        print(f"Directory created (or already exists): {output_folder}")
    except Exception as e:
        print(f"Error creating directory: {e}")
        return  # Exit if we can't create the directory
    os.makedirs(output_folder, exist_ok=True)
    # Create a Camera object
    zed = sl.Camera()

    # Create a InitParameters object and set configuration parameters
    init_params = sl.InitParameters()
    init_params.camera_resolution = sl.RESOLUTION.AUTO # Use HD720 opr HD1200 video mode, depending on camera type.
    init_params.camera_fps = 30  # Set fps at 30

    # Open the camera
    err = zed.open(init_params)
    if err != sl.ERROR_CODE.SUCCESS:
        print("Camera Open : "+repr(err)+". Exit program.")
        exit()

    print("Press 'q' to stop capturing images...")

    # Capture a frame every two seconds and save them in the datset folder
    i = 0 # frame counter
    image = sl.Mat()
    runtime_parameters = sl.RuntimeParameters()
    last_capture_time = time.time()  # Store the time of the last capture
    #frame_count = 0  # Frame counter
    while True:
        # Grab an image, a RuntimeParameters object must be given to grab()
        if zed.grab(runtime_parameters) == sl.ERROR_CODE.SUCCESS:
            # Get current time
            current_time = time.time()

            # Capture an image only if 2 seconds have passed since last capture
            if current_time - last_capture_time >= 2:
                last_capture_time = current_time  # Update last capture time

                # A new image is available if grab() returns SUCCESS
                zed.retrieve_image(image, sl.VIEW.LEFT)
                timestamp = zed.get_timestamp(sl.TIME_REFERENCE.CURRENT)  # Get the timestamp at the time the image was captured
                print("Image resolution: {0} x {1} || Image timestamp: {2}\n".format(image.get_width(), image.get_height(),
                      timestamp.get_milliseconds()))

                frame = image.get_data()[:, :, :3]
                if frame is None or frame.size == 0:
                    print("Error: Captured frame is empty!")
                    continue
                #frame = cv2.cvtColor(image.get_data()[:, :, :3], cv2.COLOR_RGB2BGR)

                # Save image
                filename = os.path.join(output_folder, f"frame_{i:04d}.jpg")
                success = cv2.imwrite(filename, frame)
                if success:
                    print(f"Saved: {filename}")
                else:
                    print(f"Failed to save image: {filename}")


                cv2.imshow("ZED Camera", frame)
                i += 1
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break

            # Display the image in real-time
            
        # Exit on 'q' key press
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    # Close the camera
    zed.close()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()

