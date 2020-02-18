using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderActions : MonoBehaviour
{
    enum HandleType
    {
        alpha1,
        alpha2,
        beta,
        gamma1,
        gamma2,
        gamma3,
    }

    [SerializeField]
    HandleType handleType = HandleType.alpha1;

    Slider slider;
    Text text;
    JointParaInput jointControl;

    // Start is called before the first frame update
    void Start()
    {
        slider = transform.GetComponent<Slider>();
        text = transform.Find("Number").GetComponent<Text>(); 
        jointControl = GameObject.Find("Hand Model").GetComponent<JointParaInput>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SendDataToJoint()
    {
        switch(handleType)
        {
            case HandleType.alpha1:
                jointControl.alpha1 = slider.value;
                break;
            case HandleType.alpha2:
                jointControl.alpha2 = slider.value;
                break;
            case HandleType.beta:
                jointControl.beta = slider.value;
                break;
            case HandleType.gamma1:
                jointControl.gamma1 = slider.value;
                break;
            case HandleType.gamma2:
                jointControl.gamma2 = slider.value;
                break;
            case HandleType.gamma3:
                jointControl.gamma3 = slider.value;
                break;
            default:
                break;
        }
        text.text = slider.value.ToString("#0.0");
    }
}
