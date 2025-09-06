using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using TMPro;
using System.Text;
using UnityEngine.InputSystem.LowLevel;

public class TeleopPhaseStreamer : MonoBehaviour
{
    int port = 8055;
    int receiveBufferSize = 1024 * 1024; // 1MB buffer size
    private UdpClient socket;
    private IPEndPoint remoteEndPoint;
    byte[] receivedBytes;

    public TMP_Text teleop_phase_text;
    private string lastMessage = "";
    private bool isMsgUpdated = false;
    private List<string> phases = new List<string> { "WAITING", "NAVIGATION", "CHEST MANIPULATION", "LEFT ARM MANIPULATION", "RIGHT ARM MANIPULATION"};
    void Start()
    {
        socket = new UdpClient(port);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        socket.Client.ReceiveBufferSize = receiveBufferSize;
        socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, receiveBufferSize);
        //socket.Client.Bind(remoteEndPoint);
        Debug.LogWarning("Listening for teleoperation messages on port " + port);
        //socket.BeginReceive(new AsyncCallback(ReceiveCallback), socket);
        socket.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }
    void Update()
    {
        if (isMsgUpdated)
        {
            teleop_phase_text.text = lastMessage;
            isMsgUpdated = false;  // Reset flag after updating UI
        }
    }

    void ReceiveCallback(IAsyncResult result)
    {
        Debug.LogWarning("result " + result);
        Debug.LogWarning("result socket " + result.AsyncState);
        Debug.LogWarning("result socket " + socket);
        
        try
        {
            receivedBytes = socket.EndReceive(result, ref remoteEndPoint);
            string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
            Debug.Log("Received Message: " + receivedMessage);

            lastMessage = receivedMessage.ToUpper();

            if(!phases.Contains(lastMessage)){
                Debug.LogError("Unknown Teleoperation Phase Error: " + lastMessage + " is not a valid teleoperation phase.");
            }  else{
                isMsgUpdated = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in ReceiveCallback: " + e.Message);
        }

        socket.BeginReceive(new AsyncCallback(ReceiveCallback), socket);
    }
  
    void OnDestroy()
    {
        if (socket != null)
        {
            socket.Close();
        }
    }
}
    // Start is called before the first frame update
//     void Start()
//     {
        
//         remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
//         socket = new UdpClient(new IPEndPoint(IPAddress.Any, port));

//         Debug.LogWarning("Listening for teleoperation messages on port " + port);
//         //socket.BeginReceive(OnUdpDataReceived, null);
//         socket.BeginReceive(new AsyncCallback(OnUdpDataReceived), socket);
//     }
//     void OnUdpDataReceived(IAsyncResult result)
//     {
//         try
//         {
//             byte[] receivedBytes = socket.EndReceive(result, ref remoteEndPoint);
//             string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

//             Debug.LogWarning("Received Teleop Message: " + receivedMessage);
//             lastMessage = receivedMessage.ToUpper();

//             if(!phases.Contains(lastMessage)){
//                 Debug.LogError("Unknown Teleoperation Phase Error: " + lastMessage + " is not a valid teleoperation phase.");
//             }  else{
//                 isMsgUpdated = true;
//             }
//             //socket.BeginReceive(OnUdpDataReceived, null);\
//             socket.BeginReceive(new AsyncCallback(OnUdpDataReceived), socket);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError("UDP Receive Error: " + e.Message);
//         }

//         socket.BeginReceive(new AsyncCallback(OnUdpDataReceived), socket);
//     }
//     void Update()
//     {
//         if (isMsgUpdated)
//         {
//             teleop_phase_text.text = lastMessage;
//             isMsgUpdated = false;  // Reset flag after updating UI
//         }
//     }

//     void OnApplicationQuit()
//     {
//         socket.Close();
//     }
// }
