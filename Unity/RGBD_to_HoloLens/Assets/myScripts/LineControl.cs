using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineControl : MonoBehaviour {

    // whether we are testing the rendering
    public bool ShowLine = true;
    // materials used in this render
    public Material LineMaterial;
    // width of the line renderer
    public float WidthMultiplier;

    // check if the marker is seen by ARCamera
    private bool seenMarker = true;

    // start and end position wrt marker, make it public for UDPSend
    public Vector3 startPosition = new Vector3(0, 0, 0);
    public Vector3 endPosition;

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

        // Find the Camera object
        GameObject Camera = GameObject.Find("Calibration/Scene Root/Camera");
        // access the ARCamera script of Camera
        ARCamera arCamera = Camera.GetComponent<ARCamera>();

        // access the end position defined by ARCamera, which is just the position of tip wrt marker
        Vector3 inputPos;
        inputPos = arCamera.renderEndMarker;
        float inputX = inputPos.x;
        float inputY = inputPos.y;
        float inputZ = inputPos.z;

        // ARToolKit's unit is meter, the real coordinate in mm needs to be divided by 1000
        endPosition = new Vector3(inputX/1000, inputY/1000, inputZ/1000);

        // if no target position yet, means that the merker is not seen, no rendering
        if (endPosition == null)
        {
            seenMarker = false;
        }

        // if the marker could be seen, render
        if (seenMarker)
        {
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            // if we don't have any result from point cloud classifier, make one for now
            startPosition.x = (float)(endPosition.x);
            startPosition.y = (float)(endPosition.y + 0.1);
            startPosition.z = (float)(endPosition.z);

            // make a vector of Vector3 for SetPositions
            var points = new Vector3[2];
            points[0] = startPosition;
            points[1] = endPosition;

            lineRenderer.SetPositions(points);
        }
        
    }

}
