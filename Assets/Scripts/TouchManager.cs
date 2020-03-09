using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

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
        DetectTouchingStatus();
    }

    // Ray hit colliders
    Collider[] hitColliders;
    bool isTouching, isOverlapped;
    /// <summary>
    /// Detect whether this finger is colliding with the other collider
    /// </summary>
    void DetectTouchingStatus()
    {
        hitColliders = Physics.OverlapSphere(
            indexFinger.m_Collider.ClosestPoint(thumb.m_Collider.bounds.center),
            0.0001f);

        isTouching = false;
        foreach (var coll in hitColliders)
        {
            isTouching |= (coll == thumb.m_Collider);
        }

        // Detect touching and overlapped
        if (isTouching)
        {
            thumb.UpdateTouchDotObj();
            indexFinger.UpdateTouchDotObj();
            isOverlapped = DetectOverlapStatus();
        }
        else
        {
            isOverlapped = false;
        }

        // Set status
        thumb.UpdateStatus(isTouching, isOverlapped);
        indexFinger.UpdateStatus(isTouching, isOverlapped);

        // Calculate touch position
        if (isTouching && !isOverlapped)
        {
            thumb.CalcTouchPosition();
            indexFinger.CalcTouchPosition();
        }
    }

    bool DetectOverlapStatus()
    {
        // Set overlapped true if the distance between two touch point is too large
        return Vector3.Distance(
                thumb.touchPointObj.position,
                indexFinger.touchPointObj.position) > 1e-3;
    }
}
