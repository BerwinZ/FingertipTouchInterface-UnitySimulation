using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using System;

public class TouchManager : MonoBehaviour
{
    TouchDetectionBase thumb;
    TouchDetectionBase indexFinger;
    IJointMangerAction jointManager;

    void Start()
    {
        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        indexFinger = ScriptFind.FindTouchDetection(Finger.index);
        // Subscribe the joint update event
        jointManager = JointManager.Instance;
        jointManager.OnJointUpdate += DetectTouchingStatus;

        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return new WaitForSeconds(0.5f);
        DetectTouchingStatus(this, null);
    }

    // Ray hit colliders
    Collider[] _hitColliders;
    bool _isTouching, _isOverlapped;
    bool IsValid => _isTouching && !_isOverlapped;

    /// <summary>
    /// Detect whether this finger is colliding with the other collider
    /// </summary>
    void DetectTouchingStatus(object sender, EventArgs e)
    {
        _isTouching = IsIntersect(
            indexFinger.m_Collider.ClosestPoint(thumb.m_Collider.bounds.center),
            thumb.m_Collider);
    
        // Detect touching and overlapped
        if (_isTouching)
        {
            _isOverlapped = DetectOverlapStatus();
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

    bool DetectOverlapStatus()
    {
        if(IsIntersect(indexFinger.m_Collider.bounds.center, thumb.m_Collider) ||
           IsIntersect(thumb.m_Collider.bounds.center, indexFinger.m_Collider))
           {
               return true;
           }

        // Check the dot obj's relative position
        thumb.UpdateTouchDotObj();
        indexFinger.UpdateTouchDotObj();

        return Vector3.Distance(
                thumb.touchPointObj.position,
                indexFinger.touchPointObj.position) > 1e-3;
    }
}
