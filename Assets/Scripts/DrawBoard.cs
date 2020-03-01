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
    Image pointMesh;
    Text indicatorThumb;
    Text indicatorIndex;
    TouchDetection thumb;
    TouchDetection index;

    // Start is called before the first frame update
    void Start()
    {
        point = transform.GetChild(1).GetComponent<RectTransform>();
        pointMesh = transform.GetChild(1).GetComponent<Image>();
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

        thumb.UpdateTouchPosition += UpdateDrawingDot;
    }

    Vector3 pointPos = Vector3.zero;
    Color red = new Color(255.0f, 0.0f, 0.0f, 255.0f) / 255.0f;
    Color blue = new Color(19.0f, 29.0f, 243.0f, 255.0f) / 255.0f;
    // Registered with the thumb update touch position event
    void UpdateDrawingDot(Vector2 touchPos)
    {
        pointPos.x = touchPos.x * movScaler;
        pointPos.y = touchPos.y * movScaler;
        point.localPosition = pointPos;

        indicatorThumb.text = "T(mm): " + thumb.touchPosition;
        indicatorIndex.text = " I(mm): " + index.touchPosition;
    }
}
