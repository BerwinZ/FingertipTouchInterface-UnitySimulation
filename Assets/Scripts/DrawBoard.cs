using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draw the position of the touch point
/// </summary>
public class DrawBoard : Singleton<DrawBoard>
{
    RectTransform point;
    Text posIndicator;
    TouchDetection thumb;
    float scaler;

    // Start is called before the first frame update
    void Start()
    {
        point = transform.GetChild(1).GetComponent<RectTransform>();
        posIndicator = transform.GetChild(2).GetComponent<Text>();
        TouchDetection[] fingeres = GameObject.FindObjectsOfType<TouchDetection>();
        foreach(var finger in fingeres)
        {
            if(finger.fingertipType == JointType.Finger.thumb)
            {
                thumb = finger;
            }
        }
        scaler = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        point.localPosition = new Vector3(thumb.touchPosition.x * scaler, thumb.touchPosition.y * scaler, 0);

        posIndicator.text = thumb.touchPosition.ToString();
    }
}
