using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPointCalc : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered!");
        ContactPoint2D[] points = new ContactPoint2D[10];
        collision.GetContacts(points);
        foreach(var value in points)
        {
            Debug.Log(value.point);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision!");
        ContactPoint2D[] points = new ContactPoint2D[collision.contactCount];
        collision.GetContacts(points);
        foreach (var value in points)
        {
            Debug.Log(value.point);
        }
    }
}
