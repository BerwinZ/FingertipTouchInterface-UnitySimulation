using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Transform[] indexFingerJoints;
    Transform[] thumbJoints;
    Dictionary<Transform, Vector3> defaultAngles;

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
    }

    public void UpdateParaValue(JointType.DOF joint, float value)
    {
        switch (joint)
        {
            case JointType.DOF.alpha1:
                alpha1 = value;
                break;
            case JointType.DOF.alpha2:
                alpha2 = value;
                break;
            case JointType.DOF.beta:
                beta = value;
                break;
            case JointType.DOF.gamma1:
                gamma1 = value;
                break;
            case JointType.DOF.gamma2:
                gamma2 = value;
                break;
            case JointType.DOF.gamma3:
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

    [System.Serializable]
    public class FileDataForm
    {
        float gamma1 = 0;
        float gamma2 = 0;
        float gamma3 = 0;
        float alpha1 = 0;
        float alpha2 = 0;
        float beta = 0;
        string imgName = "";

        public FileDataForm(float g1, float g2, float g3, float a1, float a2, float b, string name)
        {
            gamma1 = g1;
            gamma2 = g2;
            gamma3 = g3;
            alpha1 = a1;
            alpha2 = a2;
            beta = b;
            imgName = name;
        }

        public void PrintPara()
        {
            Debug.Log(
                "gamma1=" + gamma1 + ',' + 
                "gamma2=" + gamma2 + ',' +
                "gamma3=" + gamma3 + ',' +
                "alpha1=" + alpha1 + ',' +
                "alpha2=" + alpha2 + ',' +
                "beta=" + beta + ',' +
                "imgName=" + imgName + ',' 
            );
        }
    }

    public FileDataForm GenerateDataFile(string imgName)
    {
        FileDataForm fileData = new FileDataForm(
            gamma1,
            gamma2,
            gamma3,
            alpha1,
            alpha2,
            beta,
            imgName
        );
        return fileData;
    }
}
