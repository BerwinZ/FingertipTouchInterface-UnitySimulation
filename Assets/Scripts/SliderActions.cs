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

        ScreenshotManager.Instance.DatasetPanelPublisher += OnDatasetPanelOpen;
        OnDatasetPanelOpen(false);
    }

    void OnDatasetPanelOpen(bool flag)
    {
        if (!flag)
        {
            JointManager.Instance.JointUpdatePublisher -= UpdateValue;

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(delegate { SendDataToJoint(); });
            slider.onValueChanged.AddListener(delegate { UpdateText(); });
            
        }
        else
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(delegate { UpdateText(); });

            JointManager.Instance.JointUpdatePublisher += UpdateValue;
        }
    }

    void SendDataToJoint()
    {
        JointManager.Instance.UpdateDOFValue(handleType, slider.value);
    }

    void UpdateText()
    {
        text.text = slider.value.ToString("#0.0");
    }

    void UpdateValue()
    {
        slider.value = JointManager.Instance.GetDOFValue(handleType);
    }

}
