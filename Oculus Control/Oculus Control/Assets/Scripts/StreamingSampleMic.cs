using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using TMPro;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using Unity.PlasticSCM.Editor.WebApi;

namespace Whisper.Samples
{
    /// <summary>
    /// Stream transcription from microphone input.
    /// </summary>
    public class StreamingSampleMic : MonoBehaviour
    {
        public RenderTexture zedPrimaryTexture, zedSecondaryTexture;
        public RenderTexture leftWSPrimaryTexture, leftWSSecondaryTexture;
        public RenderTexture rightWSPrimaryTexture, rightWSSecondaryTexture;

        // RenderTextures of the robot's cameras that will be used in the future
        public RenderTexture activePrimaryTexture, activeSecondaryTexture;
        public RenderTexture chestPrimaryTexture, chestSecondaryTexture;
        public RenderTexture leftArmPrimaryTexture, leftArmSecondaryTexture;
        public RenderTexture rightArmPrimaryTexture, rightArmSecondaryTexture;
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public TMP_Text isRecordingText;
        private bool isVocalModeActive = false;
        private List<string> cameras = new List<string>(){"left_ws", "zed_ws", "right_ws", "robot_active", "robot_chest", "robot_left_arm", "robot_right_arm"};
        private string currentCameraPrimary, currentCameraSecondary;
    
        [Header("UI")] 
        public Text text;
        public ScrollRect scroll;
        private WhisperStream _stream;
        public TMP_Dropdown modeDropdown, primaryDropdown, secondaryDropdown;
        public string selectedDisplay = "secondary"; // primary by default
        
        [Header("Raw Images for Displays")]
        public RawImage primaryDisplay;
        public RawImage secondaryDisplay;
        //private string _buffer;
        public RawImage PrimaryBorder, SecondaryBorder;

        private async void Start()
        {
            primaryDropdown.value = 0;
            secondaryDropdown.value = 1;
            currentCameraPrimary = cameras[0];
            currentCameraSecondary = cameras[1];
            primaryDisplay.texture = leftWSPrimaryTexture;
            secondaryDisplay.texture = zedSecondaryTexture;
            try
            {
                if (whisper == null)
                {
                    UnityEngine.Debug.LogError("WhisperManager is not initialized.");
                    return;
                }
                
                if (microphoneRecord == null)
                {
                    UnityEngine.Debug.LogError("MicrophoneRecord is not initialized.");
                    return;
                }
                _stream = await whisper.CreateStream(microphoneRecord);
                if (_stream == null)
                {
                    UnityEngine.Debug.LogError("Whisper stream failed to initialize.");
                    return;
                }
                _stream.OnResultUpdated += OnResult;
                _stream.OnSegmentUpdated += OnSegmentUpdated;
                _stream.OnSegmentFinished += OnSegmentFinished;
                _stream.OnStreamFinished += OnFinished;


                microphoneRecord.useVad = true;
                microphoneRecord.vadStop = true;
                UnityEngine.Debug.Log($"VAD Enabled: {microphoneRecord.useVad}, VAD Stop: {microphoneRecord.vadStop}");
                microphoneRecord.OnRecordStop += OnRecordStop;

                modeDropdown.onValueChanged.AddListener(OnModeChanged);
                OnModeChanged(modeDropdown.value);

                primaryDropdown.onValueChanged.AddListener(delegate{OnDropdownValueChanged(primaryDropdown, primaryDisplay); });
                secondaryDropdown.onValueChanged.AddListener(delegate{OnDropdownValueChanged(secondaryDropdown, secondaryDisplay); });
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error initializing Whisper stream: {ex.Message}");
            }
            
        }
        private void OnModeChanged(int modeIndex){
            if(modeIndex == 1){
               isVocalModeActive = true;
               HighlightSelectedDisplay();
               StartListening();
            } else{
                isVocalModeActive = false;
                StopListening();
            }
        }

