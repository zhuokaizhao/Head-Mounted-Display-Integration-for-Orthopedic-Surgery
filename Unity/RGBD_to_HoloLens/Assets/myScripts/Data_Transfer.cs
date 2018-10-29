using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif

public class Data_Transfer : MonoBehaviour
{

    // Input Object used to send
    // public ARCamera sentCamera;

    // self port
    public string port = "12345";

    // external IP and port number
    public string externalIP = "10.161.159.203";
    public string externalPort = "12346";
    private string TAG = "UDPCommunication";

    [System.Serializable]
    public class SendMsg
    {
        public string caller = "NoCaller";
        public int seq = -1;
        public string data = "";
    }

    [System.Serializable]
    public class RecvMsg
    {
        public float x = 0.0f;
        public float y = 0.0f;
        public int seq = 0;
        public int eventCode = 0;
    }

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    // private PupilUDPEventParser EventParser;

#if !UNITY_EDITOR
        DatagramSocket socket;

        // Use this for initialization
        async void Start () {
            Debug.Log("Waiting for a connection");
            socket = new DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;
            HostName IP = null;
            try
            {
                var icp = NetworkInformation.GetInternetConnectionProfile();

                IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().SingleOrDefault(
                    hn =>
                        hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                        == icp.NetworkAdapter.NetworkAdapterId);

                await socket.BindEndpointAsync(IP, port);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Debug.Log(SocketError.GetStatus(e.HResult).ToString());
                return;
            }

            var sendMsg = new SendMsg();
            sendMsg.caller = TAG;
            sendMsg.seq = 0;
            sendMsg.data = "HoloLens UDP server started";
            SendMessageObj(sendMsg);

            Debug.Log("Exit Start");
	    }

        // send to external IP and external port
        private async System.Threading.Tasks.Task SendMessageUDP(string message)
        {
            using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
            {
                using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                {
                    var data = Encoding.UTF8.GetBytes(message);

                    writer.WriteBytes(data);
                    await writer.StoreAsync();
                    Debug.Log("Sent: " + message);
                }
            }
        }

        public async void SendMessageObj(SendMsg sendMsg)
        {
            await SendMessageUDP(JsonUtility.ToJson(sendMsg));
        }

#endif

    // Update is called once per frame
    void Update()
    {
        Action act = null;
        while (ExecuteOnMainThread.Count > 0)
        {
            act = ExecuteOnMainThread.Dequeue();
        }
        if (act != null)
        {
            act.Invoke();
        }
    }

#if !UNITY_EDITOR
    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
    Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        Debug.Log("Received message: ");
        //Read the message that was received from the UDP echo client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        StreamReader reader = new StreamReader(streamIn);
        string message = await reader.ReadLineAsync();

        Debug.Log("Message: " + message);
    }

#endif


}
