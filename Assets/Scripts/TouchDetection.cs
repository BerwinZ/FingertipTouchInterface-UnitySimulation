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

        // Get the touch point object
        touchPointObjMesh = touchPointObj.GetComponent<MeshRenderer>();

        // Register the events
        OnTouchStatusChange += ChangeTouchPointObjMesh;

        // Set the values
        IsTouching = false;
        IsOverlapped = false;
        TouchPosition = Vector2.zero;
    }

    void ChangeTouchPointObjMesh(bool flag)
    {
        touchPointObjMesh.enabled = flag;
    }

    #region ParaforAccurateTouchDetection
    MeshRenderer touchPointObjMesh;

    // other finger's script
    TouchDetection otherFinger;
    // other finger's collider
    Collider otherCollider;

    // Ray to intersect with this collider  
    Ray ray = new Ray();

    // Ray hit result          
    RaycastHit hitResult;

    // This capsule collider's radius, related to the collider obj's coordinate 
    float radius;

    // This capsule collider's whole height, related to the collider obj's coordinate        
    float wholeHeight;

    // This capsule collider's cylinder height, related to the collider obj's coordinate  
    float cylinderHeight;

    // The position of touch point in the collider obj's coordinate
    Vector3 localVector;

    // The projection of localVector in the collider obj's x-z plane (still in collider obj's coordinate) 
    Vector3 localProj;

    // The vector of (localVector - sphere point)
    Vector3 sphereVector;

    // The angle between localProj and the collider obj's z axis (still in collider obj's coordinate)
    float verticalAngle;

    // The angle between localProj and the collider obj's z axis (still in collider obj's coordinate)
    float horizontalAngle;

    // Touch point position in collider obj's coordinate (meter)
    Vector2 localTouchM = Vector2.zero;

    // Touch point position in world coordinate (millimeter)
    Vector2 worldTouchMM = Vector2.zero;
    #endregion

    public override void UpdateStatus(bool isTouch, bool isOverlapped)
    {
        IsTouching = isTouch;
        IsOverlapped = isOverlapped;
    }

    public override void UpdateTouchDotObj()
    {
        // Set the para for the Ray
        ray.origin = otherCollider.bounds.center;
        ray.direction = m_Collider.bounds.center - otherCollider.bounds.center;

        // Shoot the ray, get the touch point which is the intersection between ray and the collider
        m_Collider.Raycast(ray, out hitResult, 100.0f);
        touchPointObj.position = hitResult.point;
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

        radius = ((CapsuleCollider)otherCollider).radius;
        wholeHeight = ((CapsuleCollider)otherCollider).height;
        cylinderHeight = wholeHeight - 2 * radius;
        localVector = otherFinger.touchPointObj.localPosition;

        if (localVector.y > -cylinderHeight / 2 &&
            localVector.y < cylinderHeight / 2)     // In the cylinder part
        {
            localTouchM.x = localVector.y;

            // Project the localVector to the x-z plane
            localProj = Vector3.ProjectOnPlane(localVector, Vector3.up);
            verticalAngle = (localVector.x > 0) ? 1 : -1;
            verticalAngle *= Vector3.Angle(Vector3.forward, localProj);
            localTouchM.y = verticalAngle * Mathf.PI * radius / 180.0f;
        }
        else
        {
            // Use sphere vector here
            sphereVector = (localVector.y > 0) ?
                            localVector - new Vector3(0, cylinderHeight / 2, 0) :
                            localVector - new Vector3(0, -cylinderHeight / 2, 0);

            // Project sphere vector to the y-z plane 
            localProj = Vector3.ProjectOnPlane(sphereVector, Vector3.right);
            horizontalAngle = Vector3.Angle(Vector3.forward, localProj);
            localTouchM.x = (localVector.y > 0) ? 1 : -1;
            localTouchM.x *= horizontalAngle * Mathf.PI * radius / 180.0f + cylinderHeight / 2;

            // Project sphere vector to the x-z plane 
            localProj = Vector3.ProjectOnPlane(sphereVector, Vector3.up);
            verticalAngle = (localProj.x > 0) ? 1 : -1;
            verticalAngle *= Vector3.Angle(Vector3.forward, localProj);
            localTouchM.y = verticalAngle * Mathf.PI * radius / 180.0f;
        }

        // Transfer it to the world scale parameters but from meter to millimeter
        worldTouchMM = localTouchM * 1000.0f * otherCollider.transform.localScale.x;

        TouchPosition = worldTouchMM;
    }
}
