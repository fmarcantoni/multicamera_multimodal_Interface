using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UDPLeftHandler : MonoBehaviour
{
    public RenderTexture primaryRT; // Assign in Inspector
    public int listenPort = 8080;
    public int textureWidth = 640;
    public int textureHeight = 480;

    private UdpClient udpClient;
    private Thread receiveThread;
    private Dictionary<int, byte[]> chunks = new();
    private int expectedChunks = -1;
    private int lastFrameNumber = -1;
    private object lockObject = new();

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveLoop()
    {
        while (true)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] data = udpClient.Receive(ref remoteEndPoint);

                // Parse header
                int headerEnd = Array.IndexOf(data, (byte)1); // Delimiter
                string header = System.Text.Encoding.UTF8.GetString(data, 0, headerEnd);
                string[] parts = header.Split('_');

                int frame = int.Parse(parts[0]);
                int chunk = int.Parse(parts[1]);
                int totalChunks = int.Parse(parts[2]);
                int chunkIndex = int.Parse(parts[3]);
                int chunkSize = int.Parse(parts[4]);
                int totalSize = int.Parse(parts[5]);

                byte[] imageData = new byte[chunkSize];
                Buffer.BlockCopy(data, headerEnd + 1, imageData, 0, chunkSize);

                lock (lockObject)
                {
                    if (frame != lastFrameNumber)
                    {
                        chunks.Clear();
                        expectedChunks = totalChunks;
                        lastFrameNumber = frame;
                    }

                    chunks[chunk] = imageData;

                    if (chunks.Count == expectedChunks)
                    {
                        List<byte> fullImage = new List<byte>();
                        for (int i = 0; i < expectedChunks; i++)
                        {
                            fullImage.AddRange(chunks[i]);
                        }

                        chunks.Clear();
                        byte[] jpegData = fullImage.ToArray();
                        UpdateTexture(jpegData);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("UDP Receive error: " + e.Message);
            }
        }
    }

    void UpdateTexture(byte[] imageBytes)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
            tex.LoadImage(imageBytes);
            Graphics.Blit(tex, primaryRT);
        });
    }

    void OnApplicationQuit()
    {
        receiveThread?.Abort();
        udpClient?.Close();
    }
}
