using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StateMachine : MonoBehaviour
{
    public TMP_Text best_workspace_camera;
    public RenderTexture zedPrimaryTexture, zedSecondaryTexture;
    public RenderTexture leftWSPrimaryTexture, leftWSSecondaryTexture;
    public RenderTexture rightWSPrimaryTexture, rightWSSecondaryTexture;
    public RenderTexture activePrimaryTexture, activeSecondaryTexture;
    public RenderTexture chestPrimaryTexture, chestSecondaryTexture;
    public RenderTexture leftArmPrimaryTexture, leftArmSecondaryTexture;
    public RenderTexture rightArmPrimaryTexture, rightArmSecondaryTexture;

    public RawImage primaryDisplay, secondaryDisplay;
    public TMP_Dropdown primaryDropdown, secondaryDropdown;
    private List<string> cameras = new List<string>(){"left_ws", "zed_ws", "right_ws", "robot_active", "robot_back" ,"robot_base", "robot_chest", "robot_left_arm", "robot_right_arm"};
    private List<string> states = new List<string>(){"WAITING", "NAVIGATION", "CHEST MANIPULATION", "ARM MANIPULATION", "GRASPING"};
    public TMP_Text teleop_phase_text;
    private string currentState = "WAITING";
    private string previousState;
    private string previous_previousState;
    private string currentCameraPrimary, currentCameraSecondary;
    // Start is called before the first frame update
    public bool isLeftArmActive = false;
    public bool isRightArmActive = false;
    private bool InputUser = false;
    private string UserInputPrimaryCamera = "";
    private string UserInputSecondaryCamera = "";
    void Start()
    {
        primaryDropdown.value = 3;
        secondaryDropdown.value = 1;
        currentCameraPrimary = "robot_active";
        currentCameraSecondary = "zed_ws";
        //zedCamera.targetTexture = zedSecondaryRenderTexture;
        primaryDisplay.texture = activePrimaryTexture;
        secondaryDisplay.texture = zedSecondaryTexture;

        // Add a listener for a change in state sent from ROS
        primaryDropdown.onValueChanged.AddListener(delegate{OnDropdownValueChanged(primaryDropdown, primaryDisplay); });
        secondaryDropdown.onValueChanged.AddListener(delegate{OnDropdownValueChanged(secondaryDropdown, secondaryDisplay); });
    }

    void Update()
    {
        if(teleop_phase_text.text != currentState){
            previous_previousState = previousState;
            previousState = currentState;
            currentState = teleop_phase_text.text;
            UnityEngine.Debug.Log($"State changed to: {currentState}");
            StateChange();
        }
    }

    public void StateChange()
    {
        // Implement your state machine logic here
        if(InputUser && (previousState != currentState)){
            InputUser = false;
        }
            // If the user is controlling the camera, do not change the state
        switch (currentState)
        {
            case "WAITING":
            if(!InputUser){
                isLeftArmActive = false;
                isRightArmActive = false;
                primaryDisplay.texture = activePrimaryTexture;
                secondaryDisplay.texture = zedSecondaryTexture;
                currentCameraPrimary = "robot_active";
                currentCameraSecondary = "zed_ws";
                primaryDropdown.value = 3;
                secondaryDropdown.value = 1;
            } else {
                // If the user is controlling the camera, do not change the state
                if(UserInputPrimaryCamera == "robot_active"){
                    primaryDisplay.texture = activePrimaryTexture;
                    currentCameraPrimary = "robot_active";
                    primaryDropdown.value = 3;
                } else if(UserInputPrimaryCamera == "zed_ws"){
                    primaryDisplay.texture = zedPrimaryTexture;
                    currentCameraPrimary = "zed_ws";
                    primaryDropdown.value = 1;
                } else if(UserInputPrimaryCamera == "left_ws"){
                    primaryDisplay.texture = leftWSPrimaryTexture;
                    currentCameraPrimary = "left_ws";
                    primaryDropdown.value = 0;
                } else if(UserInputPrimaryCamera == "right_ws"){
                    primaryDisplay.texture = rightWSPrimaryTexture;
                    currentCameraPrimary = "right_ws";
                    primaryDropdown.value = 2;
                } else if(UserInputPrimaryCamera == "robot_chest"){
                    primaryDisplay.texture = chestPrimaryTexture;
                    currentCameraPrimary = "robot_chest";
                    primaryDropdown.value = 4;
                } else if(UserInputPrimaryCamera == "robot_left_arm"){
                    primaryDisplay.texture = leftArmPrimaryTexture;
                    currentCameraPrimary = "robot_left_arm";
                    primaryDropdown.value = 5;
                } else if(UserInputPrimaryCamera == "robot_right_arm"){
                    primaryDisplay.texture = rightArmPrimaryTexture;
                    currentCameraPrimary = "robot_right_arm";
                    primaryDropdown.value = 6;
                }
                else if(UserInputSecondaryCamera == "robot_active"){
                    secondaryDisplay.texture = activeSecondaryTexture;
                    currentCameraSecondary = "robot_active";
                    secondaryDropdown.value = 3;
                } else if(UserInputSecondaryCamera == "zed_ws"){
                    secondaryDisplay.texture = zedSecondaryTexture;
                    currentCameraSecondary = "zed_ws";
                    secondaryDropdown.value = 1;
                } else if(UserInputSecondaryCamera == "left_ws"){
                    secondaryDisplay.texture = leftWSSecondaryTexture;
                    currentCameraSecondary = "left_ws";
                    secondaryDropdown.value = 0;
                } else if(UserInputSecondaryCamera == "right_ws"){
                    secondaryDisplay.texture = rightWSSecondaryTexture;
                    currentCameraSecondary = "right_ws";
                    secondaryDropdown.value = 2;
                } else if(UserInputSecondaryCamera == "robot_chest"){
                    secondaryDisplay.texture = chestSecondaryTexture;
                    currentCameraSecondary = "robot_chest";
                    secondaryDropdown.value = 4;
                } else if(UserInputSecondaryCamera == "robot_left_arm"){
                    secondaryDisplay.texture = leftArmSecondaryTexture;
                    currentCameraSecondary = "robot_left_arm";
                    secondaryDropdown.value = 5;
                } else if(UserInputSecondaryCamera == "robot_right_arm"){
                    secondaryDisplay.texture = rightArmSecondaryTexture;
                    currentCameraSecondary = "robot_right_arm";
                    secondaryDropdown.value = 6;
                }
            }
                break;
            case "NAVIGATION":
                isLeftArmActive = false;
                isRightArmActive = false;
                if(!InputUser){
                    primaryDisplay.texture = activePrimaryTexture;
                    currentCameraPrimary = "robot_active";
                    primaryDropdown.value = 3;
                    if(best_workspace_camera.text == "ZED WS Camera"){
                        secondaryDisplay.texture = zedSecondaryTexture;
                        currentCameraSecondary = "zed_ws";
                        secondaryDropdown.value = 1;
                    } else if(best_workspace_camera.text == "Left WS Camera"){
                        secondaryDisplay.texture = leftWSSecondaryTexture;
                        currentCameraSecondary = "left_ws";
                        secondaryDropdown.value = 0;
                    } else if(best_workspace_camera.text == "Right WS Camera"){
                        secondaryDisplay.texture = rightWSSecondaryTexture;
                        currentCameraSecondary = "right_ws";
                        secondaryDropdown.value = 2;
                    }
                } else {
                    if(UserInputPrimaryCamera == "robot_active"){
                    primaryDisplay.texture = activePrimaryTexture;
                    currentCameraPrimary = "robot_active";
                    primaryDropdown.value = 3;
                } else if(UserInputPrimaryCamera == "zed_ws"){
                    primaryDisplay.texture = zedPrimaryTexture;
                    currentCameraPrimary = "zed_ws";
                    primaryDropdown.value = 1;
                } else if(UserInputPrimaryCamera == "left_ws"){
                    primaryDisplay.texture = leftWSPrimaryTexture;
                    currentCameraPrimary = "left_ws";
                    primaryDropdown.value = 0;
                } else if(UserInputPrimaryCamera == "right_ws"){
                    primaryDisplay.texture = rightWSPrimaryTexture;
                    currentCameraPrimary = "right_ws";
                    primaryDropdown.value = 2;
                } else if(UserInputPrimaryCamera == "robot_chest"){
                    primaryDisplay.texture = chestPrimaryTexture;
                    currentCameraPrimary = "robot_chest";
                    primaryDropdown.value = 4;
                } else if(UserInputPrimaryCamera == "robot_left_arm"){
                    primaryDisplay.texture = leftArmPrimaryTexture;
                    currentCameraPrimary = "robot_left_arm";
                    primaryDropdown.value = 5;
                } else if(UserInputPrimaryCamera == "robot_right_arm"){
                    primaryDisplay.texture = rightArmPrimaryTexture;
                    currentCameraPrimary = "robot_right_arm";
                    primaryDropdown.value = 6;
                }
                else if(UserInputSecondaryCamera == "robot_active"){
                    secondaryDisplay.texture = activeSecondaryTexture;
                    currentCameraSecondary = "robot_active";
                    secondaryDropdown.value = 3;
                } else if(UserInputSecondaryCamera == "zed_ws"){
                    secondaryDisplay.texture = zedSecondaryTexture;
                    currentCameraSecondary = "zed_ws";
                    secondaryDropdown.value = 1;
                } else if(UserInputSecondaryCamera == "left_ws"){
                    secondaryDisplay.texture = leftWSSecondaryTexture;
                    currentCameraSecondary = "left_ws";
                    secondaryDropdown.value = 0;
                } else if(UserInputSecondaryCamera == "right_ws"){
                    secondaryDisplay.texture = rightWSSecondaryTexture;
                    currentCameraSecondary = "right_ws";
                    secondaryDropdown.value = 2;
                } else if(UserInputSecondaryCamera == "robot_chest"){
                    secondaryDisplay.texture = chestSecondaryTexture;
                    currentCameraSecondary = "robot_chest";
                    secondaryDropdown.value = 4;
                } else if(UserInputSecondaryCamera == "robot_left_arm"){
                    secondaryDisplay.texture = leftArmSecondaryTexture;
                    currentCameraSecondary = "robot_left_arm";
                    secondaryDropdown.value = 5;
                } else if(UserInputSecondaryCamera == "robot_right_arm"){
                    secondaryDisplay.texture = rightArmSecondaryTexture;
                    currentCameraSecondary = "robot_right_arm";
                    secondaryDropdown.value = 6;
                }
            }
                break;
            case "CHEST MANIPULATION":
                isLeftArmActive = false;
                isRightArmActive = false;
                if(!InputUser){
                    primaryDisplay.texture = activePrimaryTexture;
                    currentCameraPrimary = "robot_active";

                    secondaryDisplay.texture = chestSecondaryTexture;
                    currentCameraSecondary = "robot_chest";
                    primaryDropdown.value = 3;
                    secondaryDropdown.value = 4;
                } else {
                    if(UserInputPrimaryCamera == "robot_active"){
                        primaryDisplay.texture = activePrimaryTexture;
                        currentCameraPrimary = "robot_active";
                        primaryDropdown.value = 3;
                    } else if(UserInputPrimaryCamera == "zed_ws"){
                        primaryDisplay.texture = zedPrimaryTexture;
                        currentCameraPrimary = "zed_ws";
                        primaryDropdown.value = 1;
                    } else if(UserInputPrimaryCamera == "left_ws"){
                        primaryDisplay.texture = leftWSPrimaryTexture;
                        currentCameraPrimary = "left_ws";
                        primaryDropdown.value = 0;
                    } else if(UserInputPrimaryCamera == "right_ws"){
                        primaryDisplay.texture = rightWSPrimaryTexture;
                        currentCameraPrimary = "right_ws";
                        primaryDropdown.value = 2;
                    } else if(UserInputPrimaryCamera == "robot_chest"){
                        primaryDisplay.texture = chestPrimaryTexture;
                        currentCameraPrimary = "robot_chest";
                        primaryDropdown.value = 4;
                    } else if(UserInputPrimaryCamera == "robot_left_arm"){
                        primaryDisplay.texture = leftArmPrimaryTexture;
                        currentCameraPrimary = "robot_left_arm";
                        primaryDropdown.value = 5;
                    } else if(UserInputPrimaryCamera == "robot_right_arm"){
                        primaryDisplay.texture = rightArmPrimaryTexture;
                        currentCameraPrimary = "robot_right_arm";
                        primaryDropdown.value = 6;
                    }
                    else if(UserInputSecondaryCamera == "robot_active"){
                        secondaryDisplay.texture = activeSecondaryTexture;
                        currentCameraSecondary = "robot_active";
                        secondaryDropdown.value = 3;
                    } else if(UserInputSecondaryCamera == "zed_ws"){
                        secondaryDisplay.texture = zedSecondaryTexture;
                        currentCameraSecondary = "zed_ws";
                        secondaryDropdown.value = 1;
                    } else if(UserInputSecondaryCamera == "left_ws"){
                        secondaryDisplay.texture = leftWSSecondaryTexture;
                        currentCameraSecondary = "left_ws";
                        secondaryDropdown.value = 0;
                    } else if(UserInputSecondaryCamera == "right_ws"){
                        secondaryDisplay.texture = rightWSSecondaryTexture;
                        currentCameraSecondary = "right_ws";
                        secondaryDropdown.value = 2;
                    } else if(UserInputSecondaryCamera == "robot_chest"){
                        secondaryDisplay.texture = chestSecondaryTexture;
                        currentCameraSecondary = "robot_chest";
                        secondaryDropdown.value = 4;
                    } else if(UserInputSecondaryCamera == "robot_left_arm"){
                        secondaryDisplay.texture = leftArmSecondaryTexture;
                        currentCameraSecondary = "robot_left_arm";
                        secondaryDropdown.value = 5;
                    } else if(UserInputSecondaryCamera == "robot_right_arm"){
                        secondaryDisplay.texture = rightArmSecondaryTexture;
                        currentCameraSecondary = "robot_right_arm";
                        secondaryDropdown.value = 6;
                    }
                }
                break;
            case "LEFT ARM MANIPULATION":
                isLeftArmActive = true;

                if(isLeftArmActive && isRightArmActive){
                    currentState = "ARM MANIPULATION";
                    StateChange();
                } else{
                    currentState = "GRASPING";
                    StateChange();
                }
                
                break;
            case "RIGHT ARM MANIPULATION":
                isRightArmActive = true;
                 if(isLeftArmActive && isRightArmActive){
                    currentState = "ARM MANIPULATION";
                    StateChange();
                } else{
                    currentState = "GRASPING";
                    StateChange();
                }
                
                break;
            case "ARM MANIPULATION":
                if(!InputUser){
                    primaryDisplay.texture = activePrimaryTexture;
                    currentCameraPrimary = "robot_active";
                    primaryDropdown.value = 3;
                    if(best_workspace_camera.text == "ZED WS Camera"){
                        secondaryDisplay.texture = zedSecondaryTexture;
                        currentCameraSecondary = "zed_ws";
                        secondaryDropdown.value = 1;
                    } else if(best_workspace_camera.text == "Left WS Camera"){
                        secondaryDisplay.texture = leftWSSecondaryTexture;
                        currentCameraSecondary = "left_ws";
                        secondaryDropdown.value = 0;
                    } else if(best_workspace_camera.text == "Right WS Camera"){
                        secondaryDisplay.texture = rightWSSecondaryTexture;
                        currentCameraSecondary = "right_ws";
                        secondaryDropdown.value = 2;
                    }
                } else {
                    if(UserInputPrimaryCamera == "robot_active"){
                        primaryDisplay.texture = activePrimaryTexture;
                        currentCameraPrimary = "robot_active";
                        primaryDropdown.value = 3;
                    } else if(UserInputPrimaryCamera == "zed_ws"){
                        primaryDisplay.texture = zedPrimaryTexture;
                        currentCameraPrimary = "zed_ws";
                        primaryDropdown.value = 1;
                    } else if(UserInputPrimaryCamera == "left_ws"){
                        primaryDisplay.texture = leftWSPrimaryTexture;
                        currentCameraPrimary = "left_ws";
                        primaryDropdown.value = 0;
                    } else if(UserInputPrimaryCamera == "right_ws"){
                        primaryDisplay.texture = rightWSPrimaryTexture;
                        currentCameraPrimary = "right_ws";
                        primaryDropdown.value = 2;
                    } else if(UserInputPrimaryCamera == "robot_chest"){
                        primaryDisplay.texture = chestPrimaryTexture;
                        currentCameraPrimary = "robot_chest";
                        primaryDropdown.value = 4;
                    } else if(UserInputPrimaryCamera == "robot_left_arm"){
                        primaryDisplay.texture = leftArmPrimaryTexture;
                        currentCameraPrimary = "robot_left_arm";
                        primaryDropdown.value = 5;
                    } else if(UserInputPrimaryCamera == "robot_right_arm"){
                        primaryDisplay.texture = rightArmPrimaryTexture;
                        currentCameraPrimary = "robot_right_arm";
                        primaryDropdown.value = 6;
                    }
                    else if(UserInputSecondaryCamera == "robot_active"){
                        secondaryDisplay.texture = activeSecondaryTexture;
                        currentCameraSecondary = "robot_active";
                        secondaryDropdown.value = 3;
                    } else if(UserInputSecondaryCamera == "zed_ws"){
                        secondaryDisplay.texture = zedSecondaryTexture;
                        currentCameraSecondary = "zed_ws";
                        secondaryDropdown.value = 1;
                    } else if(UserInputSecondaryCamera == "left_ws"){
                        secondaryDisplay.texture = leftWSSecondaryTexture;
                        currentCameraSecondary = "left_ws";
                        secondaryDropdown.value = 0;
                    } else if(UserInputSecondaryCamera == "right_ws"){
                        secondaryDisplay.texture = rightWSSecondaryTexture;
                        currentCameraSecondary = "right_ws";
                        secondaryDropdown.value = 2;
                    } else if(UserInputSecondaryCamera == "robot_chest"){
                        secondaryDisplay.texture = chestSecondaryTexture;
                        currentCameraSecondary = "robot_chest";
                        secondaryDropdown.value = 4;
                    } else if(UserInputSecondaryCamera == "robot_left_arm"){
                        secondaryDisplay.texture = leftArmSecondaryTexture;
                        currentCameraSecondary = "robot_left_arm";
                        secondaryDropdown.value = 5;
                    } else if(UserInputSecondaryCamera == "robot_right_arm"){
                        secondaryDisplay.texture = rightArmSecondaryTexture;
                        currentCameraSecondary = "robot_right_arm";
                        secondaryDropdown.value = 6;
                    }
                }
                break;
            case "GRASPING":
                if(!InputUser){
                    if(previous_previousState == "CHEST MANIPULATION"){
                        primaryDisplay.texture = activePrimaryTexture;
                        currentCameraPrimary = "robot_active";
                        secondaryDisplay.texture = chestSecondaryTexture;
                        currentCameraSecondary = "robot_chest";
                        primaryDropdown.value = 3;
                        secondaryDropdown.value = 4;
                    } else {
                        primaryDisplay.texture = activePrimaryTexture;
                        currentCameraPrimary = "robot_active";
                        primaryDropdown.value = 3;
                        if(isLeftArmActive && !isRightArmActive){
                            secondaryDisplay.texture = leftArmSecondaryTexture;
                            currentCameraSecondary = "robot_left_arm";
                            secondaryDropdown.value = 5;
                        } else if(!isLeftArmActive && isRightArmActive){
                            secondaryDisplay.texture = rightArmSecondaryTexture;
                            currentCameraSecondary = "robot_right_arm";
                            secondaryDropdown.value = 6;
                        } else if(isLeftArmActive && isRightArmActive){
                            if(best_workspace_camera.text == "ZED WS Camera"){
                                secondaryDisplay.texture = zedSecondaryTexture;
                                currentCameraSecondary = "zed_ws";
                                secondaryDropdown.value = 1;
                            } else if(best_workspace_camera.text == "Left WS Camera"){
                                secondaryDisplay.texture = leftWSSecondaryTexture;
                                currentCameraSecondary = "left_ws";
                                secondaryDropdown.value = 0;
                            } else if(best_workspace_camera.text == "Right WS Camera"){
                                secondaryDisplay.texture = rightWSSecondaryTexture;
                                currentCameraSecondary = "right_ws";
                                secondaryDropdown.value = 2;
                            }
                        }
                    } 
                } else {
                    if(UserInputPrimaryCamera == "robot_active"){
                        primaryDisplay.texture = activePrimaryTexture;
                        currentCameraPrimary = "robot_active";
                        primaryDropdown.value = 3;
                    } else if(UserInputPrimaryCamera == "zed_ws"){
                        primaryDisplay.texture = zedPrimaryTexture;
                        currentCameraPrimary = "zed_ws";
                        primaryDropdown.value = 1;
                    } else if(UserInputPrimaryCamera == "left_ws"){
                        primaryDisplay.texture = leftWSPrimaryTexture;
                        currentCameraPrimary = "left_ws";
                        primaryDropdown.value = 0;
                    } else if(UserInputPrimaryCamera == "right_ws"){
                        primaryDisplay.texture = rightWSPrimaryTexture;
                        currentCameraPrimary = "right_ws";
                        primaryDropdown.value = 2;
                    } else if(UserInputPrimaryCamera == "robot_chest"){
                        primaryDisplay.texture = chestPrimaryTexture;
                        currentCameraPrimary = "robot_chest";
                        primaryDropdown.value = 4;
                    } else if(UserInputPrimaryCamera == "robot_left_arm"){
                        primaryDisplay.texture = leftArmPrimaryTexture;
                        currentCameraPrimary = "robot_left_arm";
                        primaryDropdown.value = 5;
                    } else if(UserInputPrimaryCamera == "robot_right_arm"){
                        primaryDisplay.texture = rightArmPrimaryTexture;
                        currentCameraPrimary = "robot_right_arm";
                        primaryDropdown.value = 6;
                    }
                    else if(UserInputSecondaryCamera == "robot_active"){
                        secondaryDisplay.texture = activeSecondaryTexture;
                        currentCameraSecondary = "robot_active";
                        secondaryDropdown.value = 3;
                    } else if(UserInputSecondaryCamera == "zed_ws"){
                        secondaryDisplay.texture = zedSecondaryTexture;
                        currentCameraSecondary = "zed_ws";
                        secondaryDropdown.value = 1;
                    } else if(UserInputSecondaryCamera == "left_ws"){
                        secondaryDisplay.texture = leftWSSecondaryTexture;
                        currentCameraSecondary = "left_ws";
                        secondaryDropdown.value = 0;
                    } else if(UserInputSecondaryCamera == "right_ws"){
                        secondaryDisplay.texture = rightWSSecondaryTexture;
                        currentCameraSecondary = "right_ws";
                        secondaryDropdown.value = 2;
                    } else if(UserInputSecondaryCamera == "robot_chest"){
                        secondaryDisplay.texture = chestSecondaryTexture;
                        currentCameraSecondary = "robot_chest";
                        secondaryDropdown.value = 4;
                    } else if(UserInputSecondaryCamera == "robot_left_arm"){
                        secondaryDisplay.texture = leftArmSecondaryTexture;
                        currentCameraSecondary = "robot_left_arm";
                        secondaryDropdown.value = 5;
                    } else if(UserInputSecondaryCamera == "robot_right_arm"){
                        secondaryDisplay.texture = rightArmSecondaryTexture;
                        currentCameraSecondary = "robot_right_arm";
                        secondaryDropdown.value = 6;
                    }
                }
                break;

            default:
                Debug.LogWarning("Unknown state");
                break;
        }
        primaryDropdown.RefreshShownValue(); // Update UI
        secondaryDropdown.RefreshShownValue(); // Update UI
    }

    // Allow the user to switch between cameras as in manual mode
    void OnDropdownValueChanged(TMP_Dropdown dropdown, RawImage targetImage)
    {
        InputUser = true;
        int cameraSelected = dropdown.value;
        string newCamera = "";

        // Ensure targetImage is not null
        if (targetImage == null)
        {
            Debug.LogError("targetImage is null!");
            return;
        }

        // Determine which camera and texture to use based on dropdown selection
        switch (cameraSelected)
        {
            case 0: // Left workspace camera
                newCamera = "left_ws";
                targetImage.texture = (targetImage == primaryDisplay) ? leftWSPrimaryTexture : leftWSSecondaryTexture;
                if(targetImage == primaryDisplay){
                    UserInputPrimaryCamera = "left_ws";
                } else{
                    UserInputSecondaryCamera = "left_ws";
                }
                break;
            case 1: // ZED Mini camera
                newCamera = "zed_ws";
                //zedCamera.targetTexture = (targetImage == primaryDisplay) ? zedPrimaryRenderTexture : zedSecondaryRenderTexture;
                targetImage.texture = (targetImage == primaryDisplay) ? zedPrimaryTexture : zedSecondaryTexture;
                if(targetImage == primaryDisplay){
                    UserInputPrimaryCamera = "zed_ws";
                } else{
                    UserInputSecondaryCamera = "zed_ws";
                }
                break;
            case 2: // Right workspace camera
                newCamera = "right_ws";
                targetImage.texture = (targetImage == primaryDisplay) ? rightWSPrimaryTexture : rightWSSecondaryTexture;
                if(targetImage == primaryDisplay){
                    UserInputPrimaryCamera = "right_ws";
                } else{
                    UserInputSecondaryCamera = "right_ws";
                }
                break;
            case 3: // Robot's Active camera
                newCamera = "robot_active";
                targetImage.texture = (targetImage == primaryDisplay) ? activePrimaryTexture : activeSecondaryTexture;
                if(targetImage == primaryDisplay){
                    UserInputPrimaryCamera = "robot_active";
                } else{
                    UserInputSecondaryCamera = "robot_active";
                }
                break;
            case 4: // Robot's Chest camera
                newCamera = "robot_chest";
                targetImage.texture = (targetImage == primaryDisplay) ? chestPrimaryTexture : chestSecondaryTexture;
                if(targetImage == primaryDisplay){
                    UserInputPrimaryCamera = "robot_chest";
                } else{
                    UserInputSecondaryCamera = "robot_chest";
                }
                break;
            case 5: // Back camera
                newCamera = "robot_left_arm";
                targetImage.texture = (targetImage == primaryDisplay) ? leftArmPrimaryTexture : leftArmSecondaryTexture;
                if(targetImage == primaryDisplay){
                    UserInputPrimaryCamera = "robot_left_arm";
                } else{
                    UserInputSecondaryCamera = "robot_left_arm";
                }
                break;
            case 6: // Right workspace camera
                newCamera = "robot_right_arm";
                targetImage.texture = (targetImage == primaryDisplay) ? rightArmPrimaryTexture : rightArmSecondaryTexture;
                if(targetImage == primaryDisplay){
                    UserInputPrimaryCamera = "robot_right_arm";
                } else{
                    UserInputSecondaryCamera = "robot_right_arm";
                }
                break;
            default:
                Debug.LogError("Invalid camera selection.");
                return;
        }

        string otherCamUsed = (targetImage == primaryDisplay) ? currentCameraSecondary : currentCameraPrimary;

        if(newCamera == otherCamUsed){
            // if the new camera is equal to the one being used, i need to chnage the dropdown value of the other
            TMP_Dropdown otherDropdown = (targetImage == primaryDisplay) ? secondaryDropdown : primaryDropdown;
            int nextCamera = (otherDropdown.value + 1) % 7; // Cycle through the available options 
            otherDropdown.value = nextCamera;
            RawImage otherDisplay = (targetImage == primaryDisplay) ? secondaryDisplay : primaryDisplay;


            if(nextCamera == 0){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? leftWSPrimaryTexture : leftWSSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = "left_ws";
                    UserInputPrimaryCamera = "left_ws";
                } else{
                    currentCameraSecondary = "left_ws";
                    UserInputSecondaryCamera = "left_ws";
                }
            } else if(nextCamera == 1){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? zedPrimaryTexture : zedSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = "zed_ws";
                    UserInputPrimaryCamera = "zed_ws";
                } else{
                    currentCameraSecondary = "zed_ws";
                    UserInputSecondaryCamera = "zed_ws";
                }
            } else if(nextCamera == 2){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? rightWSPrimaryTexture : rightWSSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = "right_ws";
                    UserInputPrimaryCamera = "right_ws";
                } else{
                    currentCameraSecondary = "right_ws";
                    UserInputSecondaryCamera = "right_ws";
                }
            } else if(nextCamera == 3){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? activePrimaryTexture : activeSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = "robot_active";
                    UserInputPrimaryCamera = "robot_active";
                } else{
                    currentCameraSecondary = "robot_active";
                    UserInputSecondaryCamera = "robot_active";
                }
            } else if(nextCamera == 4){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? chestPrimaryTexture : chestSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = "robot_chest";
                    UserInputPrimaryCamera = "robot_chest";
                } else{
                    currentCameraSecondary = "robot_chest";
                    UserInputSecondaryCamera = "robot_chest";
                }
            } else if(nextCamera == 5){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? leftArmPrimaryTexture : leftArmSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = "robot_left_arm";
                    UserInputPrimaryCamera = "robot_left_arm";
                } else{
                    currentCameraSecondary = "robot_left_arm";
                    UserInputSecondaryCamera = "robot_left_arm";
                }
            } else if(nextCamera == 6){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? rightArmPrimaryTexture : rightArmSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = "robot_right_arm";
                    UserInputPrimaryCamera = "robot_right_arm";
                } else{
                    currentCameraSecondary = "robot_right_arm";
                    UserInputSecondaryCamera = "robot_right_arm";
                }
            }
            otherDropdown.RefreshShownValue(); // Update UI
        }   
            
    }
    
}
