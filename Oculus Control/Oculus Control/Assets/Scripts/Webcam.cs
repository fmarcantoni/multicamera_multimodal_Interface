using UnityEngine;

public class Webcam : MonoBehaviour {
    //Numeric ID for USB  webcam device
    public int deviceId = 0;
    //Input webcam dimensions
    public int inputWidth = 4096;
    public int inputHeight = 2048;
    public int maxWidth = 4096;
    public int maxHeught = 2048;
    //Render texture to write webcam image to
    public RenderTexture outputTexture;
    private WebCamTexture webcamTexture;
    //Buffer texture to be used if the input image is not a 360 resolution
    private Texture2D bufferTexture;
    private int xOffset;
    private int yOffset;
    private bool needsBuffer;
	void Start () {
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(3840, 1080);
        webcamTexture.requestedWidth = inputWidth;
        webcamTexture.requestedHeight = inputHeight;
        bufferTexture = new Texture2D(maxWidth, maxHeught);

        //Only use buffer texture if resolutions dont match
        needsBuffer = !(inputWidth== maxWidth && inputHeight== maxHeught);

        //Place image in the center of the output texture
        if (needsBuffer) {
            xOffset = (maxWidth - webcamTexture.width) /2;
            yOffset = (maxHeught - webcamTexture.height) /2;
        }
        
        webcamTexture.deviceName = devices[deviceId].name;
        webcamTexture.Play();
        Debug.Log(webcamTexture.width);
    }

    void Update() {
        if (webcamTexture.width < 100)
        {
            Debug.Log("Waiting for webcam to spin up");
            return;
        }
        //Copy webcam texture to render texture output
        if (needsBuffer) {
            xOffset = (maxWidth - webcamTexture.width) / 2;
            yOffset = (maxHeught - webcamTexture.height) / 2;
            Graphics.CopyTexture(webcamTexture, 0, 0, 0, 0, webcamTexture.width, webcamTexture.height, bufferTexture, 0, 0, xOffset, yOffset);
            Graphics.Blit(bufferTexture, outputTexture);
        } else {
            Graphics.Blit(webcamTexture, outputTexture);
        }
        
    }
}
