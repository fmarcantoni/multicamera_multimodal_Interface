import socket

UDP_IP = "0.0.0.0"
UDP_PORT = 8050
UDP_UNITY_PORT = 8055
BUFFER_SIZE = 1024  # Maximum UDP packet size

socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
socket.bind((UDP_IP, UDP_PORT))
print(f"[UDP] Listening on {UDP_IP}:{UDP_PORT}")
while True:
    data, addr = socket.recvfrom(BUFFER_SIZE)  # buffer size is 1024 bytes
    print(f"[UDP] Received message: {data.decode('utf-8')} from {addr}")
    socket.sendto(data, ("127.0.0.1", UDP_UNITY_PORT))
    print(f"[UDP] Sent message")
