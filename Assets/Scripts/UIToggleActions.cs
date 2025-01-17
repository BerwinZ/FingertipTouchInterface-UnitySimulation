﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

/// <summary>
/// Get touching status from JointManger
/// </summary>
public class UIToggleActions : MonoBehaviour
{
    public enum FlagType
    {
        isTouched,
        isOverlapped,
    }
    public FlagType flagType = FlagType.isTouched;

    Toggle toggle;

    IFingerAction thumb;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        toggle = transform.GetComponent<Toggle>();
        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        switch (flagType)
        {
            case FlagType.isTouched:
                thumb.OnTouchStatusChange += ChangeToggleState;
                break;
            case FlagType.isOverlapped:
                thumb.OnOverlapStatusChange += ChangeToggleState;
                break;
            default:
                break;
        }
    }

    void ChangeToggleState(object sender, bool flag)
    {
        toggle.isOn = flag;
    }
}
