using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleActions : MonoBehaviour
{
    public enum FlagType
    {
        isTouched,
        isOverlapped,
    }
    public FlagType flagType = FlagType.isTouched;

    Toggle toggle;

    TouchDetection thumb;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        toggle = transform.GetComponent<Toggle>();
        thumb = GameObject.Find("thumb_tip").GetComponent<TouchDetection>();
    }

    // Update is called once per frame
    void Update()
    {
        switch(flagType)
        {
            case FlagType.isTouched:
                toggle.isOn = thumb.isTouching;
                break;
            case FlagType.isOverlapped:
                toggle.isOn = thumb.isOverlapped;
                break;
            default:
                break;
        }
    }


}
