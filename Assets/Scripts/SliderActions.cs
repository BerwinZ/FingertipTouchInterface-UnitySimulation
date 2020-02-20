using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderActions : MonoBehaviour
{

    [SerializeField]
    JointType.DOF handleType = JointType.DOF.alpha1;

    Slider slider;
    Text text;
    JointManager jointControl;

    // Start is called before the first frame update
    void Start()
    {
        slider = transform.GetComponent<Slider>();
        text = transform.Find("Number").GetComponent<Text>();
        jointControl = GameObject.Find("Hand Model").GetComponent<JointManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SendDataToJoint()
    {
        switch (handleType)
        {
            case JointType.DOF.alpha1:
                jointControl.alpha1 = slider.value;
                break;
            case JointType.DOF.alpha2:
                jointControl.alpha2 = slider.value;
                break;
            case JointType.DOF.beta:
                jointControl.beta = slider.value;
                break;
            case JointType.DOF.gamma1:
                jointControl.gamma1 = slider.value;
                break;
            case JointType.DOF.gamma2:
                jointControl.gamma2 = slider.value;
                break;
            case JointType.DOF.gamma3:
                jointControl.gamma3 = slider.value;
                break;
            default:
                break;
        }
        text.text = slider.value.ToString("#0.0");
    }
}
