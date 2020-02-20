using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDetection : MonoBehaviour
{
    public JointType.Finger fingertipType = JointType.Finger.thumb;
    JointType.Finger detectedFingerType;

    bool touched;

    // Start is called before the first frame update
    void Start()
    {
        detectedFingerType = (JointType.Finger)(((int)fingertipType + 1) % 2);

        touched = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(fingertipType.ToString() + " " + touched);
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
    }
}
