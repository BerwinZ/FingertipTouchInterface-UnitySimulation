using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawBoard : Singleton<DrawBoard>
{
    RectTransform point;
    TouchDetection thumb;
    float scaler;

    // Start is called before the first frame update
    void Start()
    {
        point = transform.GetChild(0).GetComponent<RectTransform>();
        thumb = GameObject.Find("thumb_tip").GetComponent<TouchDetection>();
        scaler = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        point.localPosition = new Vector3(thumb.touchPosition.x * scaler, thumb.touchPosition.y * scaler, 0);
    }
}
