using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDetection : MonoBehaviour
{
    // The type of this finger
    public JointType.Finger fingertipType = JointType.Finger.thumb;

    // Type of the finger that is isTouching with
    JointType.Finger detectedFingerType;

    // Whether this finger is isTouching by another
    public bool isTouching {get; private set;}

    // Whether this finger is overlapped too much
    public bool isOverlapped {get; private set;}

    // For thumb, the tip special is the touch point
    // For index finger, it is the plane coordinate
    public Transform tipSpecial { get; private set; }

    // For thumb, the touch position is the X-Y position relative to the index finger coordinate. 
    // For index finger, it keeps (0, 0)
    public Vector2 touchPosition { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        detectedFingerType = (JointType.Finger)(((int)fingertipType + 1) % 2);

        isTouching = false;
        isOverlapped = false;

        tipSpecial = transform.parent.GetChild(1);

        touchPosition = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(fingertipType.ToString() + " isTouching: " + isTouching);
    }

    private void OnTriggerEnter(Collider other)
    {
        DetectTouching(other, true);
    }

    private void OnTriggerStay(Collider other)
    {
        DetectTouching(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        DetectTouching(other, false);
    }

    Vector2 posTmp = new Vector2(0, 0);
    void DetectTouching(Collider other, bool entry)
    {
        TouchDetection otherScript = other.transform.GetComponent<TouchDetection>();
        if (otherScript == null || otherScript.fingertipType != detectedFingerType)
            return;

        isTouching = entry;

        if(isTouching && fingertipType == JointType.Finger.thumb)
        {
            // Set the index finger plan to be the parent of tip special
            tipSpecial.parent = otherScript.tipSpecial;

            posTmp.x = -tipSpecial.localPosition.x;
            posTmp.y = tipSpecial.localPosition.y;
            touchPosition = posTmp;
            isOverlapped = tipSpecial.localPosition.z < 0;

            // Set the parent back
            tipSpecial.parent = transform.parent;
        }
        else
        {
            isOverlapped = false;
        }
    }
}
