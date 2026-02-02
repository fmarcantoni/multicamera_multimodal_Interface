# VR Multi-Camera Control with Voice Commands and Autonomous View Selection

**Author:** Filippo Marcantoni
**Institution:** Worcester Polytechnic Institute (WPI)  
[cite_start]**Semester:** Spring 2025 (Capstone - RBE 593) [cite: 1, 231]

## 1. Project Overview
This repository contains the implementation of an intuitive multi-camera control system for the **Intelligent Robotic Nursing Assistant (IONA)**. [cite_start]The project addresses challenges such as lack of situational awareness, depth perception issues, and inefficient camera switching during teleoperation in complex healthcare environments[cite: 233, 239].

### Project Goals
* [cite_start]**Enhance Situational Awareness:** Integrate multiple fixed and dynamic camera views into a single Virtual Reality (VR) interface[cite: 233, 234].
* [cite_start]**Reduce Cognitive Load:** Implement voice commands and autonomous switching to minimize manual interactions[cite: 232, 235].
* [cite_start]**Immersive Telepresence:** Use an "active neck" camera that mirrors the operator's head movements[cite: 234, 251].

---

## 2. Hardware and Camera Setup
[cite_start]The system utilizes a total of seven cameras to provide comprehensive workspace coverage[cite: 251, 353]:

| Camera Type | Model | Position | Purpose |
| :--- | :--- | :--- | :--- |
| **Workspace (3)** | Intel RealSense D345 / ZED Mini | [cite_start]Left, Right, and behind the bed [cite: 353] | [cite_start]Wide field of view and depth for pose estimation[cite: 253, 353]. |
| **Fixed Robot (3)** | Intel RealSense D345 | [cite_start]Robot chest and arms [cite: 355] | [cite_start]Close-up views for manipulation and grasping[cite: 154, 157]. |
| **Active Robot (1)** | Intel RealSense D345 | [cite_start]Robot head (2-DoF Gimbal) [cite: 355] | [cite_start]Mirrors VR headset rotation for immersive viewing[cite: 356]. |

---

## 3. System Architecture
[cite_start]The system is built on a multi-layered architecture distributed across three machines: the IONA robot, an Ubuntu workstation, and a Windows laptop[cite: 104, 367].

* [cite_start]**Ubuntu Machine:** Connects workspace cameras and publishes video feeds to ROS topics (e.g., `left_workspace_img_raw`)[cite: 372, 375, 379].
* [cite_start]**IONA (Robot):** Streams its internal camera feeds and teleoperation phases (Navigation, Manipulation, Grasping) via UDP[cite: 110, 111, 145].
* [cite_start]**Windows Machine (Unity):** Hosts the VR environment and performs the following[cite: 368, 115]:
    * [cite_start]Receives camera feeds and phases via UDP/TCP[cite: 107, 141].
    * [cite_start]Runs a custom **YOLOv8** object detection model for robot pose estimation[cite: 164, 167].
    * [cite_start]Integrates the **Whisper Unity** package for voice recognition[cite: 124, 131].

---

## 4. Interaction Modalities

### (i) Manual Mode
[cite_start]Operators interact directly with a side-by-side GUI using Meta Quest 2 controllers[cite: 117, 128].
* [cite_start]**Interface:** Features dropdown menus for mode selection and camera assignments for primary and secondary views[cite: 121, 127].
* [cite_start]**Interaction:** Uses the **XR Interaction Toolkit** with ray-interactors visualized as dots on the canvas[cite: 117, 128].

### (ii) Vocal Mode
[cite_start]Hands-free camera management using an Automatic Speech Recognition (ASR) system[cite: 130, 361].
* [cite_start]**Engine:** **OpenAI Whisper (Tiny English model)** optimized for real-time transcription with CUDA support[cite: 134].
* **Syntax:**
    * [cite_start]**Selection:** "Select primary view" or "Select secondary display"[cite: 137].
    * [cite_start]**Switching:** Followed by camera keywords like "left cam," "chest cam," or "active cam"[cite: 138].
* [cite_start]**Feedback:** The UI displays a live transcription scroll view and highlights selected displays in green[cite: 139, 140].

### (iii) Autonomous Mode
[cite_start]A logic-based state machine that automatically selects views based on the current teleoperation phase[cite: 141, 142]:

| Phase | Primary View | Secondary View Logic |
| :--- | :--- | :--- |
| **Navigation** | [cite_start]Active Camera [cite: 150] | [cite_start]Workspace camera with highest robot visibility (via ZED/YOLOv8)[cite: 149]. |
| **Arm/Chest Manipulation** | [cite_start]Active Camera [cite: 152, 154] | [cite_start]Closest workspace view or chest camera for alignment[cite: 152, 154]. |
| **Grasping** | [cite_start]Active Camera [cite: 157] | [cite_start]Close-up view of the specific arm being operated[cite: 157]. |

---

## 5. Software Stack & Dependencies
* [cite_start]**Operating Systems:** Ubuntu 20.04/22.04 (ROS2) and Windows 10/11 (Unity)[cite: 367].
* [cite_start]**Robotics:** ROS / ROS-TCP-Connector[cite: 366].
* [cite_start]**Game Engine:** Unity 3D with XR Interaction Toolkit[cite: 306, 117].
* [cite_start]**AI/Vision:** YOLOv8 (Object Detection) and OpenAI Whisper (Speech)[cite: 164, 131].
* [cite_start]**Communication:** UDP (for phases/feeds) and TCP (for high-bandwidth detection output)[cite: 141, 376].

---

## 6. Conclusion & Future Work

### Conclusion
[cite_start]The project successfully integrated manual, vocal, and autonomous interaction modalities into a unified VR interface[cite: 198]. [cite_start]The system enhances teleoperation flexibility and situational awareness by leveraging multi-camera fusion and automated state machines[cite: 198, 348].

### Future Work
* [cite_start]**Formal User Study:** Implement the proposed evaluation plan (NASA-TLX, SUS, and cognitive load testing via arithmetic problems) to quantify performance gains[cite: 174, 193, 200].
* [cite_start]**Manipulation Extension:** Re-integrate arm and chest cameras into the primary testing loop[cite: 201].
* [cite_start]**Shared Control:** Explore adaptive perspective methods for high-precision grasping tasks[cite: 202].

---
[cite_start]*For more technical details, refer to the full Directed Research report at WPI.* [cite: 231]