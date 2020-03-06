using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class BackupMethods: MonoBehaviour
{
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

        // Debug.Log(fingertipType.ToString() + " " + contactOffset.ToString("F4"));
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