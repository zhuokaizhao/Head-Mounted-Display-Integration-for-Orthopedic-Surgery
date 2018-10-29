using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
// using HoloToolkit.Unity;
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

/// <summary>
/// UDPCommunication tries to receive UDP packets
/// The script can run as part of an UWP app running on HoloLens.
/// See: https://www.hackster.io/team-constructar/constructar-the-holographic-tool-belt-b44698#toc-hololens-6
/// It can also run with Holographic Remoting on a remote system that is running the Unity editor.
/// See: http://stackoverflow.com/questions/37131742/how-to-use-udp-with-unity-methods/37131831#37131831
/// </summary>
public class TransferPose : MonoBehaviour
{
    //public GameObject receiverObject;
    //private ObjectParse objectParse;

    public int myPort = 12345;
    public string TargetHololensIP = "10.189.59.165";

#if UNITY_EDITOR || UNITY_STANDALONE
    static UdpClient udp;
    Thread thread;
    static readonly object lockObject = new object();

    // string returnData = "";
    bool processData = false;

    // Use this for initialization
    void Start()
    {
        //objectParse = receiverObject.GetComponent<ObjectParse>();

        udp = new UdpClient(myPort);
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (processData)
        {
            lock (lockObject)
            {
                processData = false;

                // Debug.Log("Received: " + returnData);
                //objectParse.parse(returnData);

                // returnData = "";
            }
        }
    }

    private void OnApplicationQuit()
    {
        udp.Close();
        thread.Abort();
    }

    private void ThreadMethod()
    {
        while (true)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);

            lock (lockObject)
            {
                // returnData = Encoding.ASCII.GetString(receiveBytes);
                processData = true;
            }
        }
    }

    public void Send(Byte[] payload)
    {
        Debug.Log("Sending bytes...");
        //IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, );
        Debug.Log("payload: " + payload.Length);
        udp.Send(payload, payload.Length, "127.0.0.1", 12345);
    }
#endif


#if !UNITY_EDITOR && UNITY_METRO
    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    DatagramSocket socket;

    // use this for initialization
    async void Start()
    {
        //objectParse = receiverObject.GetComponent<ObjectParse>();

        Debug.Log("Waiting for a connection...");

        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        HostName IP = null;
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

        var message = "hello from " + IP;
 //       await SendMessage(message);
 //       await SendMessage("hello");

        Debug.Log("exit start");
    }

//    private async System.Threading.Tasks.Task SendMessage(string message)
//    {
//        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
//        {
//            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
//            {
//                var data = Encoding.UTF8.GetBytes(message);
//
//                writer.WriteBytes(data);
//                await writer.StoreAsync();
//                Debug.Log("Sent: " + message);
//            }
//        }
//    }

    // Update is called once per frame
    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        Debug.Log("GOT MESSAGE: ");
        //Read the message that was received from the UDP echo client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        StreamReader reader = new StreamReader(streamIn);
        string message = await reader.ReadLineAsync();

        Debug.Log("Received: " + message);

        //if (ExecuteOnMainThread.Count == 0)
        //{
        //    ExecuteOnMainThread.Enqueue(() =>
        //    {
        //        objectParse.parse(message);
        //    });
        //}
    }
#endif
}
