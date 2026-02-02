# VR Multi-Camera Control with Voice Commands and Autonomous View Selection

**Author:** Filippo Marcantoni
**Institution:** Worcester Polytechnic Institute (WPI)  
**Semester:** Spring 2025 (Capstone - RBE 593)

## 1. Project Overview
This repository contains the implementation of an intuitive multi-camera control system for the **Intelligent Robotic Nursing Assistant (IONA)**. The project addresses challenges such as lack of situational awareness, depth perception issues, and inefficient camera switching during teleoperation in complex healthcare environments.

### Project Goals
* **Enhance Situational Awareness:** Integrate multiple fixed and dynamic camera views into a single Virtual Reality (VR) interface.
* **Reduce Cognitive Load:** Implement voice commands and autonomous switching to minimize manual interactions.
* **Immersive Telepresence:** Use an "active neck" camera that mirrors the operator's head movements.

---

## 2. Hardware and Camera Setup
The system utilizes a total of seven cameras to provide comprehensive workspace coverage:

| Camera Type | Model | Position | Purpose |
| :--- | :--- | :--- | :--- |
| **Workspace (3)** | Intel RealSense D345 / ZED Mini | Left, Right, and behind the bed in nursing setup. | Wide field of view and depth for pose estimation on ZED Mini. |
| **Fixed Robot (3)** | Intel RealSense D345 | Robot chest and arms | Close-up views for manipulation and grasping. |
| **Active Robot (1)** | Intel RealSense D345 | Robot head (2-DoF Gimbal) | Mirrors VR headset rotation for immersive viewing. | Refer to the repository: [https://github.com/fmarcantoni/active_neck]

---

## 3. System Architecture
The system is built on a multi-layered architecture distributed across three machines: the IONA robot, an Ubuntu workstation, and a Windows laptop.

* **Ubuntu Machine:** Connects workspace cameras and publishes video feeds to ROS topics. Refer to the repository: [https://github.com/fmarcantoni/udp_ros].
* **IONA (Robot):** Streams its internal camera feeds and teleoperation phases (Navigation, Chest Movement, Manipulation, Grasping) via UDP [https://github.com/fmarcantoni/udp_ros].
* **Windows Machine (Unity):** Hosts the VR environment and performs the following:
    * Receives camera feeds and phases via UDP/TCP [https://github.com/fmarcantoni/udp_ros].
    * Runs a custom **YOLOv8** object detection model for robot pose estimation.
    * Integrates the **Whisper Unity** package for voice recognition.

---

## 4. Interaction Modalities

### (i) Manual Mode
Operators interact directly with a side-by-side GUI using Meta Quest 2 controllers.
* **Interface:** Features dropdown menus for mode selection and camera assignments for primary and secondary views.
* **Interaction:** Uses the **XR Interaction Toolkit** with ray-interactors visualized as dots on the canvas.

### (ii) Vocal Mode
Hands-free camera management using an Automatic Speech Recognition (ASR) system.
* **Engine:** **OpenAI Whisper (Tiny English model)** optimized for real-time transcription with CUDA support.
* **Syntax:**
    * **Selection:** "Select primary view" or "Select secondary display".
    * **Switching:** Followed by camera keywords like "left cam," "chest cam," or "active cam".
* **Feedback:** The UI displays a live transcription scroll view and highlights selected displays in green.

### (iii) Autonomous Mode
A logic-based state machine that automatically selects views based on the current teleoperation phase:

| Phase | Primary View | Secondary View Logic |
| :--- | :--- | :--- |
| **Navigation** | Active Camera | Workspace camera with highest robot visibility (via ZED/YOLOv8). |
| **Arm/Chest Manipulation** | Active Camera | Closest workspace view or chest camera for alignment. |
| **Grasping** | Active Camera | Close-up view of the specific arm being operated. |

---

## 5. Software Stack & Dependencies
* **Operating Systems:** Ubuntu 20.04/22.04 (ROS1) and Windows 10/11 (Unity).
* **Robotics:** ROS / ROS-TCP-Connector.
* **Game Engine:** Unity 3D with XR Interaction Toolkit.
* **AI/Vision:** YOLOv8 (Object Detection) and OpenAI Whisper (Speech).
* **Communication:** UDP (for phases/feeds) and TCP (for high-bandwidth detection output). [https://github.com/fmarcantoni/udp_ros]

---

## 6. Conclusion & Future Work

### Conclusion
The project successfully integrated manual, vocal, and autonomous interaction modalities into a unified VR interface. The system enhances teleoperation flexibility and situational awareness by leveraging multi-camera fusion and automated state machines.

### Future Work
* **Formal User Study:** Implement the proposed evaluation plan (NASA-TLX, SUS, and cognitive load testing via arithmetic problems) to quantify performance gains.
* **Manipulation Extension:** Re-integrate arm and chest cameras into the primary testing loop.
* **Shared Control:** Explore adaptive perspective methods for high-precision grasping tasks.

---
*For more technical details, refer to the full Directed Research report at WPI.* [https://drive.google.com/file/d/17oOjVCBM6c-d76qBRqQg9C-A8jvlqcbv/view]
