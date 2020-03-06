using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public delegate void JointUpdateHander();

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

    // TODO: decide what kind of properties left
    public float Gamma1 => gamma1;
    public float Gamma2 => gamma2;
    public float Gamma3 => gamma3;
    public float Alpha1 => alpha1;
    public float Alpha2 => alpha2;
    public float Beta => beta;

    public float this[DOF jointType]
    {
        get
        {
            switch (jointType)
            {
                case DOF.alpha1:
                    return alpha1;

                case DOF.alpha2:
                    return alpha2;

                case DOF.beta:
                    return beta;

                case DOF.gamma1:
                    return gamma1;

                case DOF.gamma2:
                    return gamma2;

                case DOF.gamma3:
                    return gamma3;

                default:
                    return 0;
            }

        }
        set
        {
            switch (jointType)
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
            UpdateJointObjTransform();
            JointUpdatePublisher?.Invoke();
        }
    }

    [Header("Debug Chocie")]
    public bool controlInEditor = false;

    public JointUpdateHander JointUpdatePublisher;

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

        // Initialize publish
        StartCoroutine(Initialize());   //??
    }

    /// <summary>
    /// TODO: fix it
    /// </summary>
    /// <returns></returns>
    IEnumerator Initialize()
    {
        yield return new WaitForSeconds(2.0f);
        UpdateJointObjTransform();
        // JointUpdatePublisher?.Invoke();
    }

    void Update()
    {
        if (controlInEditor)
        {
            UpdateJointObjTransform();
            JointUpdatePublisher?.Invoke();
        }
    }

    void UpdateJointObjTransform()
    {
        float[] gama = new float[3] { gamma1, gamma2, gamma3 };

        for (int index = 0; index < 3; index++)
        {
            indexFingerJoints[index].localEulerAngles = defaultAngles[indexFingerJoints[index]] + new Vector3(0, gama[index], 0);
        }

        thumbJoints[0].localEulerAngles = defaultAngles[thumbJoints[0]] + new Vector3(0, alpha1, beta);
        thumbJoints[1].localEulerAngles = defaultAngles[thumbJoints[1]] + new Vector3(0, alpha2, 0);
    }
}
