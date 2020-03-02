using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;


public delegate void StatusUpdateHandler(bool flag);
public delegate void PositionChangeHandler(Vector2 pos);

/// <summary>
/// Detect the touch action, including the following parameters
/// 1. If touched
/// 2. If two fingers are overlapped
/// 3. The 2-d touch position of the thumb related to the index finger
/// </summary>
public class TouchDetection : MonoBehaviour
{
    // The type of this finger
    public Finger fingertipType = Finger.thumb;

    // Type of the finger that is isTouching with
    Finger targetFingerType;

    Collider m_Collider;


    public event StatusUpdateHandler TouchStatusUpdatePublisher;
    // Whether this finger is isTouching by another
    bool isTouching;
    public bool IsTouching
    {
        get { return isTouching; }
        private set
        {
            isTouching = value;
            TouchStatusUpdatePublisher?.Invoke(value);
        }
    }

    public event StatusUpdateHandler OverlapStatusUpdatePublisher;
    // Whether this finger is overlapped too much
    bool isOverlapped;
    public bool IsOverlapped
    {
        get { return isOverlapped; }
        private set
        {
            isOverlapped = value;
            OverlapStatusUpdatePublisher?.Invoke(value);
        }
    }


    // The touch point
    Transform touchPointObj;
    MeshRenderer touchPointObjMesh;
    public event PositionChangeHandler TouchPositionUpdatePublisher;
    // For thumb, the touch position is the X-Y position relative to the index finger coordinate. 
    // For index finger, the touch position is the X-Y position relative to the thumb coordinate. 
    Vector2 touchPosition;
    public Vector2 TouchPosition
    {
        get { return touchPosition; }
        private set
        {
            touchPosition = value;
            TouchPositionUpdatePublisher?.Invoke(value);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        targetFingerType = (Finger)(((int)fingertipType + 1) % 2);

        m_Collider = GetComponent<Collider>();

        // Get the touch point object
        touchPointObj = transform.GetChild(0);
        touchPointObjMesh = touchPointObj.GetComponent<MeshRenderer>();

        // Register the events
        TouchStatusUpdatePublisher += OnUpdateTouchStatus;

        // Set the values
        IsTouching = false;
        IsOverlapped = false;
        TouchPosition = Vector2.zero;
    }

    void OnUpdateTouchStatus(bool flag)
    {
        touchPointObjMesh.enabled = flag;
    }

    private void OnTriggerEnter(Collider other)
    {
        DetectTouching(other, true);
    }

    private void OnTriggerStay(Collider other)
    {
        DetectTouching(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        DetectTouching(other, false);
    }

    #region ParaforAccurateTouchDetection
    // other finger's script
    TouchDetection otherFinger;

    // Ray to intersect with this collider  
    Ray ray = new Ray();

    // Ray hit result          
    RaycastHit hit;

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
    #endregion ParaforAccurateTouchDetection

    void DetectTouching(Collider other, bool isEntry)
    {
        otherFinger = other.transform.GetComponent<TouchDetection>();
        if (otherFinger == null || otherFinger.fingertipType != targetFingerType)
            return;

        IsTouching = isEntry;

        // Calculate the position of touch point
        if (IsTouching)
        {
            // Set the para for the Ray
            ray.origin = other.bounds.center;
            ray.direction = m_Collider.bounds.center - other.bounds.center;

            // Shoot the ray, get the touch point which is the intersection between ray and the collider
            m_Collider.Raycast(ray, out hit, 100.0f);
            touchPointObj.position = hit.point;

            // Set overlapped true if the distance between two touch point is too large
            IsOverlapped =
                Vector3.Distance(
                    touchPointObj.position,
                    otherFinger.touchPointObj.position) > 1e-3;

            // Calculate the touch point
            if (!IsOverlapped)
            {
                TouchPosition = CalcTouchPosition(other);
            }
        }
        else if (!IsTouching)
        {
            IsOverlapped = false;
        }
    }

    /// <summary>
    /// Calculate the touch point's coordinate and store them in the paramters. 
    /// </summary>
    /// <param name="other"></param>
    /// <returns name="worldPositionMM"></returns>
    Vector2 CalcTouchPosition(Collider other)
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

        return worldTouchMM;
    }

    #region TestColliderFunctions
    void TestBounds()
    {
        Collider m_Collider;
        Vector3 m_Center;
        Vector3 m_Size, m_Min, m_Max;

        //Fetch the Collider from the GameObject
        m_Collider = GetComponent<Collider>();
        //Fetch the center of the Collider volume
        m_Center = m_Collider.bounds.center;
        //Fetch the size of the Collider volume
        m_Size = m_Collider.bounds.size;
        //Fetch the minimum and maximum bounds of the Collider volume
        m_Min = m_Collider.bounds.min;
        m_Max = m_Collider.bounds.max;
        //Closest point
        Vector3 closetPointOnBound = m_Collider.bounds.ClosestPoint(new Vector3(0, 0, 0));

        //Output to the console the center and size of the Collider volume
        Debug.Log("Object World Position :" + transform.position.ToString("F4"));
        Debug.Log("Collider Center : " + m_Center.ToString("F4"));
        Debug.Log("Collider Size : " + m_Size.ToString("F4"));
        Debug.Log("Collider bound Minimum : " + m_Min.ToString("F4"));
        Debug.Log("Collider bound Maximum : " + m_Max.ToString("F4"));
        Debug.Log("Closest Point on the bounds to the original point : " + closetPointOnBound.ToString("F4"));
    }

    void TestContact()
    {
        Collider m_Collider;
        m_Collider = GetComponent<Collider>();

        // Settings in the physics para
        float contactOffset = m_Collider.contactOffset;

        Debug.Log(fingertipType.ToString() + " " + contactOffset.ToString("F4"));
    }

    GameObject targetPointObj;
    GameObject objOnCollider;
    GameObject objOnBound;
    GameObject objBoundRay;
    void TestClosestPoint()
    {
        Collider m_Collider;
        m_Collider = GetComponent<Collider>();

        if (targetPointObj == null)
        {
            targetPointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetPointObj.transform.localScale = Vector3.one * 0.01f;

            objOnCollider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objOnCollider.transform.localScale = Vector3.one * 0.001f;

            objOnBound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objOnBound.transform.localScale = Vector3.one * 0.001f;
            objOnBound.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f);

            objBoundRay = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objBoundRay.transform.localScale = Vector3.one * 0.001f;
            objBoundRay.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 1.0f, 0.0f);
        }

        objOnCollider.transform.position = m_Collider.ClosestPoint(targetPointObj.transform.position);

        objOnBound.transform.position = m_Collider.ClosestPointOnBounds(targetPointObj.transform.position);

        Ray ray = new Ray(targetPointObj.transform.position, m_Collider.bounds.center - targetPointObj.transform.position);
        RaycastHit hit;

        if (m_Collider.Raycast(ray, out hit, 100.0f))
        {
            objBoundRay.transform.position = hit.point;
        }
    }
    #endregion TestColliderFunctions
}
