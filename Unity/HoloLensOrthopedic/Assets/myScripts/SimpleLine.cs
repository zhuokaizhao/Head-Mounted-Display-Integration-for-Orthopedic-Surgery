using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA;

public class SimpleLine : MonoBehaviour
{

    // whether we are testing the rendering
    public bool ShowLine = true;
    // materials used in this render
    public Material LineMaterial;

    // width of the line renderer
    public float WidthMultiplier;
    // rendering the line
    public LineRenderer lineRenderer;
    public Vector3 RenderStartPos = new Vector3();
    public Vector3 RenderEndPos = new Vector3();

    // flag for defining if the anchor is there
    // bool anchorExist = false;

    // check if the marker is seen by ARCamera
    private bool seenMarker = true;

    void Start()
    {
        // set up the line renderer if ShowLine is true
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        // get the input material
        lineRenderer.material = LineMaterial;
        // an overall multipier that is applied to the LineRenderer.widthCurve to get the final width of the line
        lineRenderer.widthMultiplier = WidthMultiplier;

    }

    void Update()
    {
        if (!ShowLine)
            return;

        // Set the input position
        // Find the Camera object
        GameObject Computations = GameObject.Find("Computations");
        // access the ARCamera script of Camera
        ProcessData data = Computations.GetComponent<ProcessData>();

        RenderStartPos = data.HOLOStartPosition;
        RenderEndPos = data.HOLOEndPosition;

        if (RenderStartPos == null || RenderEndPos == null)
        {
            seenMarker = false;
        }
        else
        {
            seenMarker = true;
        }

        // if the marker could be seen, render
        if (seenMarker)
        {   
            /*
            if (anchorExist)
            {
                Destroy(gameObject.GetComponent<WorldAnchor>());
                anchorExist = false;
            }
            */
            lineRenderer = GameObject.Find("Augmentations").GetComponent<LineRenderer>();
            // make a vector of Vector3 for SetPositions
            var points = new Vector3[2];
            points[0] = RenderStartPos;
            points[1] = RenderEndPos;

            // gradient color
            //lineRenderer.material.SetFloat("_TopLine", RenderStartPos.y);
            //lineRenderer.material.SetFloat("_BottomLine", RenderEndPos.y);
            lineRenderer.SetPositions(points);

            /*
            // add a world anchor to improve more stable
            WorldAnchor anchor = gameObject.AddComponent<WorldAnchor>();
            anchorExist = true;
            */
        }

    }

}