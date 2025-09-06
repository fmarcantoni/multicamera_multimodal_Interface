import socket
import numpy as np
import cv2

# Settings - must match the sender
UDP_IP = "0.0.0.0"  # Listen on all interfaces
UDP_PORT = 8080     # Must match sender's port
BUFFER_SIZE = 65536  # Maximum UDP packet size

# Data structures
frame_chunks = {}  # Maps frame_number to a dict of chunk_index: data
frame_info = {}    # Maps frame_number to (expected_total_chunks, total_size)

# Create the UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
print(f"Listening on {UDP_IP}:{UDP_PORT}")

while True:
    data, addr = sock.recvfrom(BUFFER_SIZE)
    sender_ip = addr[0]

    try:
        # Split the header and payload
        header, payload = data.split(bytes([1]), 1)
        header = header.decode('utf-8')
        frame_number, chunk_index, max_chunks, current_index, payload_size, frame_size = map(int, header.split('_'))

        # Store the frame info
        if frame_number not in frame_chunks:
            frame_chunks[frame_number] = {}
            frame_info[frame_number] = (max_chunks, frame_size)

        # Save the chunk
        frame_chunks[frame_number][chunk_index] = payload

        # Check if full frame is received
        if len(frame_chunks[frame_number]) == frame_info[frame_number][0]:
            # Reconstruct the image bytes
            chunks = [frame_chunks[frame_number][i] for i in range(frame_info[frame_number][0])]
            full_image_bytes = b''.join(chunks)

            # Decode image from bytes
            img_array = np.frombuffer(full_image_bytes, dtype=np.uint8)
            image = cv2.imdecode(img_array, cv2.IMREAD_COLOR)

            if image is not None:
                print(f"Received image of length {len(full_image_bytes)} bytes from {sender_ip}")
                # Optional: display image
                # cv2.imshow("Received Image", image)
                # cv2.waitKey(1)
            else:
                print(f"Failed to decode image from {sender_ip}")

            # Cleanup
            del frame_chunks[frame_number]
            del frame_info[frame_number]

    except Exception as e:
        print(f"Error receiving data: {e}")
