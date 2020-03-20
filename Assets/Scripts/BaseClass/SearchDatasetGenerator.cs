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
            _stepLength = datasetPara[DOF.gamma1][DataRange.step];
            // StartCoroutine(DFSSearchGeneratingCore());
            StartCoroutine(BFSSearchGeneratingCore());
        }
    }

    Queue<string> _openList;
    HashSet<string> _closedList;
    string _checkPattern;
    int _validCnt;
    float _stepLength;
    float[] _para = null;
    float[] _minBound = new float[] { -15, -15, -15, -15, -15, -15 };
    float[] _maxBound = new float[] { 15, 15, 15, 15, 15, 15 };
    IEnumerator BFSSearchGeneratingCore()
    {
        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName, streamDataGenerator);

        _openList = new Queue<string>();
        _closedList = new HashSet<string>();
        _validCnt = 0;

        // BFS Search
        _para = new float[] { 0, 0, 0, 0, 0, 0 };
        _openList.Enqueue(ConvertString(_para));

        while (_openList.Count > 0)
        {
            _para = ConvertFloat(_openList.Dequeue());
            _closedList.Add(ConvertString(_para));

            // Check current para, set the joint value
            for (int i = 0; i < _para.Length; i++)
            {
                yield return null;
                jointManager.SetJointValue((DOF)i, _para[i]);
            }

            if (IsValid)
            {
                // If valid, Save this data
                _validCnt++;
                datasetPanel.UpdateCurrentSampleCnt(_validCnt);

                SaveStreamDataToDisk();

                // Generate next values and add to open list
                // Iterate 12 next steps.
                // If they are not in the closed list, add it to open list
                for (int i = 0; i < _para.Length; i++)
                {
                    yield return null;

                    _para[i] += _stepLength;
                    _checkPattern = ConvertString(_para);
                    if (InBoundary(i, _para[i]) &&
                        !_closedList.Contains(_checkPattern) &&
                        !_openList.Contains(_checkPattern))
                    {
                        _openList.Enqueue(_checkPattern);
                    }
                    _para[i] -= _stepLength;

                    yield return null;

                    _para[i] -= _stepLength;
                    _checkPattern = ConvertString(_para);
                    if (InBoundary(i, _para[i]) &&
                        !_closedList.Contains(_checkPattern) &&
                        !_openList.Contains(_checkPattern))
                    {
                        _openList.Enqueue(_checkPattern);
                    }
                    _para[i] += _stepLength;
                }
                yield return null;
            }
        }

        // Close the writer
        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Valid Image: " + _validCnt, "Finish", 0);

        Debug.Log("Finish Generaing!");
    }

    IEnumerator DFSSearchGeneratingCore()
    {
        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName, streamDataGenerator);

        _closedList = new HashSet<string>();
        _validCnt = 0;

        // DFS Search
        _para = new float[] { 0, 0, 0, 0, 0, 0 };
        _closedList.Add(ConvertString(_para));

        yield return StartCoroutine(DFS());

        // Close the writer
        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Valid Image: " + _validCnt, "Finish", 0);

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

    float[] ConvertFloat(string str)
    {
        string[] strs = str.Split(',');
        float[] data = new float[6];
        for (int i = 0; i < data.Length; i++)
        {
            float.TryParse(strs[i], out data[i]);
        }
        return data;
    }

    bool InBoundary(int paraIndex, float value)
    {
        return _minBound[paraIndex] <= value &&
                value <= _maxBound[paraIndex];
    }

    IEnumerator DFS()
    {
        // Set the joint value
        for (int i = 0; i < _para.Length; i++)
        {
            yield return null;
            jointManager.SetJointValue((DOF)i, _para[i]);
        }

        yield return null;

        // Check current validation
        // If valid
        if (IsValid)
        {
            // If valid, Save this data
            _validCnt++;
            datasetPanel.UpdateCurrentSampleCnt(_validCnt);

            SaveStreamDataToDisk();

            // Iterate 12 next steps, if the _stepLength is not in the closed list
            // Add it to closed list, and search that _stepLength
            for (int i = 0; i < _para.Length; i++)
            {
                yield return null;

                _para[i] += _stepLength;
                if (!_closedList.Contains(ConvertString(_para)) &&
                    InBoundary(i, _para[i]))
                {
                    _closedList.Add(ConvertString(_para));
                    yield return StartCoroutine(DFS());
                }
                _para[i] -= _stepLength;

                _para[i] -= _stepLength;
                if (!_closedList.Contains(ConvertString(_para)) &&
                    InBoundary(i, _para[i]))
                {
                    _closedList.Add(ConvertString(_para));
                    yield return StartCoroutine(DFS());
                }
                _para[i] += _stepLength;
            }
        }
    }
}