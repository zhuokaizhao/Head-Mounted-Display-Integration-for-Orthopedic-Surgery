using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.Input;

public class LaserBeamController : MonoBehaviour {

    // position where the laser starts, actually just the needle tip position
    public Vector3 LaserStartPos;
    // laser direction, which is the same as the virtual line
    public Vector3 LaserDirection;
    // entry ring that is going to be used
    public MeshRenderer entryRing;
    public Vector3 PlacingPosition;
    public Quaternion Rotation;

    // new linerenderers
    public LineRenderer lineRendererIn;
    public LineRenderer lineRendererOut;

    // materials for new occuluded and outside needles
    public Material LineMaterialIn;
    public Material LineMaterialOut;

    // different widths
    public float WidthMultiplierIn;
    public float WidthMultiplierOut;

    // Text that shows the distance between the needle tip and surface
    private Text DistanceText;

    // this is for the controller input or tap input
    private bool placing = false;
    private Vector3 PermanentPosition;
    private Quaternion PermanentRotation;

    // boolean for if inserted
    private bool isInserted = false;

    // Use this for initialization
    void Start () {
        LaserStartPos = new Vector3();
        LaserDirection = new Vector3();

        // Prepare the new renderings
        // set up the line renderers
        lineRendererIn = GameObject.Find("Augmentations/LineIn").AddComponent<LineRenderer>();
        lineRendererOut = GameObject.Find("Augmentations/LineOut").AddComponent<LineRenderer>();

        // get the input material
        lineRendererIn.material = LineMaterialIn;
        lineRendererOut.material = LineMaterialOut;

        // an overall multipier that is applied to the LineRenderer.widthCurve to get the final width of the line
        lineRendererIn.widthMultiplier = WidthMultiplierIn;
        lineRendererOut.widthMultiplier = WidthMultiplierOut;

        // not show the new lines now
        lineRendererIn.enabled = false;
        lineRendererOut.enabled = false;

    }

    void OnSelect()
    {
        // On each Select gesture, toggle whether the user is in placing mode.
        Debug.Log("Clicked\n");
        placing = !placing;
    }

    // Update is called once per frame
    void Update () {
        // get our line object
        GameObject Augmentations = GameObject.Find("Augmentations");
        SimpleLine line = Augmentations.GetComponent<SimpleLine>();

        // get the line renderer and the start/end positions for setting the line collider
        LineRenderer lineRenderer = line.lineRenderer;
        Vector3 RenderStartPos = line.RenderStartPos;
        Vector3 RenderEndPos = line.RenderEndPos;
        float WidthMultiplier = lineRenderer.widthMultiplier;

        // update the laser start position
        LaserStartPos = RenderEndPos;

        // Direction should align with the virtual needle direction
        LaserDirection = RenderEndPos - RenderStartPos;

        RaycastHit hitInfo;
        // check if the ray hits the spatial mapping mesh
        bool hit = Physics.Raycast(LaserStartPos, LaserDirection, out hitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask);
        if (hit)
        {
            // Debug.Log("The laser hits the spatial mesh");
            // put the entry point there
            float dist;
            if(!placing)
            {
                PlacingPosition = hitInfo.point;
                entryRing.transform.position = PlacingPosition;

                // Rotate the ring to hug the surface of the hologram
                Rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                entryRing.transform.rotation = Rotation;

                // update the permanent position and rotation information
                PermanentPosition = PlacingPosition;
                PermanentRotation = Rotation;

                // distance between the hit position and the laser start position
                dist = hitInfo.distance;
            }
            else
            {
                entryRing.transform.position = PermanentPosition;
                entryRing.transform.rotation = PermanentRotation;
                // distance between the ring position and needle tip position
                dist = Vector3.Distance(PermanentPosition, LaserStartPos);
            }

            // determine the distance between the hit position and the laser start position
            DistanceText = GameObject.Find("ARDisplayHUD/DistToSurface").GetComponent<Text>();
            DistanceText.text = "Distance from tip to surface: " + dist.ToString();

            lineRendererIn = GameObject.Find("Augmentations/LineIn").GetComponent<LineRenderer>();
            lineRendererOut = GameObject.Find("Augmentations/LineOut").GetComponent<LineRenderer>();

            // points for occluded part of needle
            var pointsIn = new Vector3[2];
            pointsIn[0] = new Vector3((float)(PlacingPosition.x - 0.005), PlacingPosition.y, PlacingPosition.z);
            pointsIn[1] = RenderEndPos;

            // points for outside part of needle
            var pointsOut = new Vector3[2];
            pointsOut[0] = RenderStartPos;
            pointsOut[1] = new Vector3((float)(PlacingPosition.x - 0.005), PlacingPosition.y, PlacingPosition.z);

            // gradient color for occluded part of needle
            lineRendererIn.material.SetFloat("_TopLine", RenderStartPos.y);
            lineRendererIn.material.SetFloat("_BottomLine", RenderEndPos.y);

            // if the distance is closed to zero, mark it as inserted and go to the second mode
            if (dist < 0.02)
            {
                isInserted = true;
            }
            // if they are too further away, go back to not inserted stage
            if (dist > 0.3)
            {
                isInserted = false;
            }

            if (isInserted)
            {
                // Debug.Log("Inserted!!!!!!!!!");
                // disable the previous linerenderer
                lineRenderer.enabled = false;
                // render two lines
                lineRendererIn.SetPositions(pointsIn);
                lineRendererOut.SetPositions(pointsOut);
                lineRendererIn.enabled = true;
                lineRendererOut.enabled = true;
            }
            else
            {
                // Debug.Log("Not Inserted");
                if (lineRendererIn.enabled == true && lineRendererOut.enabled == true)
                {
                    lineRendererIn.enabled = false;
                    lineRendererOut.enabled = false;
                }
                if (lineRenderer.enabled == false)
                {
                    lineRenderer.enabled = true;
                }
            }
            
        }

    }
}
