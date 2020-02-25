using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Send slider data to the JointManager.
/// </summary>
public class SliderActions : MonoBehaviour
{

    [SerializeField]
    JointType.DOF handleType = JointType.DOF.alpha1;

    Slider slider;
    Text text;

    // Start is called before the first frame update
    void Start()
    {
        slider = transform.GetComponent<Slider>();
        text = transform.Find("Number").GetComponent<Text>();
        slider.onValueChanged.AddListener(delegate { SendDataToJoint(); });
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
                JointManager.Instance.alpha1 = slider.value;
                break;
            case JointType.DOF.alpha2:
                JointManager.Instance.alpha2 = slider.value;
                break;
            case JointType.DOF.beta:
                JointManager.Instance.beta = slider.value;
                break;
            case JointType.DOF.gamma1:
                JointManager.Instance.gamma1 = slider.value;
                break;
            case JointType.DOF.gamma2:
                JointManager.Instance.gamma2 = slider.value;
                break;
            case JointType.DOF.gamma3:
                JointManager.Instance.gamma3 = slider.value;
                break;
            default:
                break;
        }
        text.text = slider.value.ToString("#0.0");
    }
}