        // Update is called once per frame
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
                    //otherDisplay.texture = (otherDisplay == primaryDisplay) ? activePrimaryTexture : activeSecondaryTexture;
                    otherDisplay.texture = null;
                    if(otherDisplay == primaryDisplay){
                        currentCameraPrimary = cameras[3];
                    } else{
                        currentCameraSecondary = cameras[3];
                    }
                } else if(nextCamera == 4){
                    //otherDisplay.texture = (otherDisplay == primaryDisplay) ? chestPrimaryTexture : chestSecondaryTexture;
                    otherDisplay.texture = null;
                    if(otherDisplay == primaryDisplay){
                        currentCameraPrimary =  cameras[4];
                    } else{
                        currentCameraSecondary =  cameras[4];
                    }
                } else if(nextCamera == 5){
                    //otherDisplay.texture = (otherDisplay == primaryDisplay) ? leftArmPrimaryTexture : leftArmSecondaryTexture;
                    otherDisplay.texture = null;
                    if(otherDisplay == primaryDisplay){
                        currentCameraPrimary =  cameras[5];
                    } else{
                        currentCameraSecondary =  cameras[5];
                    }
                } else if(nextCamera == 6){
                    //otherDisplay.texture = (otherDisplay == primaryDisplay) ? rightArmPrimaryTexture : rightArmSecondaryTexture;
                    otherDisplay.texture = null;
                    if(otherDisplay == primaryDisplay){
                        currentCameraPrimary = cameras[6];
                    } else{
                        currentCameraSecondary = cameras[6];
                    }
                }  
                otherDropdown.RefreshShownValue(); // Update UI
            }   
                
        }

        private void StartListening(){
            if(!microphoneRecord.IsRecording){
                _stream.StartStream();
                microphoneRecord.StartRecord();
                UnityEngine.Debug.Log("Start recording...");
                isRecordingText.text = "Recording...";
                isRecordingText.color = Color.green;
            }
        }

        private void StopListening(){
            if(microphoneRecord.IsRecording){
                //_stream.StopStream();
                microphoneRecord.StopRecord();
                UnityEngine.Debug.Log("Stop recording...");
                isRecordingText.text = "Transcribing...";
                isRecordingText.color = Color.red;

                text.text = "Transcribing... When text 'Recording...' appears on the top right, you can start speaking.";
                UiUtils.ScrollDown(scroll);
            }
        }
    
        private void OnRecordStop(AudioChunk recordedAudio)
        {
            if(isVocalModeActive){
                StartListening();
            }
        }
    
        private void OnResult(string result)
        {
            text.text = result;
            UiUtils.ScrollDown(scroll);
        }
        
        private void OnSegmentUpdated(WhisperResult segment)
        {
            print($"Segment updated: {segment.Result}");
            ProcessCommands(segment.Result);
        }
        
        private void OnSegmentFinished(WhisperResult segment)
        {
            print($"Segment finished: {segment.Result}");
            ProcessCommands(segment.Result);
        }
        
        private void OnFinished(string finalResult)
        {
            print("Stream finished!");
            isRecordingText.text = "Transcribing...";
            isRecordingText.color = Color.red;

            text.text = "Transcribing... When text 'Recording...' appears on the top right, you can start speaking.";
            UiUtils.ScrollDown(scroll);
        }

        private void ProcessCommands(string command){
            command = command.ToLower(); // Normalize transcription
            // Remove any unwanted characters (., ..., etc.)
            command = Regex.Replace(command, @"[^\w\s]", "");

            if(command.Contains("select left") || command.Contains("select left display") || command.Contains("select left view") ||
            command.Contains("select the left") || command.Contains("select the left display") || command.Contains("select the left view") ||
            command.Contains("select primary") || command.Contains("select primary display") || command.Contains("select primary view") ||
            command.Contains("select the primary") || command.Contains("select the primary display") || command.Contains("select the primary view")){
                selectedDisplay = "primary";
                UnityEngine.Debug.Log("Primary Display Selected.");
                HighlightSelectedDisplay();
            } else if(command.Contains("select right") || command.Contains("select right display") || command.Contains("select right view") ||
            command.Contains("select the right") || command.Contains("select the right display") || command.Contains("select the right view") ||
            command.Contains("select secondary") || command.Contains("select secondary display") || command.Contains("select secondary view") ||
            command.Contains("select the secondary") || command.Contains("select the secondary display") || command.Contains("select the secondary view")){
                selectedDisplay = "secondary";
                UnityEngine.Debug.Log("Secondary Display Selected.");
                HighlightSelectedDisplay();
            }

            if(command.Contains("left camera") || command.Contains("left cam")){
                SwitchCamera(cameras[0]);
            } else if(command.Contains("right camera") || command.Contains("right cam") || command.Contains("write camera") || command.Contains("write cam")){
                SwitchCamera(cameras[2]);
            } else if(command.Contains("zed camera") || command.Contains("zed cam") || command.Contains("z cam") || command.Contains("zed mini") || command.Contains("set camera") ||
            command.Contains("set cam") ||command.Contains("mini cam") || command.Contains("mini camera") || command.Contains("mini")){
                SwitchCamera(cameras[1]);
            } else if(command.Contains("active camera") || command.Contains("active cam") || command.Contains("active") ||
            command.Contains("robot active camera") || command.Contains("robot active cam") || command.Contains("robot active") ||
            command.Contains("IONA active camera") || command.Contains("IONA active cam") || command.Contains("IONA active")){
                SwitchCamera(cameras[3]);
            } else if(command.Contains("chest camera") || command.Contains("chest cam") || command.Contains("chest") ||
            command.Contains("chast camera") || command.Contains("chast cam") || command.Contains("chast") || command.Contains("test camera") || command.Contains("test cam") || 
            command.Contains("robot chest camera") || command.Contains("robot chest cam") || command.Contains("robot chest") ||
            command.Contains("IONA chest camera") || command.Contains("IONA chest cam") || command.Contains("IONA chest")){
                SwitchCamera(cameras[4]);
            } else if(command.Contains("left arm camera") || command.Contains("left arm cam") || command.Contains("left arm") ||
            command.Contains("robot left arm camera") || command.Contains("robot left arm cam") || command.Contains("robot left arm") ||
            command.Contains("IONA left arm camera") || command.Contains("IONA left arm cam") || command.Contains("IONA left arm")){
                SwitchCamera(cameras[5]);
            } else if(command.Contains("right arm camera") || command.Contains("right arm cam") || command.Contains("right arm") ||
            command.Contains("robot right arm camera") || command.Contains("robot right arm cam") || command.Contains("robot right arm") ||
            command.Contains("IONA right arm camera") || command.Contains("IONA right arm cam") || command.Contains("IONA right arm")){
                SwitchCamera(cameras[6]);
            }


        return;
        }

        private void SwitchCamera(string camera_name){
            UnityEngine.Debug.Log($"Switching {selectedDisplay} to {camera_name}");

            TMP_Dropdown targetDropdown = selectedDisplay == "primary" ? primaryDropdown : secondaryDropdown;

            string otherCamUsed = selectedDisplay == "primary" ? currentCameraSecondary : currentCameraPrimary;

            if (targetDropdown == null)
            {
                UnityEngine.Debug.LogError($"{selectedDisplay}CamDropdown component not found on " + gameObject.name);
                return;
            }

            switch(camera_name){
                case "left_ws":
                    targetDropdown.value = 0;
                    if(selectedDisplay == "primary"){
                        primaryDisplay.texture = leftWSPrimaryTexture;
                        currentCameraPrimary = camera_name;
                    } else if(selectedDisplay == "secondary"){
                        secondaryDisplay.texture = leftWSSecondaryTexture;
                        currentCameraSecondary = camera_name;
                    }
                    break;
                case "zed_ws":
                    targetDropdown.value = 1;
                    if(selectedDisplay == "primary"){
                        primaryDisplay.texture = zedPrimaryTexture;
                        currentCameraPrimary = camera_name;
                    } else if(selectedDisplay == "secondary"){
                        secondaryDisplay.texture = zedSecondaryTexture;
                        currentCameraSecondary = camera_name;
                    }
                    break;
                case "right_ws":
                    targetDropdown.value = 2;
                    if(selectedDisplay == "primary"){
                        primaryDisplay.texture = rightWSPrimaryTexture;
                        currentCameraPrimary = camera_name;
                    } else if(selectedDisplay == "secondary"){
                        secondaryDisplay.texture = rightWSSecondaryTexture;
                        currentCameraSecondary = camera_name;
                    }
                    break;
                case "robot_active":
                    targetDropdown.value = 3;
                    if(selectedDisplay == "primary"){
                        primaryDisplay.texture = activePrimaryTexture;
                        currentCameraPrimary = camera_name;
                    } else if(selectedDisplay == "secondary"){
                        secondaryDisplay.texture = activeSecondaryTexture;
                        currentCameraSecondary = camera_name;
                    }
                    break;
                case "robot_chest":
                    targetDropdown.value = 4;
                    if(selectedDisplay == "primary"){
                        primaryDisplay.texture = chestPrimaryTexture;
                        currentCameraPrimary = camera_name;
                    } else if(selectedDisplay == "secondary"){
                        secondaryDisplay.texture = chestSecondaryTexture;
                        currentCameraSecondary = camera_name;
                    }
                    break;
                case "robot_left_arm":
                    targetDropdown.value = 5;
                    if(selectedDisplay == "primary"){
                        primaryDisplay.texture = leftArmPrimaryTexture;
                        currentCameraPrimary = camera_name;
                    } else if(selectedDisplay == "secondary"){
                        secondaryDisplay.texture = leftArmSecondaryTexture;
                        currentCameraSecondary = camera_name;
                    }
                    break;
                case "robot_right_arm":
                    targetDropdown.value = 6;
                    if(selectedDisplay == "primary"){
                        primaryDisplay.texture = rightArmPrimaryTexture;
                        currentCameraPrimary = camera_name;
                    } else if(selectedDisplay == "secondary"){
                        secondaryDisplay.texture = rightArmSecondaryTexture;
                        currentCameraSecondary = camera_name;
                    }
                    break;
            }
            

            if(otherCamUsed == camera_name){
                // if the new camera is equal to the one being used, i need to chnage the dropdown value of the other
                TMP_Dropdown otherDropdown = selectedDisplay == "primary" ? secondaryDropdown : primaryDropdown;
                int nextCamera = (otherDropdown.value + 1) % 7; // Cycle through the available options (0, 1, 2)
                otherDropdown.value = nextCamera;
                RawImage otherDisplay = selectedDisplay == "primary" ? secondaryDisplay : primaryDisplay;
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

        private void HighlightSelectedDisplay()
        {
            if (PrimaryBorder != null && SecondaryBorder != null)
            {
                if(selectedDisplay == "primary"){
                    PrimaryBorder.color = new Color(0f, 1f, 0f, 0.7f); // Full green with 50% transparency
                    SecondaryBorder.color = Color.white;
                } else if(selectedDisplay == "secondary"){
                    SecondaryBorder.color = new Color(0f, 1f, 0f, 0.7f); // Full green with 50% transparency
                    PrimaryBorder.color = Color.white;
                }
            }
        }
    }
}