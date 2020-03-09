using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

/// <summary>
/// 1. Send slider data to the JointManager.
/// 2. Get data from JoingManager when the program is in the searching status
/// </summary>
public class UISliderActions : MonoBehaviour
{

    [SerializeField]
    DOF handleType = DOF.alpha1;

    Slider slider;
    Text text;
    IJointMangerAction jointManager;
    IGameAction gameManager;

    // Start is called before the first frame update
    void Start()
    {
        slider = transform.GetComponent<Slider>();
        text = transform.Find("Number").GetComponent<Text>();
        gameManager = ProgramManager.Instance;

        jointManager = JointManager.Instance;

        gameManager.OnDatasetPanelChange += OnDatasetPanelOpen;
        OnDatasetPanelOpen(false);
    }

    void OnDatasetPanelOpen(bool flag)
    {
        // When the dataset panel doesn't open, send data to the joint
        if (!flag)
        {
            jointManager.OnJointUpdate -= UpdateValue;

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(delegate { SendDataToJoint(); });
            slider.onValueChanged.AddListener(delegate { UpdateText(); });

        }
        else // When the dataset panel opens, substribe the joint update event
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(delegate { UpdateText(); });

            jointManager.OnJointUpdate += UpdateValue;
        }
    }

    void SendDataToJoint()
    {
        jointManager.SetJointValue(handleType, slider.value);
    }

    void UpdateText()
    {
        text.text = slider.value.ToString("#0.0");
    }

    void UpdateValue()
    {
        slider.value = jointManager.GetJointValue(handleType);
    }

}
