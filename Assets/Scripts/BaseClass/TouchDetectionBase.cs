using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public abstract class TouchDetectionBase : MonoBehaviour, IFingerAction
{
    // The type of this finger
    [SerializeField]
    protected Finger fingertipType = Finger.thumb;
    public Finger FingerType => fingertipType;
    protected Finger targetFingerType;
    public Collider m_Collider { get; protected set; }
    
    // The touch point
    public Transform touchPointObj {get; private set; }

    public event BooleanEventHandler OnTouchStatusChange;
    public event BooleanEventHandler OnOverlapStatusChange;
    public event Vector2EventHandler OnTouchPositionChange;

    // Whether this finger is isTouching by another
    bool isTouching;
    // Whether this finger is overlapped too much
    bool isOverlapped;
    // For thumb, the touch position is the X-Y position relative to the index finger coordinate. 
    // For index finger, the touch position is the X-Y position relative to the thumb coordinate. 
    Vector2 touchPosition;

    public bool IsTouching
    {
        get => isTouching;
        protected set
        {
            isTouching = value;
            OnTouchStatusChange?.Invoke(value);
        }
    }

    public bool IsOverlapped
    {
        get => isOverlapped;
        protected set
        {
            isOverlapped = value;
            OnOverlapStatusChange?.Invoke(value);
        }
    }

    public Vector2 TouchPosition
    {
        get => touchPosition;
        protected set
        {
            touchPosition = value;
            OnTouchPositionChange?.Invoke(value);
        }
    }

    protected virtual void Start()
    {
        targetFingerType = (Finger)(((int)fingertipType + 1) % 2);

        m_Collider = GetComponent<Collider>();

        touchPointObj = transform.GetChild(0);
    }

    public abstract void UpdateStatus(bool isTouch, bool isOverlapped);
    public abstract void UpdateTouchDotObj();
    public abstract void CalcTouchPosition();
}