// Zhuokai Zhao
// Learn from the link below
// [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]

using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPSend : MonoBehaviour
{
    // private static int localPort;

    // inputs from user in Unity
    public string IP;  
    public int port;
    public Vector3 SentStartPosition;
    public Vector3 SentEndPosition;
    public Matrix4x4 MarkerToCamera;

    // connection things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    public void Start()
    {
        // initialization
        print("Initializing connection...\n");
        // use the user defined IP and port
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

        // status
        print("Sending to " + IP + ":" + port + "\n");
    }

    public void Update()
    {
        // Find the Camera object
        GameObject Camera = GameObject.Find("Calibration/Scene Root/Camera");
        // access the ARCamera script of Camera
        ARCamera arCamera = Camera.GetComponent<ARCamera>();
        // find the transformation from marker to camera
        MarkerToCamera = arCamera.myPose;

        // Set the input positions for start and end
        GameObject AugmentLine = GameObject.Find("Calibration/Scene Root/Marker/Augment Line");
        LineControl line = AugmentLine.GetComponent<LineControl>();

        // the input end position is the position of needle tip wrt marker
        float inputEndX = line.endPosition.x;
        float inputEndY = line.endPosition.y;
        float inputEndZ = line.endPosition.z;
        // the input start position is the start position of needle tip wrt marker
        float inputStartX = line.startPosition.x;
        float inputStartY = line.startPosition.y;
        float inputStartZ = line.startPosition.z;

        // Now we need to convert these positions to be wrt RGBD camera
        Vector4 inputStart = new Vector4(inputStartX, inputStartY, inputStartZ, 1.0f);
        Vector4 inputEnd = new Vector4(inputEndX, inputEndY, inputEndZ, 1.0f);

        Vector4 StartPosition4 = MarkerToCamera * inputStart;
        Vector4 EndPosition4 = MarkerToCamera * inputEnd;
        SentStartPosition = new Vector3(StartPosition4.x, StartPosition4.y, StartPosition4.z);
        SentEndPosition = new Vector3(EndPosition4.x, EndPosition4.y, EndPosition4.z);

        // print("Sent End Position is defined.\n");
        // print("Sent Start Position is defined.\n");
        string sendMessage = makeMessage(SentStartPosition, SentEndPosition, MarkerToCamera);
        
        print(sendMessage + "\n");

        // send the message
        sendString(sendMessage);
    }

    // helper function that make the send message
    string makeMessage(Vector3 StartPosition, Vector3 EndPosition, Matrix4x4 MarkerToCamera)
    {
        // convert the vector to string
        string startX = SentStartPosition.x.ToString();
        string startY = SentStartPosition.y.ToString();
        string startZ = SentStartPosition.z.ToString();

        string endX = SentEndPosition.x.ToString();
        string endY = SentEndPosition.y.ToString();
        string endZ = SentEndPosition.z.ToString();

        // We also want to send the transformation
        string matrix = " ";
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                matrix = matrix + MarkerToCamera[i, j].ToString() + " ";
            }
        }

        // now combine the two
        string sendMessage = startX + " " + startY + " " + startZ + " " + endX + " " + endY + " " + endZ + matrix;

        return sendMessage;
    }

    // sendData
    private void sendString(string message)
    {
        try
        {
            // convert the message to byte type
            byte[] data = Encoding.UTF8.GetBytes(message);

            // send
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

}
