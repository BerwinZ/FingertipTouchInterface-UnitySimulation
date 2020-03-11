using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;

public class SingleDatasetGenerator : DatasetGeneratorBase
{
    /// <summary>
    /// Save a single image and store the para in the .csv file
    /// </summary>
    public override void StartGeneratingDataset()
    {
        // Check whether can save this img currently
        if (!IsValid)
        {
            WinFormTools.MessageBox(IntPtr.Zero, "Finger not touched or overlapped", "Cannot Save Image", 0);
            return;
        }

        // Save the data into disk
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName, streamDataGenerator);

        SaveStreamDataToDisk();

        // Close the writer
        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Saved Image!", "Finish", 0);
        // LoadCSVFile(csvName);
    }
}