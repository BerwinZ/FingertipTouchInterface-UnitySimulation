using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

/// <summary>
/// Draw the position of the thumb touch _point
/// </summary>
public class DrawBoard : MonoBehaviour
{
    [Range(5, 30), SerializeField]
    float movScaler = 5.0f;

    RectTransform _point;
    Image _pointMesh;
    Text _thumbPosText;
    Text _indexPosText;
    
    IFingerAction thumb;
    IFingerAction index;

    bool _isTouching;
    bool _isOverlapped;
    Vector3 _pointPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        _point = transform.GetChild(1).GetComponent<RectTransform>();
        _pointMesh = transform.GetChild(1).GetComponent<Image>();
        _thumbPosText = transform.GetChild(2).GetComponent<Text>();
        _indexPosText = transform.GetChild(3).GetComponent<Text>();

        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        thumb.OnTouchStatusChange += delegate (object sender, bool flag)
        {
            _isTouching = flag;
            ChangeDotColor();
        };
        thumb.OnOverlapStatusChange += delegate (object sender, bool flag)
        {
            _isOverlapped = flag;
            ChangeDotColor();
        };
        thumb.OnTouchPositionChange += delegate (object sender, Vector2 touchPos)
        {
            _pointPos.x = touchPos.x * movScaler;
            _pointPos.y = touchPos.y * movScaler;
            _point.localPosition = _pointPos;
        };
        thumb.OnTouchPositionChange += delegate (object sender, Vector2 touchPos)
        {
            _thumbPosText.text = "T(mm): " + touchPos;
        };

        index = ScriptFind.FindTouchDetection(Finger.index);
        index.OnTouchPositionChange += delegate (object sender, Vector2 touchPos)
        {
            _indexPosText.text = " I(mm): " + touchPos;
        };
    }

    Color _red = new Color(255.0f, 0.0f, 0.0f, 255.0f) / 255.0f;
    Color _blue = new Color(19.0f, 29.0f, 243.0f, 255.0f) / 255.0f;

    bool IsValid => _isTouching && !_isOverlapped;
    void ChangeDotColor()
    {
        _pointMesh.color = IsValid ? _blue : _red;
    }
}
