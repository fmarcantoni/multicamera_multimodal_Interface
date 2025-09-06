using UnityEngine;
using System.Collections;

// attach this scrip to the camera you want to draw your rt from elsewhere
public class Spectator : MonoBehaviour
{
    public RenderTexture rt; // render to this rt from another camera
    public RenderTexture rtBack;

    Camera cam;
    Rect cameraViewRect;
    Rect cameraViewBackRact;

    void Start()
    {
        cam = GetComponent<Camera>();
        //cameraViewRect = new Rect(cam.rect.xMin * Screen.width, Screen.height - cam.rect.yMax * Screen.height, cam.pixelWidth, cam.pixelHeight);
        cameraViewRect = new Rect(0, 0, 3840, 1080);
        cameraViewBackRact = new Rect(1081, (3840 / 2) - (1280 / 2), 1280, 720);
    }

    private void Update()
    {
        cameraViewRect = new Rect(0, 0, 3840, 1080);
        cameraViewBackRact = new Rect((3840 / 2) - (1280 / 2), 1081, 1280, 720);
    }

    // works

    void OnGUI()
    {
        //if (Event.current.type.Equals(EventType.Repaint))
        //    { 
        //Graphics.DrawTexture(rect, rt);
        //      }
    }

    // works, if have ongui method included
    IEnumerator OnPostRender()
    {
        yield return new WaitForEndOfFrame();
        Graphics.DrawTexture(cameraViewRect, rt);
        Graphics.DrawTexture(cameraViewBackRact, rtBack);
    }


}