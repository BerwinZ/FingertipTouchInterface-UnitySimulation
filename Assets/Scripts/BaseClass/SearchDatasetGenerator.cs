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

        StartCoroutine(SearchGeneratingCore());
    }

    IEnumerator SearchGeneratingCore()
    {
        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName, streamDataGenerator);
        int validCnt = 0;

        // TODO: BFS DFS Search

        // Close the writer
        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Valid Image: " + validCnt, "Finish", 0);

        Debug.Log("Finish Generaing!");
        yield return null;
    }
}