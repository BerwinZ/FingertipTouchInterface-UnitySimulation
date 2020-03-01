using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

/// <summary>
/// Send slider data to the JointManager.
/// </summary>
public class SliderActions : MonoBehaviour
{

    [SerializeField]
    DOF handleType = DOF.alpha1;

    Slider slider;
    Text text;

    // Start is called before the first frame update
    void Start()
    {
        slider = transform.GetComponent<Slider>();
        text = transform.Find("Number").GetComponent<Text>();
        slider.onValueChanged.AddListener(delegate { SendDataToJoint(); });
    }

    public void SendDataToJoint()
    {
        JointManager.Instance.UpdateParaValue(handleType, slider.value);
        text.text = slider.value.ToString("#0.0");
    }
}
