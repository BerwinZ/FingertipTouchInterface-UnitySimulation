using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using System;

public class TouchManager : MonoBehaviour, ITouchManagerAction
{ 
    TouchDetectionBase thumb;
    TouchDetectionBase indexFinger;

    public event EventHandler<bool> OnTouchCalcFinish;

    void Start()
    {
        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        indexFinger = ScriptFind.FindTouchDetection(Finger.index);

        // Subscribe the joint update event
        JointManager.Instance.OnJointUpdate += 
            (sender, e) => StartCoroutine(DetectTouchingStatus());

        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(DetectTouchingStatus());
    }

    // Ray hit colliders
    Collider[] _hitColliders;
    bool _isTouching, _isOverlapped;
    bool IsValid => _isTouching && !_isOverlapped;

    /// <summary>
    /// Detect whether this finger is colliding with the other collider
    /// </summary>
    IEnumerator DetectTouchingStatus()
    {
        _isTouching = IsIntersect(
            indexFinger.m_Collider.ClosestPoint(thumb.m_Collider.bounds.center),
            thumb.m_Collider);
    
        // Detect touching and overlapped
        if (_isTouching)
        {
            // Need some time here
            yield return StartCoroutine(DetectOverlapStatus());
        }
        else
        {
            _isOverlapped = false;
        }

        // Set status
        thumb.UpdateStatus(_isTouching, _isOverlapped);
        indexFinger.UpdateStatus(_isTouching, _isOverlapped);

        // Calculate touch position if is valid
        if (IsValid)
        {
            thumb.CalcTouchPosition();
            indexFinger.CalcTouchPosition();
        }

        OnTouchCalcFinish?.Invoke(this, IsValid);
    }

    bool IsIntersect(Vector3 pos, Collider coll)
    {
        _hitColliders = Physics.OverlapSphere(pos, 0.0001f);
        foreach (var hitColl in _hitColliders)
        {
            if(coll == hitColl)
                return true;
        }
        return false;
    }

    IEnumerator DetectOverlapStatus()
    {
        if(IsIntersect(indexFinger.m_Collider.bounds.center, thumb.m_Collider) ||
           IsIntersect(thumb.m_Collider.bounds.center, indexFinger.m_Collider))
           {
                _isOverlapped = true;
               yield break;
           }

        // Check the dot obj's relative position
        thumb.UpdateTouchDotObj();
        indexFinger.UpdateTouchDotObj();

        // need wait some time here to update in Unity
        yield return new WaitForFixedUpdate();

        _isOverlapped = Vector3.Distance(
                thumb.touchPointObj.position,
                indexFinger.touchPointObj.position) > 1e-3;
    }
}
