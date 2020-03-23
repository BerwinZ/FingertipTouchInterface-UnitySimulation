using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

/// <summary>
/// Detect the touch action, including the following parameters
/// 1. If touched
/// 2. If two fingers are overlapped
/// 3. The 2-d touch position of the one finger related to the other fingertip
/// </summary>
public class TouchDetection : TouchDetectionBase, IFingerAction
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        otherFinger = ScriptFind.FindTouchDetection(targetFingerType);
        otherCollider = otherFinger.transform.GetComponent<Collider>();

        // Get the parameters for the other collider
        _capsuleRadius = ((CapsuleCollider)otherCollider).radius;
        _capsuleHeight = ((CapsuleCollider)otherCollider).height;
        _cylinderHeight = _capsuleHeight - 2 * _capsuleRadius;

        // Get the touch point object
        touchPointObjMesh = touchPointObj.GetComponent<MeshRenderer>();

        // Register the events
        OnTouchStatusChange += (sender, flag) => touchPointObjMesh.enabled = flag;

        // Set the values
        IsTouching = false;
        IsOverlapped = false;
        TouchPosition = Vector2.zero;
    }

    #region ParaforAccurateTouchDetection
    MeshRenderer touchPointObjMesh;

    // other finger's script
    TouchDetection otherFinger;
    // other finger's collider
    Collider otherCollider;

    // Ray to intersect with this collider  
    Ray _ray = new Ray();

    // Ray hit result          
    RaycastHit _hitResult;

    // This capsule collider's _capsuleRadius, related to the collider obj's coordinate 
    float _capsuleRadius;

    // This capsule collider's whole height, related to the collider obj's coordinate        
    float _capsuleHeight;

    // This capsule collider's cylinder height, related to the collider obj's coordinate  
    float _cylinderHeight;

    // The position of touch point in the collider obj's coordinate
    Vector3 _localVector;

    // The projection of _localVector in the collider obj's x-z plane (still in collider obj's coordinate) 
    Vector3 _localProj;

    // The vector of (_localVector - sphere point)
    Vector3 _sphereVector;

    // The angle between _localProj and the collider obj's z axis (still in collider obj's coordinate)
    float _verticalAngle;

    // The angle between _localProj and the collider obj's z axis (still in collider obj's coordinate)
    float _horizontalAngle;

    // Touch point position in collider obj's coordinate (meter)
    Vector2 _localTouchM = Vector2.zero;

    // Touch point position in world coordinate (millimeter)
    Vector2 _worldTouchMM = Vector2.zero;
    #endregion

    public override void UpdateStatus(bool isTouch, bool isOverlapped)
    {
        IsTouching = isTouch;
        IsOverlapped = isOverlapped;
    }

    public override void UpdateTouchDotObj()
    {
        // Set the para for the Ray
        _ray.origin = otherCollider.bounds.center;
        _ray.direction = m_Collider.bounds.center - otherCollider.bounds.center;

        // Shoot the _ray, get the touch point which is the intersection between _ray and the collider
        m_Collider.Raycast(_ray, out _hitResult, 100.0f);
        touchPointObj.position = _hitResult.point;
    }

    /// <summary>
    /// Calculate the touch point's coordinate and store them in the paramters. 
    /// </summary>
    /// <param name="otherCollider"></param>
    /// <returns name="worldPositionMM"></returns>
    public override void CalcTouchPosition()
    {
        // Get the coordinate
        // X axis faces up (to world)
        // Y axis faces right (to world), the direction of axle of capsule
        // Z axis faces outside (to world)
        _localVector = otherFinger.touchPointObj.localPosition;

        if (_localVector.y > -_cylinderHeight / 2 &&
            _localVector.y < _cylinderHeight / 2)     // In the cylinder part
        {
            _localTouchM.x = _localVector.y;

            // Project the _localVector to the x-z plane
            _localProj = Vector3.ProjectOnPlane(_localVector, Vector3.up);
            _verticalAngle = (_localVector.x > 0) ? 1 : -1;
            _verticalAngle *= Vector3.Angle(Vector3.forward, _localProj);
            _localTouchM.y = _verticalAngle * Mathf.PI * _capsuleRadius / 180.0f;
        }
        else
        {
            // Use sphere vector here
            _sphereVector = (_localVector.y > 0) ?
                            _localVector - new Vector3(0, _cylinderHeight / 2, 0) :
                            _localVector - new Vector3(0, -_cylinderHeight / 2, 0);

            // Project sphere vector to the y-z plane 
            _localProj = Vector3.ProjectOnPlane(_sphereVector, Vector3.right);
            _horizontalAngle = Vector3.Angle(Vector3.forward, _localProj);
            _localTouchM.x = (_localVector.y > 0) ? 1 : -1;
            _localTouchM.x *= _horizontalAngle * Mathf.PI * _capsuleRadius / 180.0f + _cylinderHeight / 2;

            // Project sphere vector to the x-z plane 
            _localProj = Vector3.ProjectOnPlane(_sphereVector, Vector3.up);
            _verticalAngle = (_localProj.x > 0) ? 1 : -1;
            _verticalAngle *= Vector3.Angle(Vector3.forward, _localProj);
            _localTouchM.y = _verticalAngle * Mathf.PI * _capsuleRadius / 180.0f;
        }

        // Transfer it to the world scale parameters but from meter to millimeter
        _worldTouchMM = _localTouchM * 1000.0f * otherCollider.transform.localScale.x;

        TouchPosition = _worldTouchMM;
    }
}
