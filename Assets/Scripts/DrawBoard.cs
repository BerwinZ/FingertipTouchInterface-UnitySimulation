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

        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        thumb.TouchStatusUpdatePublisher += TurnOnOffDotfromTouching;
        thumb.OverlapStatusUpdatePublisher += TurnOnOffDotfromOverlapped;
        thumb.TouchPositionUpdatePublisher += UpdateDrawingDotPos;
        thumb.TouchPositionUpdatePublisher += UpdateThumbIndicator;

        index = ScriptFind.FindTouchDetection(Finger.index);
        index.TouchPositionUpdatePublisher += UpdateIndexIndicator;
    }

    Vector3 pointPos = Vector3.zero;
    // Registered with the thumb update touch position event
    void UpdateDrawingDotPos(Vector2 touchPos)
    {
        pointPos.x = touchPos.x * movScaler;
        pointPos.y = touchPos.y * movScaler;
        point.localPosition = pointPos;
    }

    void UpdateThumbIndicator(Vector2 touchPos)
    {
        indicatorThumb.text = "T(mm): " + touchPos;
    }

    void UpdateIndexIndicator(Vector2 touchPos)
    {
        indicatorIndex.text = " I(mm): " + touchPos;
    }

    bool isTouching;
    bool isOverlapped;
    void TurnOnOffDotfromTouching(bool flag)
    {
        isTouching = flag;
        ChangeDotColor();
    }

    void TurnOnOffDotfromOverlapped(bool flag)
    {
        isOverlapped = flag;
        ChangeDotColor();
    }


    Color red = new Color(255.0f, 0.0f, 0.0f, 255.0f) / 255.0f;
    Color blue = new Color(19.0f, 29.0f, 243.0f, 255.0f) / 255.0f;
    void ChangeDotColor()
    {
        pointMesh.color = (isTouching && !isOverlapped)? blue: red;
    }
}
