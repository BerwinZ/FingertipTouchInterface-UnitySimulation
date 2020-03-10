using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;
using System;

public class SearchPanel : MonoBehaviour, IPanelAction
{
    InputField inputField;
    Text cntIndicator;
    long currentCnt;

    void Start()
    {
        inputField = transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<InputField>();
        cntIndicator = transform.GetChild(1).Find("Count").GetComponent<Text>();

        currentCnt = 0;
    }

    public bool PackData(
            out Dictionary<DOF, Dictionary<DataRange, float>> datasetPara)
    {
        datasetPara = new Dictionary<DOF, Dictionary<DataRange, float>>();

        if (!CheckValidation(out string msg))
        {
            WinFormTools.MessageBox(IntPtr.Zero, msg, "Invalid Input", 0);
            return false;
        }

        string text = inputField.text;
        float value;
        float.TryParse(text, out value);

        Dictionary<DataRange, float> singleItem = new Dictionary<DataRange, float>();
        singleItem[DataRange.step] = value;
        datasetPara[DOF.gamma1] = singleItem;

        return true;
    }

    bool CheckValidation(out string msg)
    {
        msg = "";
        string text = inputField.text;
        float value;
        if (!float.TryParse(text, out value))
        {
            msg = "Invalid Input";
            return false;
        }
        if(value <= 0)
        {
            msg = "The step cannot less than 0";
            return false;
        }
        if(value < 1e-6)
        {
            msg = ": The step is too small";
            return false;
        }
        return true;
    }

    public void UpdateTotalSampleCnt(long value)
    {

    }
    public void UpdateCurrentSampleCnt(long value)
    {
        currentCnt = value;
        cntIndicator.text = String.Format("Current: {0}", currentCnt);
    }
}