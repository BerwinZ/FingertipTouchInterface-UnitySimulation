using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDetection : MonoBehaviour
{
    // The type of this finger
    public JointType.Finger fingertipType = JointType.Finger.thumb;

    // Type of the finger that is touched with
    JointType.Finger detectedFingerType;

    // Whether this finger is touched by another
    bool touched;

    public Transform tipSpecial { get; private set; }

    // The touch position of thumb relative to the index coordinate. 
    // For index finger, it keeps (0, 0)
    public Vector2 touchPosition { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        detectedFingerType = (JointType.Finger)(((int)fingertipType + 1) % 2);

        touched = false;

        tipSpecial = transform.parent.GetChild(1);

        touchPosition = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(fingertipType.ToString() + " Touched: " + touched);
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

    void DetectTouching(Collider other, bool isTouch)
    {
        TouchDetection otherScript = other.transform.GetComponent<TouchDetection>();
        if (otherScript == null || otherScript.fingertipType != detectedFingerType)
            return;

        touched = isTouch;

        if(touched && fingertipType == JointType.Finger.thumb)
        {
            tipSpecial.parent = otherScript.tipSpecial;
            touchPosition = new Vector2(-tipSpecial.localPosition.x, tipSpecial.localPosition.y);
            tipSpecial.parent = transform.parent;
            // Debug.Log(touchPosition);
        }
    }
}
