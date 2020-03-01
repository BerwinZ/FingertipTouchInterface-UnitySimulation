using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;
using System;

public class DatasetManager : Singleton<DatasetManager>
{
    Transform[] itemObjects;

    string[] itemsHeader = { "gamma1", "gamma2", "gamma3", "alpha1", "alpha2", "beta" };

    string[] singleItemHeader = { "min", "max", "step" };

    Text cntIndicator;
    long totalCnt;
    long currentCnt;
    Scrollbar percentageIndicator;
    float percentage;

    // Start is called before the first frame update
    void Start()
    {
        itemObjects = new Transform[6];
        for (int i = 0; i < 6; i++)
        {
            itemObjects[i] = transform.GetChild(1).GetChild(i);
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
        if (PackData(out Dictionary<string, Dictionary<string, float>> data))
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
    public bool PackData(out Dictionary<string, Dictionary<string, float>> items)
    {
        items = new Dictionary<string, Dictionary<string, float>>();

        if (!CheckValidation(out string msg))
        {
            WinFormTools.MessageBox(IntPtr.Zero, msg, "Invalid Input", 0);
            return false;
        }

        for (int i = 0; i < 6; i++)
        {
            Dictionary<string, float> singleItem = new Dictionary<string, float>();
            for (int j = 0; j < 3; j++)
            {
                string text = itemObjects[i].GetChild(j + 4).GetComponent<InputField>().text;
                float value;
                float.TryParse(text, out value);
                singleItem[singleItemHeader[j]] = value;
            }
            items[itemsHeader[i]] = singleItem;
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
                string text = itemObjects[i].GetChild(j + 4).GetComponent<InputField>().text;
                float value;
                float.TryParse(text, out value);
                list.Add(value);
            }
            if (list[0] > list[1])
            {
                msg = itemsHeader[i] + ": The min value is larger than the max value";
                return false;
            }
            if (list[2] < 0)
            {
                msg = itemsHeader[i] + ": The step is not larger than 0";
                return false;
            }
            if (list[2] == 0 && list[0] != list[1])
            {
                msg = itemsHeader[i] + ": The step cannot be 0";
                return false;
            }
            if (list[2] < 1e-8f && list[0] != list[1])
            {
                msg = itemsHeader[i] + ": The step is too small";
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Function to show the items data
    /// </summary>
    void ShowItem(Dictionary<string, Dictionary<string, float>> items)
    {
        foreach (var singleItem in items)
        {
            string text = singleItem.Key;
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
