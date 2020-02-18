using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointParaInput : MonoBehaviour
{
    [Header("Index Finger")]
    [Range(-20, 20)]
    public float gama1 = 0;
    [Range(-20, 20)]
    public float gama2 = 0;
    [Range(-20, 20)]
    public float gama3 = 0;

    [Header("Thumb")]
    [Range(-20, 20)]
    public float alpha1 = 0;
    [Range(-20, 20)]
    public float alpha2 = 0;
    [Range(-20, 20)]
    public float beta = 0;

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

    // Update is called once per frame
    void Update()
    {
        SetJointsPara();
    }

    /// <summary>
    /// Set the parameters for the joint
    /// </summary>
    void SetJointsPara()
    {
        float[] gama = new float[3] { gama1, gama2, gama3 };

        for (int index = 0; index < 3; index++)
        {
            indexFingerJoints[index].localEulerAngles = defaultAngles[indexFingerJoints[index]] + new Vector3(0, gama[index], 0);
        }

        thumbJoints[0].localEulerAngles = defaultAngles[thumbJoints[0]] + new Vector3(0, alpha1, beta);
        thumbJoints[1].localEulerAngles = defaultAngles[thumbJoints[1]] + new Vector3(0, alpha2, 0);
    }
}
