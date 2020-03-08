using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

/// <summary>
/// Detect the touch action, including the following parameters
/// 1. If touched
/// 2. If two fingers are overlapped
/// 3. The 2-d touch position of the thumb related to the index finger
/// </summary>
public class TouchDetection : MonoBehaviour, IFingerAction
{
    // The type of this finger
    [SerializeField]
    Finger fingertipType = Finger.thumb;
    public Finger FingerType => fingertipType;

    public event StatusUpdateHandler OnTouchStatusChange;
    public event StatusUpdateHandler OnOverlapStatusChange;
    public event PositionChangeHandler OnTouchPositionChange;

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
        private set
        {
            isTouching = value;
            OnTouchStatusChange?.Invoke(value);
        }
    }

    public bool IsOverlapped
    {
        get => isOverlapped;
        private set
        {
            isOverlapped = value;
            OnOverlapStatusChange?.Invoke(value);
        }
    }

    public Vector2 TouchPosition
    {
        get => touchPosition;
        private set
        {
            touchPosition = value;
            OnTouchPositionChange?.Invoke(value);
        }
    }

    IJointMangerAction jointManager;

    // Start is called before the first frame update
    void Start()
    {
        targetFingerType = (Finger)(((int)fingertipType + 1) % 2);

        m_Collider = GetComponent<Collider>();

        otherFinger = ScriptFind.FindTouchDetection(targetFingerType);
        otherCollider = otherFinger.transform.GetComponent<Collider>();

        // Get the touch point object
        touchPointObj = transform.GetChild(0);
        touchPointObjMesh = touchPointObj.GetComponent<MeshRenderer>();

        // Register the events
        OnTouchStatusChange += ChangeTouchPointObjMesh;

        // Set the values
        IsTouching = false;
        IsOverlapped = false;
        TouchPosition = Vector2.zero;

        // Subscribe the joint update event
        jointManager = JointManager.Instance;
        jointManager.OnJointUpdate += DetectTouchingStatus;
    }

    void ChangeTouchPointObjMesh(bool flag)
    {
        touchPointObjMesh.enabled = flag;
    }



    #region ParaforAccurateTouchDetection
    // Type of the finger that is isTouching with
    Finger targetFingerType;
    // This collider
    Collider m_Collider;
    // The touch point
    Transform touchPointObj;
    MeshRenderer touchPointObjMesh;

    // other finger's script
    TouchDetection otherFinger;
    // other finger's collider
    Collider otherCollider;

    // Ray to intersect with this collider  
    Ray ray = new Ray();

    // Ray hit result          
    RaycastHit hitResult;

    // Ray hit colliders
    Collider[] hitColliders;

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

    /// <summary>
    /// Detect whether this finger is colliding with the other collider
    /// </summary>
    void DetectTouchingStatus()
    {
        hitColliders = Physics.OverlapSphere(
            otherCollider.ClosestPoint(m_Collider.bounds.center), 0.0001f);

        bool flag = false;
        foreach (var coll in hitColliders)
        {
            flag |= (coll == m_Collider);
        }

        if (flag)
        {
            IsTouching = true;
            DetectOverlapStatus(otherCollider);
        }
        else
        {
            IsTouching = false;
            IsOverlapped = false;
        }
    }

    /// <summary>
    /// Accurate detecting the touch point and set other flags
    /// </summary>
    /// <param name="other">the other collider that collides with</param>
    void DetectOverlapStatus(Collider other)
    {
        if (other == null)
            return;

        // Set the para for the Ray
        ray.origin = other.bounds.center;
        ray.direction = m_Collider.bounds.center - other.bounds.center;

        // Shoot the ray, get the touch point which is the intersection between ray and the collider
        m_Collider.Raycast(ray, out hitResult, 100.0f);

        touchPointObj.position = hitResult.point;

        // Set overlapped true if the distance between two touch point is too large
        IsOverlapped =
            Vector3.Distance(
                touchPointObj.position,
                otherFinger.touchPointObj.position) > 1e-3;

        // Calculate the touch point
        if (!IsOverlapped)
        {
            CalcTouchPosition(other);
        }
    }

    /// <summary>
    /// Calculate the touch point's coordinate and store them in the paramters. 
    /// </summary>
    /// <param name="other"></param>
    /// <returns name="worldPositionMM"></returns>
    void CalcTouchPosition(Collider other)
    {
        // Get the coordinate
        // X axis faces up (to world)
        // Y axis faces right (to world), the direction of axle of capsule
        // Z axis faces outside (to world)

        radius = ((CapsuleCollider)other).radius;
        wholeHeight = ((CapsuleCollider)other).height;
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
        worldTouchMM = localTouchM * 1000.0f * other.transform.localScale.x;

        TouchPosition = worldTouchMM;
    }
}
