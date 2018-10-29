/*
 *  ARCamera.cs
 *  ARToolKit for Unity
 *
 *  This file is part of ARToolKit for Unity.
 *
 *  ARToolKit for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ARToolKit for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with ARToolKit for Unity.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Philip Lamb, Julian Looser
 *
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class which links an ARCamera to any available ARMarker via an AROrigin object.
/// 
/// To get a list of foreground Camera objects, do:
///
///     List<Camera> foregroundCameras = new List<Camera>();
///     ARCamera[] arCameras = FindObjectsOfType<ARCamera>(); // (or FindObjectsOfType(typeof(ARCamera)) as ARCamera[])
///     foreach (ARCamera arc in arCameras) {
///         foregroundCameras.Add(arc.gameObject.camera);
///     }
/// </summary>
/// 
[RequireComponent(typeof(Transform))]   // A Transform is required to update the position and orientation from tracking
[ExecuteInEditMode]                     // Run in the editor so we can keep the scale at 1
public class ARCamera : MonoBehaviour
{
	private const string LogTag = "ARCamera: ";
	
	public enum ViewEye
	{
		Left = 1,
		Right = 2,
	}
	
	private AROrigin _origin = null;
	protected ARMarker _marker = null;				// Instance of marker that will be used as the origin for the camera pose.
	
	[NonSerialized]
	protected Vector3 arPosition = Vector3.zero;	// Current 3D position from tracking
	[NonSerialized]
	protected Quaternion arRotation = Quaternion.identity; // Current 3D rotation from tracking
	[NonSerialized]
	protected bool arVisible = false;				// Current visibility from tracking
	[NonSerialized]
	protected float timeLastUpdate = 0;				// Time when tracking was last updated.
	[NonSerialized]
	protected float timeTrackingLost = 0;			// Time when tracking was last lost.
	
	public GameObject eventReceiver;
	
	// Stereo settings.
	public bool Stereo = false;
	public ViewEye StereoEye = ViewEye.Left;
	
	// Optical settings.
	public bool Optical = false;
	private bool opticalSetupOK = false;
	public int OpticalParamsFilenameIndex = 0;
	public string OpticalParamsFilename = "";
	public byte[] OpticalParamsFileContents = new byte[0]; // Set by the Editor.
	public float OpticalEyeLateralOffsetRight = 0.0f;
	private Matrix4x4 opticalViewMatrix; // This transform expresses the position and orientation of the physical camera in eye coordinates.

    // Calculate the pose and Display it to the text
    private Text Rotation;  // Newly added
    private Text Position;  // Newly added
    private Text Transformation; // Newly added
    public Matrix4x4 myPose; // Newly added
    private Text tipPositionCamera; // Newly added
    public Vector3 renderEndMarker; // Newly added, be public for another script
    public Vector4 tipCamera;

    public bool SetupCamera(float nearClipPlane, float farClipPlane, Matrix4x4 projectionMatrix, ref bool opticalOut)
	{
        // Link all the variables to their Texts
        Rotation = GameObject.Find("/Canvas/Rotation").GetComponent<Text>();
        Position = GameObject.Find("/Canvas/Position").GetComponent<Text>();
        Transformation = GameObject.Find("/Canvas/Camera_T_Marker").GetComponent<Text>();
        tipPositionCamera = GameObject.Find("/Canvas/Tip Position").GetComponent<Text>();

        Camera c = this.gameObject.GetComponent<Camera>();
		
		// A perspective projection matrix from the tracker
		c.orthographic = false;
		
		// Shouldn't really need to set these, because they are part of the custom 
		// projection matrix, but it seems that in the editor, the preview camera view 
		// isn't using the custom projection matrix.
		c.nearClipPlane = nearClipPlane;
		c.farClipPlane = farClipPlane;
		
		if (Optical) {
			float fovy ;
			float aspect;
			float[] m = new float[16];
			float[] p = new float[16];
			opticalSetupOK = PluginFunctions.arwLoadOpticalParams(null, OpticalParamsFileContents, OpticalParamsFileContents.Length, out fovy, out aspect, m, p);
			if (!opticalSetupOK) {
				ARController.Log(LogTag + "Error loading optical parameters.");
				return false;
			}
			m[12] *= 0.001f;
			m[13] *= 0.001f;
			m[14] *= 0.001f;
			ARController.Log(LogTag + "Optical parameters: fovy=" + fovy  + ", aspect=" + aspect + ", camera position (m)={" + m[12].ToString("F3") + ", " + m[13].ToString("F3") + ", " + m[14].ToString("F3") + "}");
			
			c.projectionMatrix = ARUtilityFunctions.MatrixFromFloatArray(p);
			
			opticalViewMatrix = ARUtilityFunctions.MatrixFromFloatArray(m);
			if (OpticalEyeLateralOffsetRight != 0.0f) opticalViewMatrix = Matrix4x4.TRS(new Vector3(-OpticalEyeLateralOffsetRight, 0.0f, 0.0f), Quaternion.identity, Vector3.one) * opticalViewMatrix; 
			// Convert to left-hand matrix.
			opticalViewMatrix = ARUtilityFunctions.LHMatrixFromRHMatrix(opticalViewMatrix);
			
			opticalOut = true;
		} else {
			c.projectionMatrix = projectionMatrix;
		}
		
		// Don't clear anything or else we interfere with other foreground cameras
		c.clearFlags = CameraClearFlags.Nothing;
		
		// Renders after the clear and background cameras
		c.depth = 2;
		
		c.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
		c.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
		c.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		
		return true;
	}
	
	// Return the origin associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual AROrigin GetOrigin()
	{
		if (_origin == null) {
			// Locate the origin in parent.
			_origin = this.gameObject.GetComponentInParent<AROrigin>();
		}
		return _origin;
	}
	
	// Get the marker, if any, currently acting as the base.
	public virtual ARMarker GetMarker()
	{
		AROrigin origin = GetOrigin();
		if (origin == null) return null;
		return (origin.GetBaseMarker());
	}
	
	// Updates arVisible, arPosition, arRotation based on linked marker state.
	private void UpdateTracking(Boolean write)
	{   
        // Note the current time
        timeLastUpdate = Time.realtimeSinceStartup;

		// First, ensure we have a base marker. If none, then no markers are currently in view.
		ARMarker marker = GetMarker();
		if (marker == null)
        {
			if (arVisible)
            {
				// Marker was visible but now is hidden.
				timeTrackingLost = timeLastUpdate;
				arVisible = false;
			}
        }
        else
        {
			if (marker.Visible)
            {
				Matrix4x4 pose;
                //Matrix4x4 myPose;
                if (Optical && opticalSetupOK)
                {
					pose = (opticalViewMatrix * marker.TransformationMatrix).inverse;
                    // myPose is the transformation from marker to camera
                    myPose = (opticalViewMatrix * marker.TransformationMatrix);
                }
                else
                {
					pose = marker.TransformationMatrix.inverse;
                    // myPose is the transformation from marker to camera
                    myPose = marker.TransformationMatrix;
                }

                // choose to treat an unrotated marker as standing vertically, and apply a transform to the scene to
                // to get it to lie flat on the ground.
                arPosition = ARUtilityFunctions.PositionFromMatrix(pose);
				arRotation = ARUtilityFunctions.QuaternionFromMatrix(pose);

                // Show the pose to Text
                showText(myPose);

                // Write the pose to txt
                if(write)
                {
                    writeText(myPose);
                }

                // read pivot calibration from txt file
                string path = "C:/Users/zhaoz/Desktop/JohnsHopkins/Spring2017/CISII/F200/Pivot Calibration/pointTipNeedle.txt";
                Vector4 tipMarker;
                tipMarker = readPivotCalibration(path);

                // transform the tip position from wrt marker to wrt camera
                tipCamera = myPose * tipMarker;

                // convert Vector4 tipMarker to 3D
                renderEndMarker.x = tipMarker.x;
                renderEndMarker.y = tipMarker.y;
                renderEndMarker.z = tipMarker.z;
                
                tipPositionCamera.text = "Tip Position: " + tipCamera.ToString();

                if (!arVisible)
                {
                    // Marker was hidden but still show the augmentation as it was there
                    arVisible = true;
                }
            }
            else
            {
				if (arVisible)
                {
                    // Marker was hidden but still show the augmentation as it was there
                    timeTrackingLost = timeLastUpdate;
					arVisible = false;
				}
            }
            
        }
	}
	
	protected virtual void ApplyTracking()
	{
		if (arVisible) {
			transform.localPosition = arPosition; // TODO: Change to transform.position = PositionFromMatrix(origin.transform.localToWorldMatrix * pose) etc;
			transform.localRotation = arRotation;
		}
	}
	
	// Use LateUpdate to be sure the ARMarker has updated before we try and use the transformation.
	public void LateUpdate()
	{
		// Local scale is always 1 for now
		transform.localScale = Vector3.one;
		// Update tracking if we are running in Player.
		if (Application.isPlaying) {
            UpdateTracking(false);
            ApplyTracking();
		}
	}

    // Display the rotaion and position part to scene
    void showText(Matrix4x4 curPose) // Newly added
    {
        Rotation.text = "Rotation: " + ARUtilityFunctions.QuaternionFromMatrix(curPose).ToString();
        Position.text = "Position: " + ARUtilityFunctions.PositionFromMatrix(curPose).ToString();
        Transformation.text = curPose.ToString();
    }

    // Save the transformation to a txt file
    void writeText(Matrix4x4 curPose)
    {
        // Check if the txt file exists
        string path = "C:/Users/zhaoz/Desktop/JohnsHopkins/Spring2017/CISII/F200/Pivot Calibration/zzkMarker0.txt";
        bool isExisted = File.Exists(path);

        // if existed, continue writing
        if (isExisted == true)
        {

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    File.AppendAllText(@path, curPose[i, j] + " ");
                }
                File.AppendAllText(@path, Environment.NewLine);
            }

        }
        // if not existed, create a new file and starting writing
        else
        {
            TextWriter tw = new StreamWriter(path);
            using (tw)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        tw.Write(curPose[i, j] + " ");
                    }
                    tw.WriteLine();
                }
                tw.Close();
            }
        }

    }

    // read the pivot calibration result from a txt file
    Vector4 readPivotCalibration(string path)
    {
        // the function reads from a path and returns the position w.r.t marker

        Vector4 pointTip = new Vector4();
        if(path == null)
        {
            print("No path!\n");
            return pointTip;
        }

        using (TextReader reader = File.OpenText(path))
        {
            pointTip.x = float.Parse(reader.ReadLine());
            pointTip.y = float.Parse(reader.ReadLine());
            pointTip.z = float.Parse(reader.ReadLine());
            pointTip.w = float.Parse(reader.ReadLine());
        }

        return pointTip;
    }
}

