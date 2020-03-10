using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;

public class SearchDatasetGenerator : DatasetGeneratorBase
{
    public override void StartGeneratingDataset()
    {
        Debug.Log("Start Searching...");

        Dictionary<DOF, Dictionary<DataRange, float>> datasetPara = new Dictionary<DOF, Dictionary<DataRange, float>>();

        if (datasetPanel.PackData(out datasetPara))
        {
            step = datasetPara[DOF.gamma1][DataRange.step];
            StartCoroutine(SearchGeneratingCore());
        }
    }

    HashSet<string> closedList;
    int validCnt;
    float step;
    float[] para = null;
    IEnumerator SearchGeneratingCore()
    {
        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName, streamDataGenerator);

        closedList = new HashSet<string>();
        validCnt = 0;

        // DFS Search
        para = new float[] { 0, 0, 0, 0, 0, 0 };
        closedList.Add(ConvertString(para));
        yield return StartCoroutine(DFS());

        // Close the writer
        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Valid Image: " + validCnt, "Finish", 0);

        Debug.Log("Finish Generaing!");
    }

    string ConvertString(float[] data)
    {
        string str = "";
        for (int i = 0; i < data.Length; i++)
        {
            if (i == data.Length - 1)
            {
                str += data[i].ToString();
            }
            else
            {
                str += data[i].ToString() + ",";
            }
        }
        return str;
    }

    IEnumerator DFS()
    {
        // Set the joint value
        for (int i = 0; i < para.Length; i++)
        {
            yield return null;
            jointManager.SetJointValue((DOF)i, para[i]);
        }

        yield return null;

        // Check current validation
        // If valid
        if (IsValid)
        {
            // If valid, Save this data
            validCnt++;
            datasetPanel.UpdateCurrentSampleCnt(validCnt);

            SaveStreamDataToDisk();

            // Iterate 12 next steps, if the step is not in the closed list
            // Add it to closed list, and search that step
            for (int i = 0; i < para.Length; i++)
            {
                yield return null;

                para[i] += step;
                if (!closedList.Contains(ConvertString(para)))
                {
                    closedList.Add(ConvertString(para));
                    yield return StartCoroutine(DFS());
                }
                para[i] -= step;

                para[i] -= step;
                if (!closedList.Contains(ConvertString(para)))
                {
                    closedList.Add(ConvertString(para));
                    yield return StartCoroutine(DFS());
                }
                para[i] += step;
            }
        }
    }
}