using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GestureManager : MonoBehaviour
{

    // public GameObject FocusedObject { get; private set; }
    GameObject FocusedObject;
    // used to recognize gesture
    GestureRecognizer recognizer;

    // sets up the Gesture Recognizer and sets an event for when the user Air taps
    void Start()
    {
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            // Send an OnSelect message to the ring mesh
            FocusedObject.SendMessageUpwards("OnSelect");
        };
        // The recognizer is always listening for any registered events once it is started
        recognizer.StartCapturingGestures();
    }

    // Update is called once per frame
    void Update()
    {
        // link the gameobject
        FocusedObject = GameObject.Find("LaserBeam");
    }
}
