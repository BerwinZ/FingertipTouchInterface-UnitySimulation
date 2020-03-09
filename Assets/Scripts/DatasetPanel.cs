using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;
using System;

/// <summary>
/// Control the action of the dataset panel.
/// 1. Check the whether the input filed is valid
/// 2. Generate the dictionary of dataset search space parameters
/// 3. Update UI of panel indicating the generating process 
/// </summary>
public class DatasetPanel : MonoBehaviour, IPanelAction
{
    Dictionary<DOF, Dictionary<DataRange, InputField>> inputFieldObj;

    Text cntIndicator;
    long totalCnt;
    long currentCnt;
    float percentage;
    Scrollbar percentageIndicator;

    // Start is called before the first frame update
    void Start()
    {
        inputFieldObj = new Dictionary<DOF, Dictionary<DataRange, InputField>>();
        for (int i = 0; i < 6; i++)
        {
            Dictionary<DataRange, InputField> singleItem = new Dictionary<DataRange, InputField>();
            Transform inputFieldCollection = transform.GetChild(1).GetChild(i);
            for (int j = 0; j < 3; j++)
            {
                singleItem[(DataRange)j] =
                    inputFieldCollection.GetChild(j + 4).GetComponent<InputField>();
            }
            inputFieldObj[(DOF)i] = singleItem;
        }
        cntIndicator = transform.GetChild(1).Find("Count").GetComponent<Text>();
        percentageIndicator = transform.GetChild(1).Find("Percentage").GetComponent<Scrollbar>();

        totalCnt = 0;
        currentCnt = 0;
        percentage = 0.0f;
        percentageIndicator.size = 0.0f;
    }

    /// <summary>
    /// Test function triggried from UI button
    /// </summary>
    public void PackDataBtn()
    {
        if (PackData(out Dictionary<DOF, Dictionary<DataRange, float>> data))
        {
            ShowItem(data);
        }
    }

    /// <summary>
    /// Pack data, store it in class and return a data
    /// </summary>
    /// <param name="Dictionary<string"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool PackData(out Dictionary<DOF, Dictionary<DataRange, float>> datasetPara)
    {
        datasetPara = new Dictionary<DOF, Dictionary<DataRange, float>>();

        if (!CheckValidation(out string msg))
        {
            WinFormTools.MessageBox(IntPtr.Zero, msg, "Invalid Input", 0);
            return false;
        }

        foreach (var oneDOF in inputFieldObj)
        {
            Dictionary<DataRange, float> singleItem = new Dictionary<DataRange, float>();
            foreach (var oneInputField in oneDOF.Value)
            {
                string text = oneInputField.Value.text;
                float value;
                float.TryParse(text, out value);
                singleItem[oneInputField.Key] = value;
            }
            datasetPara[oneDOF.Key] = singleItem;
        }

        return true;
    }

    /// <summary>
    /// Check whether the data obeys the rules
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    bool CheckValidation(out string msg)
    {
        msg = "";
        for (int i = 0; i < 6; i++)
        {
            List<float> list = new List<float>();
            for (int j = 0; j < 3; j++)
            {
                string text = inputFieldObj[(DOF)i][(DataRange)j].text;
                float value;
                if (!float.TryParse(text, out value))
                {
                    msg = (DOF)i + ": Invalid Input";
                    return false;
                }
                list.Add(value);
            }
            if (list[0] > list[1])
            {
                msg = (DOF)i + ": The min value is larger than the max value";
                return false;
            }
            if (list[2] < 0)
            {
                msg = (DOF)i + ": The step is not larger than 0";
                return false;
            }
            if (list[2] == 0 && list[0] != list[1])
            {
                msg = (DOF)i + ": The step cannot be 0";
                return false;
            }
            if (list[2] < 1e-8f && list[0] != list[1])
            {
                msg = (DOF)i + ": The step is too small";
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Function to show the datasetPara data
    /// </summary>
    void ShowItem(Dictionary<DOF, Dictionary<DataRange, float>> datasetPara)
    {
        foreach (var singleItem in datasetPara)
        {
            string text = singleItem.Key.ToString();
            foreach (var item in singleItem.Value)
            {
                text += " " + item.Key + "=" + item.Value;
            }
            Debug.Log(text);
        }
    }

    /// <summary>
    /// Set the total cnt of sample
    /// </summary>
    /// <param name="value"></param>
    public void UpdateTotalSampleCnt(long value)
    {
        totalCnt = value;
        cntIndicator.text = String.Format("Current: {0}  Total: {1}", currentCnt, totalCnt);
    }

    /// <summary>
    /// Set the current cnt of sample
    /// </summary>
    /// <param name="value"></param>
    public void UpdateCurrentSampleCnt(long value)
    {
        currentCnt = value;
        percentage = (float)currentCnt / (float)totalCnt;
        cntIndicator.text = String.Format("Current: {0}  Total: {1}", currentCnt, totalCnt);
        percentageIndicator.size = percentage;
    }
}
