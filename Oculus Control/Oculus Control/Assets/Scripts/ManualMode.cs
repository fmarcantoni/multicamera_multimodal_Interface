using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ManualMode : MonoBehaviour
{
   private List<string> cameras = new List<string>(){"left_ws", "zed_ws", "right_ws", "robot_active", "robot_chest", "robot_left_arm", "robot_right_arm"};
    private string currentCameraPrimary, currentCameraSecondary;
    public RenderTexture zedPrimaryTexture;
    public RenderTexture zedSecondaryTexture;
    public RenderTexture leftWSPrimaryTexture;
    public RenderTexture leftWSSecondaryTexture;
    public RenderTexture rightWSPrimaryTexture;
    public RenderTexture rightWSSecondaryTexture;

    // RenderTextures of the robot's cameras that will be used in the future
    public RenderTexture activePrimaryTexture, activeSecondaryTexture;
    public RenderTexture chestPrimaryTexture, chestSecondaryTexture;
    public RenderTexture leftArmPrimaryTexture, leftArmSecondaryTexture;
    public RenderTexture rightArmPrimaryTexture, rightArmSecondaryTexture;
    public RawImage primaryDisplay;
    public RawImage secondaryDisplay;
    public TMP_Dropdown primaryDropdown;
    public TMP_Dropdown secondaryDropdown;

    void Start()
    {
        primaryDropdown.value = 0;
        secondaryDropdown.value = 1;
        currentCameraPrimary = cameras[0];
        currentCameraSecondary = cameras[1];
        primaryDisplay.texture = leftWSPrimaryTexture;
        secondaryDisplay.texture = zedSecondaryTexture;
        
        primaryDropdown.onValueChanged.AddListener(delegate{OnDropdownValueChanged(primaryDropdown, primaryDisplay); });
        secondaryDropdown.onValueChanged.AddListener(delegate{OnDropdownValueChanged(secondaryDropdown, secondaryDisplay); });
    }

    // // Update is called once per frame
    void OnDropdownValueChanged(TMP_Dropdown dropdown, RawImage targetImage)
    {
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
                newCamera = cameras[cameraSelected];
                targetImage.texture = (targetImage == primaryDisplay) ? leftWSPrimaryTexture : leftWSSecondaryTexture;
                break;
            case 1: // ZED Mini camera
                newCamera = cameras[cameraSelected];
                targetImage.texture = (targetImage == primaryDisplay) ? zedPrimaryTexture : zedSecondaryTexture;
                break;
            case 2: // Right workspace camera
                newCamera = cameras[cameraSelected];
                targetImage.texture = (targetImage == primaryDisplay) ? rightWSPrimaryTexture : rightWSSecondaryTexture;
                break;
            case 3: // Robot's active camera
                newCamera = cameras[cameraSelected];
                targetImage.texture = (targetImage == primaryDisplay) ? activePrimaryTexture : activeSecondaryTexture;
                break;
            case 4: // Robot's chest camera
                newCamera = cameras[cameraSelected];
                targetImage.texture = (targetImage == primaryDisplay) ? chestPrimaryTexture : chestSecondaryTexture;
                break;
            case 5: // Robot's left arm camera
                newCamera = cameras[cameraSelected];
                targetImage.texture = (targetImage == primaryDisplay) ? leftArmPrimaryTexture : leftArmSecondaryTexture;
                break;
            case 6: // Robot's right arm camera
                newCamera = cameras[cameraSelected];
                targetImage.texture = (targetImage == primaryDisplay) ? rightArmPrimaryTexture : rightArmSecondaryTexture;
                break;
            default:
                Debug.LogError("Invalid camera selection.");
                return;
        }

        // Ensure that if we are switching to a camera, 
        if(targetImage == primaryDisplay){
            currentCameraPrimary = newCamera;
        } else{
            currentCameraSecondary = newCamera;
        }
        string otherCamUsed = (targetImage == primaryDisplay) ? currentCameraSecondary : currentCameraPrimary;

        if(newCamera == otherCamUsed){
            // if the new camera is equal to the one being used, i need to chnage the dropdown value of the other
            TMP_Dropdown otherDropdown = (targetImage == primaryDisplay) ? secondaryDropdown : primaryDropdown;
            int nextCamera = (otherDropdown.value + 1) % 7; // Cycle through the available options (0, 1, 2)
            otherDropdown.value = nextCamera;
            RawImage otherDisplay = (targetImage == primaryDisplay) ? secondaryDisplay : primaryDisplay;
            if(nextCamera == 0){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? leftWSPrimaryTexture : leftWSSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = cameras[0];
                } else{
                    currentCameraSecondary = cameras[0];
                }
            } else if(nextCamera == 1){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? zedPrimaryTexture : zedSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = cameras[1];
                } else{
                    currentCameraSecondary = cameras[1];
                }
            } else if(nextCamera == 2){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? rightWSPrimaryTexture : rightWSSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = cameras[2];
                } else{
                    currentCameraSecondary = cameras[2];
                }
            } else if(nextCamera == 3){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? activePrimaryTexture : activeSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = cameras[3];
                } else{
                    currentCameraSecondary = cameras[3];
                }
            } else if(nextCamera == 4){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? chestPrimaryTexture : chestSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary =  cameras[4];
                } else{
                    currentCameraSecondary =  cameras[4];
                }
            } else if(nextCamera == 5){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? leftArmPrimaryTexture : leftArmSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary =  cameras[5];
                } else{
                    currentCameraSecondary =  cameras[5];
                }
            } else if(nextCamera == 6){
                otherDisplay.texture = (otherDisplay == primaryDisplay) ? rightArmPrimaryTexture : rightArmSecondaryTexture;
                if(otherDisplay == primaryDisplay){
                    currentCameraPrimary = cameras[6];
                } else{
                    currentCameraSecondary = cameras[6];
                }
            }  
            otherDropdown.RefreshShownValue(); // Update UI
        }   
            
    }
}
