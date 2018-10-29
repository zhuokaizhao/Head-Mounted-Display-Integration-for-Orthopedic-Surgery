using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif
#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif

public class ReceiveData : MonoBehaviour
{
    // user defined port
    public int myPort = 12345;

    // Below are the information I received/wanted, also are the inputs for other computations done within HoloLens
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public Matrix4x4 MarkerToCamera;

    // public Vector2 aaa;


    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();
#if !UNITY_EDITOR && UNITY_METRO
    DatagramSocket socket;
#endif
    // use this for initialization
    public void Start()
    {
#if !UNITY_EDITOR && UNITY_METRO
        Initialize();
#endif
    }

#if !UNITY_EDITOR && UNITY_METRO
    async void Initialize()
    {

        // indicate the start
        Debug.Log("Waiting for a connection...");

        // initialize the socket
        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        // initialize the host IP
        HostName IP = null;

        // get the sender(host)'s IP
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
            .SingleOrDefault(
                hn =>
                    hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                    == icp.NetworkAdapter.NetworkAdapterId);

            await socket.BindEndpointAsync(IP, myPort.ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }
        
        // make a message
        var message = "hello from " + IP;

        // exit the initialization
        Debug.Log("exit start");

}
#endif

    // Update is called once per frame
    void Update()
    {
#if !UNITY_EDITOR && UNITY_METRO
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
#endif
    }

#if !UNITY_EDITOR && UNITY_METRO
    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        // Read the message that was received from the UDP echo client.
        // Initialize the streaming
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        StreamReader reader = new StreamReader(streamIn);
        string message = await reader.ReadLineAsync();
        
        // parse the input string
        char[] delimiterChars = { ' ' };
        string[] words = message.Split(delimiterChars);

        // the first 3 numbers received is the start position
        StartPosition.x = float.Parse(words[0]);
        StartPosition.y = float.Parse(words[1]);
        StartPosition.z = float.Parse(words[2]);

        // the second 3 numbers received is the end position
        EndPosition.x = float.Parse(words[3]);
        EndPosition.y = float.Parse(words[4]);
        EndPosition.z = float.Parse(words[5]);

        // the rest of numbers are transformation matrix from marker to RGBD Camera
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                MarkerToCamera[i, j] = float.Parse(words[i*4 + j + 6]);
            }
        }

        // convert the Vectors to string for verify
        string startPos = StartPosition.ToString(); 
        string endPos = EndPosition.ToString();
        string T = MarkerToCamera.ToString();

        // show the received message on log
        // Debug.Log("Received: \n" + message);
        // Debug.Log("Processed Start Position: \n" + startPos);
        // Debug.Log("Processed End Position: \n" + endPos);
        // Debug.Log("Processed Matrix: \n" + T);
    }
#endif

}
