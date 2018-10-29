using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Text;

public class LineControl : MonoBehaviour
{

    // whether we are testing the rendering
    public bool ShowLine = true;
    // materials used in this render
    public Material LineMaterial;
    // width of the line renderer
    public float WidthMultiplier;
    // public variables for start and end
    public Vector3 EndPosition;
    public Vector3 StartPosition;
    


    // check if the marker is seen by ARCamera
    private bool seenMarker = true;

    void Start()
    {
        // set up the line renderer if ShowLine is true
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        // get the input material
        lineRenderer.material = LineMaterial;
        // set color
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        // an overall multipier that is applied to the LineRenderer.widthCurve to get the final width of the line
        lineRenderer.widthMultiplier = WidthMultiplier;

    }

    void Update()
    {
        if (!ShowLine)
            return;

        // Set the input position
        // Find the Camera object
        GameObject marker = GameObject.Find("ARUWP Controller");
        // access the ARCamera script of Camera
        ARUWPMarker markerHiro = marker.GetComponent<ARUWPMarker>();

        // read the transformation matrix
        Matrix4x4 myPose = markerHiro.transMatrix;

        // read pivot calibration from txt file
        Vector4 tipMarker;
        tipMarker = new Vector4((float)(-3.276183/100.0), (float)(-187.381409/100.0), (float)(2.641751/100.0), 1);

        // multiply the tip position with the transformation
        Vector4 tipHoloLens;
        tipHoloLens = myPose * tipMarker;

        // convert Vector4 tipMarker to 3D
        EndPosition.x = tipHoloLens.x;
        EndPosition.y = tipHoloLens.y;
        EndPosition.z = tipHoloLens.z;
        // For now we don't know the start position yet, should come from classifier
        Vector3 startPosition = new Vector3(0, 0, 0);

        // if no target position yet, means that the merker is not seen, no rendering
        // endPosition is in fact the position of the tool tip
        if (EndPosition == null)
        {
            seenMarker = false;
        }

        // if the marker could be seen, render
        if (seenMarker)
        {
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            // if we don't have any result from point cloud classifier, make one for now
            StartPosition.x = (float)(EndPosition.x);
            StartPosition.y = (float)(EndPosition.y + 0.03);
            StartPosition.z = (float)(EndPosition.z);
            // lineRenderer.transform.position = startPosition;
            lineRenderer.useWorldSpace = true;
            // make a vector of Vector3 for SetPositions
            var points = new Vector3[2];
            points[0] = StartPosition;
            points[1] = EndPosition;

            lineRenderer.SetPositions(points);
        }

    }

    // read the pivot calibration result from a txt file
    Vector4 readPivotCalibration(string path)
    {
        // the function reads from a path and returns the position w.r.t marker

        Vector4 pointTip = new Vector4();
        if (path == null)
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