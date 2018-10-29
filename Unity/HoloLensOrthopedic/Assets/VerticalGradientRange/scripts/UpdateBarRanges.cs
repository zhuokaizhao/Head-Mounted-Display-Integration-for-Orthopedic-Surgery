// VerticalGradientRange by mgear / unitycoder.com
// attach this script to the object(s) which uses gradientBar material/shader

// this script sends top line and bottom line world Y positions to shader
// it wouldnt be necessary to send them inside Update() loop, only when they are moved.
// But in this example they are sent inside Update() loop.

using UnityEngine;
using System.Collections;

namespace unitycodercom_barColors
{

    public class UpdateBarRanges : MonoBehaviour
    {

        // assign top line object here (it doesnt have to be visible object, empty gameobject is also ok)
        public Transform topLine;
        // assign bottom line object here (it doesnt have to be visible object, empty gameobject is also ok)
        public Transform bottomLine;

        void Update()
        {
            // send our objects world Y position to shader
            // renderer.material.SetFloat("_TopLine", topLine.position.y);
            // renderer.material.SetFloat("_BottomLine", bottomLine.position.y);
        }
    }

}