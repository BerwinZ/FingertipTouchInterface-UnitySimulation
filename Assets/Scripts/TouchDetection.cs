using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detect the touch action, including the following parameters
/// 1. If touched
/// 2. If two fingers are overlapped
/// 3. The 2-d touch position of the thumb related to the index finger
/// </summary>
public class TouchDetection : MonoBehaviour
{
    // The type of this finger
    public JointType.Finger fingertipType = JointType.Finger.thumb;

    // Type of the finger that is isTouching with
    JointType.Finger targetFingerType;

    // Whether this finger is isTouching by another
    public bool isTouching { get; private set; }

    // Whether this finger is overlapped too much
    public bool isOverlapped { get; private set; }

    Collider m_Collider;

    // The touch point
    public Transform touchPoint { get; private set; }


    // For thumb, the touch position is the X-Y position relative to the index finger coordinate. 
    // For index finger, the touch position is the X-Y position relative to the thumb coordinate. 
    public Vector2 touchPosition { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        targetFingerType = (JointType.Finger)(((int)fingertipType + 1) % 2);

        isTouching = false;
        isOverlapped = false;

        m_Collider = GetComponent<Collider>();

        touchPoint = transform.GetChild(0);
        touchPosition = Vector2.zero;

        // Test Collider Code
        // if (fingertipType == JointType.Finger.thumb)
        // {
        //     TestClosestPoint();
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(fingertipType.ToString() + " isTouching: " + isTouching);

        // Test Collider Code
        // if (fingertipType == JointType.Finger.thumb)
        // {
        //     TestClosestPoint();
        // }
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

    TouchDetection otherFinger;
    Ray ray = new Ray();
    RaycastHit hit;
    float radius;
    float wholeHeight;
    float cylinHeight;
    Vector3 localVector;
    Vector3 worldVector;
    Vector3 worldProj; 
    float cylinderAngle;
    Vector2 tmpTouch = Vector2.zero;
    void DetectTouching(Collider other, bool isEntry)
    {
        otherFinger = other.transform.GetComponent<TouchDetection>();
        if (otherFinger == null || otherFinger.fingertipType != targetFingerType)
            return;

        isTouching = isEntry;

        // Calculate the position of touch point
        if (isTouching)
        {
            // Link the two center of the collider
            ray.origin = other.bounds.center;
            ray.direction = m_Collider.bounds.center - other.bounds.center;

            // Touch point is the intersection between ray and the collider
            m_Collider.Raycast(ray, out hit, 100.0f);
            touchPoint.position = hit.point;

            // If the distance between two touch point is too large
            isOverlapped = Vector3.Distance(touchPoint.position, otherFinger.touchPoint.position) > 0.002f;

            // Get the coordinate
            // X axis faces up
            // Y axis faces right, the direction of axle
            // Z axis faces outside
            radius = ((CapsuleCollider)other).radius;
            wholeHeight = ((CapsuleCollider)other).height;
            cylinHeight = wholeHeight - 2 * radius;
            localVector = otherFinger.touchPoint.localPosition;
            worldVector = other.transform.TransformVector(localVector);

// TODO: Solve the coordinate
            if (localVector.y > cylinHeight / 2)   // In the top sphere part
            {
                Debug.Log("Top Sphere");
            }
            else if (localVector.y < -cylinHeight / 2)  // In the bottom sphere part
            {
                Debug.Log("Bottom Sphere");
            }
            else    // In the cylinder part
            {
                worldProj = Vector3.ProjectOnPlane(worldVector, other.transform.up);

                cylinderAngle = (localVector.x > 0) ? 1 : -1;
                cylinderAngle *= Vector3.Angle(other.transform.forward, worldProj);

                tmpTouch.y = cylinderAngle * Mathf.PI * radius / 180.0f;
                tmpTouch.x = localVector.y;
            }
            touchPosition = tmpTouch;

        }
        else if (!isTouching)
        {
            isOverlapped = false;
            touchPosition = Vector2.zero;
        }
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
