using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

/// <summary>
/// This class control the values of 6 DOF of the thumb and index finger joints.
/// </summary>
public class JointManager : Singleton<JointManager>
{
    [Header("Index Finger")]
    [SerializeField, Range(-20, 20)]
    float gamma1 = 0;
    [SerializeField, Range(-20, 20)]
    float gamma2 = 0;
    [SerializeField, Range(-20, 20)]
    float gamma3 = 0;

    [Header("Thumb")]
    [SerializeField, Range(-20, 20)]
    float alpha1 = 0;
    [SerializeField, Range(-20, 20)]
    float alpha2 = 0;
    [SerializeField, Range(-20, 20)]
    float beta = 0;

    public bool controlInEditor = false;

    Transform[] indexFingerJoints;
    Transform[] thumbJoints;
    Dictionary<Transform, Vector3> defaultAngles;

    TouchDetection thumb;
    TouchDetection index;

    // Start is called before the first frame update
    void Start()
    {
        indexFingerJoints = new Transform[3];
        indexFingerJoints[0] = GameObject.Find("index_l").transform;
        indexFingerJoints[1] = GameObject.Find("index_l_001").transform;
        indexFingerJoints[2] = GameObject.Find("index_l_002").transform;

        thumbJoints = new Transform[2];
        thumbJoints[0] = GameObject.Find("thumb_l_001").transform;
        thumbJoints[1] = GameObject.Find("thumb_l_002").transform;

        defaultAngles = new Dictionary<Transform, Vector3>();
        foreach (var item in indexFingerJoints)
        {
            defaultAngles[item] = item.localEulerAngles;
        }
        foreach (var item in thumbJoints)
        {
            defaultAngles[item] = item.localEulerAngles;
        }

        // Get thumb and index finger
        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        index = ScriptFind.FindTouchDetection(Finger.index);
    }

    void Update()
    {
        if(controlInEditor)
        {
            SetJointsPara();
        }
    }

    public void UpdateParaValue(DOF joint, float value)
    {
        switch (joint)
        {
            case DOF.alpha1:
                alpha1 = value;
                break;
            case DOF.alpha2:
                alpha2 = value;
                break;
            case DOF.beta:
                beta = value;
                break;
            case DOF.gamma1:
                gamma1 = value;
                break;
            case DOF.gamma2:
                gamma2 = value;
                break;
            case DOF.gamma3:
                gamma3 = value;
                break;
            default:
                break;
        }
        SetJointsPara();
    }


    /// <summary>
    /// Set the parameters for the joint
    /// </summary>
    void SetJointsPara()
    {
        float[] gama = new float[3] { gamma1, gamma2, gamma3 };

        for (int index = 0; index < 3; index++)
        {
            indexFingerJoints[index].localEulerAngles = defaultAngles[indexFingerJoints[index]] + new Vector3(0, gama[index], 0);
        }

        thumbJoints[0].localEulerAngles = defaultAngles[thumbJoints[0]] + new Vector3(0, alpha1, beta);
        thumbJoints[1].localEulerAngles = defaultAngles[thumbJoints[1]] + new Vector3(0, alpha2, 0);
    }

#region ForDataSaveInDisk
    /// <summary>
    /// Generate the header line for the .csv file
    /// </summary>
    /// <returns></returns>
    public string GenerateStreamHeader()
    {
        string header = "Gamma1," + "Gamma2," + "Gamma3," + "Alpha1,"  + "Alpha2," + "Beta," + "thumb_x," + "thumb_y," + "index_x," + "index_y," + "ImgName";
        return header;
    }

    /// <summary>
    /// Generate the data line for the .csv file
    /// </summary>
    /// <param name="imgName"></param>
    /// <returns></returns>
    public string GenerateStreamData(string imgName)
    {
        string data = 
            gamma1.ToString("F2") + "," + 
            gamma2.ToString("F2") + "," +
            gamma3.ToString("F2") + "," +
            alpha1.ToString("F2") + "," +
            alpha2.ToString("F2") + "," +
            beta.ToString("F2") + "," +
            thumb.TouchPosition.x.ToString("F2") + "," +
            thumb.TouchPosition.y.ToString("F2") + "," +
            index.TouchPosition.x.ToString("F2") + "," +
            index.TouchPosition.y.ToString("F2") + "," +
            imgName;
        return data;
    }
#endregion ForDataSaveInDisk
}
