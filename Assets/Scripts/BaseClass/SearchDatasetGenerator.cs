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

    Queue<float[]> _openList;
    HashSet<float[]> _closedListBFS;
    HashSet<string> _closedList;
    int _validCnt;
    float _stepLength;
    float[] _para = null;
    float[] _minBound = new float[] { -15, -15, -15, -15, -15, -15 };
    float[] _maxBound = new float[] { 15, 15, 15, 15, 15, 15 };
    IEnumerator BFSSearchGeneratingCore()
    {
        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName, streamDataGenerator);

        _openList = new Queue<float[]>();
        _closedListBFS = new HashSet<float[]>();
        _validCnt = 0;

        // BFS Search
        _para = new float[] { 0, 0, 0, 0, 0, 0 };
        _openList.Enqueue((float[])_para.Clone());

        while (_openList.Count > 0)
        {
            _para = _openList.Dequeue();
            _closedListBFS.Add((float[])_para.Clone());

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
                    if (InBoundary(i, _para[i]) && 
                        !_closedListBFS.Contains(_para) &&
                        !_openList.Contains(_para))
                    {
                        _openList.Enqueue((float[])_para.Clone());
                    }
                    _para[i] -= _stepLength;

                    yield return null;

                    _para[i] -= _stepLength;
                    if (InBoundary(i, _para[i]) && 
                        !_closedListBFS.Contains(_para) &&
                        !_openList.Contains(_para))
                    {
                        _openList.Enqueue((float[])_para.Clone());
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