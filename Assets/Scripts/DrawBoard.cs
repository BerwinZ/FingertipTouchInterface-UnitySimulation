using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

/// <summary>
/// Draw the position of the touch point
/// </summary>
public class DrawBoard : Singleton<DrawBoard>
{
    [Range(5, 30)]
    public float movScaler = 5.0f;
    RectTransform point;
    Text indicatorThumb;
    Text indicatorIndex;
    TouchDetection thumb;
    TouchDetection index;

    // Start is called before the first frame update
    void Start()
    {
        point = transform.GetChild(1).GetComponent<RectTransform>();
        indicatorThumb = transform.GetChild(2).GetComponent<Text>();
        indicatorIndex = transform.GetChild(3).GetComponent<Text>();

        TouchDetection[] fingeres = GameObject.FindObjectsOfType<TouchDetection>();
        foreach(var finger in fingeres)
        {
            if(finger.fingertipType == Finger.thumb)
            {
                thumb = finger;
            }
            else if(finger.fingertipType == Finger.index)
            {
                index = finger;
            }
        }
    }

    Vector3 pointPos = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        pointPos.x = thumb.touchPosition.x * movScaler;
        pointPos.y = thumb.touchPosition.y * movScaler;
        point.localPosition = pointPos;

        indicatorThumb.text = "T(mm): " + thumb.touchPosition;
        indicatorIndex.text = " I(mm): " + index.touchPosition;
    }
}
