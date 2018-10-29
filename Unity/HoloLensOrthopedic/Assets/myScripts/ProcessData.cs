using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessData : MonoBehaviour {

    // received start and end position wrt RGBD camera
    public Vector3 RGBDStartPosition3;
    public Vector3 RGBDEndPosition3;
    private bool UseExternalCamera = false;
    private bool UseHoloCalibration = true;

    // Received transformation from Marker to Camera
    public Matrix4x4 MarkerToRGBD;
    public Matrix4x4 MarkerToHOLO;

    // computed results
    public Vector3 HOLOStartPosition;
    public Vector3 HOLOEndPosition;
    public Matrix4x4 RGBDToHOLO;

    // private variables
    private GameObject DataTransfer;
    private GameObject ARUWPController;

    // Line the text
    private Text StartPosText;
    private Text EndPosText;

    // Use this for initialization
    void Start () {
        // don't really need to initialize anything
	}
	
	// Update is called once per frame
	void Update () {
        if(UseExternalCamera)
        {
            //********************* Initial Method that takes everything form RGBD Camera *************************************
            // link the received data
            DataTransfer = GameObject.Find("DataTransfer");
            ReceiveData receivedData = DataTransfer.GetComponent<ReceiveData>();

            // Obtain the received data, which are start and end positions wrt RGBD camera
            RGBDStartPosition3 = receivedData.StartPosition;
            RGBDEndPosition3 = receivedData.EndPosition;
            MarkerToRGBD = receivedData.MarkerToCamera;
            //Debug.Log("Start Position wrt RGBD is: \n" + RGBDStartPosition3.x.ToString() + " " + RGBDStartPosition3.y.ToString() + " " + RGBDStartPosition3.z.ToString());
            //Debug.Log("End Position wrt RGBD is: \n" + RGBDEndPosition3.x.ToString() + " " + RGBDEndPosition3.y.ToString() + " " + RGBDEndPosition3.z.ToString());

            // link the transformation obtained within HoloLens
            ARUWPController = GameObject.Find("ARUWP Controller");
            ARUWPMarker trackedMarker = ARUWPController.GetComponent<ARUWPMarker>();
            // Obtain the tracking done by HoloLens
            MarkerToHOLO = trackedMarker.transMatrix;
            //Debug.Log("Transformation from Marker to HoloLens is: \n" + MarkerToHOLO.ToString());
            
            // Now we've collected what we need
            // Calculate the transformation from RGBD to HoloLens - Note that it is To_T_From
            // HOLO_T_RGBD = HOLO_T_MARKER * MARKER_T_RGBD = MarkerToHolo * RGBDToMarker = MarkerToHOLO * MarkerToRGBD.inverse
            RGBDToHOLO = MarkerToHOLO * MarkerToRGBD.inverse;

            // Then calculate the positions from wrt RGBD to wrt HoloLens
            Vector4 RGBDStartPosition4 = new Vector4(RGBDStartPosition3.x, RGBDStartPosition3.y, RGBDStartPosition3.z, 1.0f);
            Vector4 RGBDEndPosition4 = new Vector4(RGBDEndPosition3.x, RGBDEndPosition3.y, RGBDEndPosition3.z, 1.0f);
            Vector4 HOLOStartPosition4 = RGBDToHOLO * RGBDStartPosition4;
            Vector4 HOLOEndPosition4 = RGBDToHOLO * RGBDEndPosition4;
            HOLOStartPosition = new Vector3(HOLOStartPosition4.x, HOLOStartPosition4.y, HOLOStartPosition4.z);
            HOLOEndPosition = new Vector3(HOLOEndPosition4.x, HOLOEndPosition4.y, HOLOEndPosition4.z);

            // Use HoloCalib to decrease the Holographic display offsets
            if (UseHoloCalibration)
            {
                HOLOStartPosition = HoloCalib(HOLOStartPosition);
                HOLOEndPosition = HoloCalib(HOLOEndPosition);
            }

            // Now we transfer the position from wrt to HoloLens to wrt world (local to world)
            GameObject MainCamera = GameObject.Find("Main Camera");
            Camera myCamera = MainCamera.GetComponent<Camera>();
            HOLOStartPosition = myCamera.transform.TransformPoint(HOLOStartPosition);
            HOLOEndPosition = myCamera.transform.TransformPoint(HOLOEndPosition);

            //Debug.Log("Start Position wrt HoloLens is: \n" + HOLOStartPosition.x.ToString() + " " + HOLOStartPosition.y.ToString() + " " + HOLOStartPosition.z.ToString());
            //Debug.Log("End Position wrt HoloLens is: \n" + HOLOEndPosition.x.ToString() + " " + HOLOEndPosition.y.ToString() + " " + HOLOEndPosition.z.ToString());

            // Now work on linking the text
            StartPosText = GameObject.Find("/ARDisplayHUD/HOLOStartPos").GetComponent<Text>();
            EndPosText = GameObject.Find("/ARDisplayHUD/HOLOEndPos").GetComponent<Text>();
            StartPosText.text = "Start Position: " + "[" + HOLOStartPosition.x.ToString() + ", " + HOLOStartPosition.y.ToString() + ", " + HOLOStartPosition.z.ToString() + "]";
            EndPosText.text = "End Position: " + "[" + HOLOEndPosition.x.ToString() + " " + HOLOEndPosition.y.ToString() + " " + HOLOEndPosition.z.ToString() + "]";
            
            //************************ Initial Method ends here ***************************************************************
        }
        else
        {
            // *********************** Alternative method - calculate the tracking by itself *************************************
            // Local end position (tool tip) wrt to the marker, obtained from pivot caibration
            Vector3 localEnd;
            // below is for the pen model pivot calibration result
            // localEnd.x = (float)(-3.276183 / 1000 /*+ 0.013*/);
            // localEnd.y = (float)(-187.381409 / 1000 /*- 0.02*/);
            // localEnd.z = (float)(2.641751 / 1000 /*- 0.035*/);
            // Vector3 localStart;
            // localStart.x = localEnd.x;
            // localStart.y = (float)(localEnd.y + 0.12);
            // localStart.z = localEnd.z;

            // below is for the needle model pivot calibration result
            localEnd.x = (float)(-4.7846 / 1000);
            localEnd.y = (float)(-110.211241 / 1000);
            localEnd.z = (float)(0.024813 / 1000);

            // get the local start position
            Vector3 localStart;
            localStart.x = localEnd.x;
            localStart.y = (float)(localEnd.y + 0.08);
            localStart.z = localEnd.z;

            // get the target from ARUWPMarker.cs
            ARUWPController = GameObject.Find("ARUWP Controller");
            ARUWPMarker trackedMarker = ARUWPController.GetComponent<ARUWPMarker>();
            if (trackedMarker.visible)
            {
                //Debug.Log("The marker is visible");
                
                // get the transformation from ARUWPMarker, which is the transformation from Marker to HoloLens
                Matrix4x4 MarkerToHOLO = trackedMarker.transMatrix;
                
                // transfer both local start and local end to HoloLens coordinate
                Vector4 localStart4 = new Vector4(localStart.x, localStart.y, localStart.z, 1.0f);
                Vector4 localEnd4 = new Vector4(localEnd.x, localEnd.y, localEnd.z, 1.0f);
                Vector4 HoloStart4 = MarkerToHOLO * localStart4;
                Vector4 HoloEnd4 = MarkerToHOLO * localEnd4;

                // save to a Vector3
                HOLOStartPosition = new Vector3(HoloStart4.x, HoloStart4.y, HoloStart4.z);
                HOLOEndPosition = new Vector3(HoloEnd4.x, HoloEnd4.y, HoloEnd4.z);

                // Use HoloCalib to decrease the Holographic display offsets
                if (UseHoloCalibration)
                {
                    HOLOStartPosition = HoloCalib(HOLOStartPosition);
                    HOLOEndPosition = HoloCalib(HOLOEndPosition);
                }

                // Now we transfer the position from wrt to HoloLens to wrt world (Holo to world)
                GameObject MainCamera = GameObject.Find("Main Camera");
                Camera myCamera = MainCamera.GetComponent<Camera>();
                HOLOStartPosition = myCamera.transform.TransformPoint(HOLOStartPosition);
                HOLOEndPosition = myCamera.transform.TransformPoint(HOLOEndPosition);

                //Debug.Log("Transformation from Marker to HoloLens is: \n" + MarkerToHOLO.ToString());
                //Debug.Log("Start Position wrt HoloLens is: \n" + HOLOStartPosition.x.ToString() + " " + HOLOStartPosition.y.ToString() + " " + HOLOStartPosition.z.ToString());
                //Debug.Log("End Position wrt HoloLens is: \n" + HOLOEndPosition.x.ToString() + " " + HOLOEndPosition.y.ToString() + " " + HOLOEndPosition.z.ToString());

                // Now work on linking the text
                StartPosText = GameObject.Find("/ARDisplayHUD/HOLOStartPos").GetComponent<Text>();
                EndPosText = GameObject.Find("/ARDisplayHUD/HOLOEndPos").GetComponent<Text>();
                StartPosText.text = "Start Position: " + "[" + HOLOStartPosition.x.ToString() + ", " + HOLOStartPosition.y.ToString() + ", " + HOLOStartPosition.z.ToString() + "]";
                EndPosText.text = "End Position: " + "[" + HOLOEndPosition.x.ToString() + " " + HOLOEndPosition.y.ToString() + " " + HOLOEndPosition.z.ToString() + "]";

            }
        }

    }

    private Vector3 HoloCalib(Vector3 inputVector3)
    {
        // below is the result I got from HoloCalib using Affine
        Vector4 firstRow = new Vector4(1.0600f, 0.0348f, -0.0356f, 0.0029f);
        Vector4 secondRow = new Vector4(0.0023f, 1.0561f, -0.1177f, 0.0150f);
        Vector4 thirdRow = new Vector4(-0.1256f, -0.0994f, 1.0899f, 0.0737f);
        Vector4 fourthRow = new Vector4(0.0000f, 0.0000f, 0.0000f, 1.0000f);

        // set up the 4x4 matrix
        Matrix4x4 CalibrationMatrix = new Matrix4x4();
        CalibrationMatrix.SetRow(0, firstRow);
        CalibrationMatrix.SetRow(1, secondRow);
        CalibrationMatrix.SetRow(2, thirdRow);
        CalibrationMatrix.SetRow(3, fourthRow);

        // convert the input matrix to homogeneous
        Vector4 inputVector4 = new Vector4(inputVector3.x, inputVector3.y, inputVector3.z, 1.0f);

        // calculate the output vector
        Vector4 outputVector4 = CalibrationMatrix * inputVector4;
        Vector3 outputVector3 = new Vector3(outputVector4.x, outputVector4.y, outputVector4.z);

        return outputVector3;
    }
}
