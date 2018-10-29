using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionHandler : MonoBehaviour {

	// initialization
	void Start () {
        // Now we add a collider to the virtual needle
        // BoxCollider lineCollider = GameObject.Find("Augmentations/Collider").AddComponent<BoxCollider>();
        // turn on the collision trigger
        //lineCollider.isTrigger = true;
        //Debug.Log("line collider has been added in Start()");
        /*
        // add rigid body to the line renderer
        Rigidbody rigidLine = GameObject.Find("Augmentations").AddComponent<Rigidbody>();
        // disable the gravity
        rigidLine.useGravity = false;
        // kinematics
        rigidLine.isKinematic = true;
        // detect collisions
        rigidLine.detectCollisions = true;
        Debug.Log("rigidbody for line is added in Start()");
        */

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

        // get the collider we created
        BoxCollider lineCollider = GameObject.Find("Augmentations/Collider").GetComponent<BoxCollider>();
        // the collider is the child of the line 
        Debug.Log("Box Collider's parent is:" + lineCollider.transform.parent.name);
        lineCollider.transform.parent = lineRenderer.transform;
        // get the length of the line
        float lineLength = Vector3.Distance(RenderStartPos, RenderEndPos);
        Debug.Log("The length of the box collider is: " + lineLength.ToString());
        // size of collider is set where X is the lenght of line, Y is the width of the line, Z will be set as per requirement
        lineCollider.size = new Vector3(lineLength, WidthMultiplier, 1f);
        // midpoint
        Vector3 midPoint = (RenderStartPos + RenderEndPos) / 2;
        // setting position of collider object
        lineCollider.transform.position = midPoint;
        Debug.Log("The central position of the box collider is: " + midPoint.ToString());
        // calculate the angle between startPos and endPos using angle = atan(y/x)
        // calculate abs(y/x)
        float angle = (Mathf.Abs(RenderStartPos.y - RenderEndPos.y) / Mathf.Abs(RenderStartPos.x - RenderEndPos.x));
        // when y/x is negative, we need to add -1 to the above abs(y/x)
        if ((RenderStartPos.y < RenderEndPos.y && RenderStartPos.x > RenderEndPos.x) || (RenderEndPos.y < RenderStartPos.y && RenderEndPos.x > RenderStartPos.x))
        {
            angle *= -1;
        }
        // calculate the angle, notice that the Atan returns radians, which needs to be converted to degree
        angle = Mathf.Rad2Deg * Mathf.Atan(angle);
        // rotate the collider
        lineCollider.transform.Rotate(0, 0, angle);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter OnTriggerEnter");
        // compare using the tag of the spatial mapping
        if (other.gameObject.CompareTag("MappingMesh"))
        {
            Debug.Log("Collision!");
            Text collisionStatus = GameObject.Find("ARDisplayHUD/CollisionStatus").GetComponent<Text>();
            collisionStatus.text = "Collision Detected!";
        }

    }
    
}
