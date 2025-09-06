using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;
using TMPro;
using Unity.VisualScripting;

public class UnityTcpConnection : MonoBehaviour
{
    public StateMachine stateMachine;
    public RenderTexture zedPrimaryTexture, zedSecondaryTexture;
    public TMP_Text most_relevant_cam;
    private TcpListener tcpListener;
    private TcpClient client;
    private NetworkStream stream;
    private BinaryReader reader;
    private Thread serverThread;
    private byte[] imageBuffer;
    private int imageLength = 0;
    private Texture2D tex, tex2;
    private const int port = 51420;  // Port number to listen on
    private bool isRunning = true;
    private object queueLock = new object();
    private string receivedMessage = null;

    private ConcurrentQueue<byte[]> imageQueue = new ConcurrentQueue<byte[]>(); // Thread-safe queue

    void Start()
    {
        tex = new Texture2D(zedPrimaryTexture.width, zedPrimaryTexture.height, TextureFormat.RGB24, false);
        serverThread = new Thread(ListenForConnections);
        serverThread.IsBackground = true;
        serverThread.Start();
    }
    void Update()
    {
        // Process images on the main thread
        if (imageQueue.TryDequeue(out byte[] imageBytes))
        {
            tex.LoadImage(imageBytes);
            Graphics.Blit(tex, zedPrimaryTexture);

            tex2 = new Texture2D(zedSecondaryTexture.width, zedSecondaryTexture.height, TextureFormat.RGB24, false);
            tex2.LoadImage(imageBytes);
            Graphics.Blit(tex2, zedSecondaryTexture);
        }
        // Process received camera selection message
        lock (queueLock)
        {
            if (receivedMessage != null)
            {
                Debug.Log($"Received Camera Selection: {receivedMessage}");
                if(receivedMessage == "zed_camera"){
                    most_relevant_cam.text = "ZED WS Camera";
                    stateMachine.StateChange();
                } else if(receivedMessage == "left_camera"){
                    most_relevant_cam.text = "Left WS Camera";
                    stateMachine.StateChange();
                } else if(receivedMessage == "right_camera"){
                    most_relevant_cam.text = "Right WS Camera";
                    stateMachine.StateChange();
                } 
                receivedMessage = null; // Reset after handling
            }
        }
    }
    
    void ListenForConnections()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Debug.Log($"Server started on port {port}");

            while (isRunning)
            {
                using (TcpClient client = tcpListener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    Debug.Log("Client connected!");

                    // Read the message type identifier (first 3 bytes: "MSG" or "IMG")
                    byte[] typeBytes = reader.ReadBytes(3);
                    string messageType = Encoding.ASCII.GetString(typeBytes);

                    if(messageType == "MSG"){
                        int stringLength = reader.ReadInt32(); // Read the length of the string
                        byte[] stringBytes = reader.ReadBytes(stringLength);
                        string message = Encoding.ASCII.GetString(stringBytes);
                        Debug.Log($"Received message: {receivedMessage}");
                        lock (queueLock)
                        {
                            receivedMessage = message; // Store it for main thread processing
                        }

                        // Handle the zed image
                    } else if(messageType == "IMG"){
                        // Read image length
                        byte[] lengthBytes = reader.ReadBytes(4);
                        if (lengthBytes.Length < 4)
                        {
                            Debug.LogError("Received invalid image length.");
                            continue;
                        }

                        int length = BitConverter.ToInt32(lengthBytes, 0);
                        Debug.Log($"Received image length: {length}");

                        if (length <= 0)
                        {
                            Debug.LogError($"Invalid image length: {length}");
                            continue;
                        }

                        // Read image data
                        byte[] imageBytes = reader.ReadBytes(length);
                        if (imageBytes.Length != length)
                        {
                            Debug.LogError($"Image data size mismatch. Expected {length}, but received {imageBytes.Length}.");
                            continue;
                        }

                        // Add image to queue for main thread processing
                        imageQueue.Enqueue(imageBytes);
                    } else{
                        Debug.LogError("Received unknown message type.");
                    }
                    
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in TCP server: {e.Message}");
        }
    }
    
    void OnApplicationQuit()
    {
        // Ensure to close all connections when the application quits
        if (client != null)
        {
            client.Close();
        }
        if (tcpListener != null)
        {
            tcpListener.Stop();
        }
    }

}
