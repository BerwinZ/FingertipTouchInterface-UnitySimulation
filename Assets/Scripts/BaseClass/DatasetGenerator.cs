using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;


/// <summary>
/// This class involves following functions:
/// 1. Generate single image dataset
///     1.1 Check the validation
///     1.2 Generate stream data and save in disk
/// 2. Generate several image dataset according to the search space
///     2.1 Get parameters settings from panel
///     2.2 Iterates each step, check the validation;
///         If Valid, generate stream data and save in disk
///         If not, pass
///     2.3 Update UI indicator
///     2.4 Provide a interface to stop the process any time
/// </summary>
public class DatasetGenerator : MonoBehaviour, IDatasetGeneratorAction
{
    public string FolderName { get; set; }
    public string CSVFileName { get; set; }
    bool IsValid => finger.IsTouching && !finger.IsOverlapped;
    IFingerAction finger;
    IJointMangerAction jointManager;
    IPanelAction datasetPanel;
    IStreamGeneratorAction streamDataGenerator;

    // Start is called before the first frame update
    public void Initialize(IStreamGeneratorAction streamDataGenerator,
                           IJointMangerAction jointManager,
                           IPanelAction datasetPanel,
                           IFingerAction finger,
                           string folderName,
                           string csvFileName)
    {
        this.streamDataGenerator = streamDataGenerator;
        this.jointManager = jointManager;
        this.datasetPanel = datasetPanel;
        this.finger = finger;
        this.FolderName = folderName;
        this.CSVFileName = csvFileName;
    }

    /// <summary>
    /// Start to generate dataset
    /// </summary>
    public void StartGeneratingDataset()
    {
        Dictionary<DOF, Dictionary<DataRange, float>> datasetPara = new Dictionary<DOF, Dictionary<DataRange, float>>();

        if (datasetPanel.PackData(out datasetPara))
        {
            StartCoroutine(GenerateDatasetCore(datasetPara));
        }
    }

    public void StopCancelGenerating()
    {
        StopAllCoroutines();
        commonWriter?.Flush();
        commonWriter?.Close();
        datasetPanel.UpdateCurrentSampleCnt(0);
    }

    StreamWriter commonWriter;
    /// <summary>
    /// Iterate the parameters and save image and data to disk
    /// </summary>
    IEnumerator GenerateDatasetCore(
        Dictionary<DOF, Dictionary<DataRange, float>> para)
    {
        Debug.Log("Start Generaing...");

        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName);

        // Calculate the total number
        long totalCnt = 1;
        foreach (var joint in para.Keys)
        {
            totalCnt *= (para[joint][DataRange.step] == 0) ?
                1 : Convert.ToInt64((para[joint][DataRange.max] - para[joint][DataRange.min]) / para[joint][DataRange.step]);
        }
        datasetPanel.UpdateTotalSampleCnt(totalCnt);

        // Start iteration
        long currentCnt = 0;
        long validCnt = 0;
        for (float gamma1 = para[DOF.gamma1][DataRange.min];
        gamma1 <= para[DOF.gamma1][DataRange.max];
        gamma1 += Mathf.Max(para[DOF.gamma1][DataRange.step], 1e-8f))
        {
            jointManager.SetJointValue(DOF.gamma1, gamma1);

            for (float gamma2 = para[DOF.gamma2][DataRange.min];
            gamma2 <= para[DOF.gamma2][DataRange.max];
            gamma2 += Mathf.Max(para[DOF.gamma2][DataRange.step], 1e-8f))
            {
                jointManager.SetJointValue(DOF.gamma2, gamma2);

                for (float gamma3 = para[DOF.gamma3][DataRange.min];
                gamma3 <= para[DOF.gamma3][DataRange.max];
                gamma3 += Mathf.Max(para[DOF.gamma3][DataRange.step], 1e-8f))
                {
                    jointManager.SetJointValue(DOF.gamma3, gamma3);

                    for (float alpha1 = para[DOF.alpha1][DataRange.min];
                    alpha1 <= para[DOF.alpha1][DataRange.max];
                    alpha1 += Mathf.Max(para[DOF.alpha1][DataRange.step], 1e-8f))
                    {
                        jointManager.SetJointValue(DOF.alpha1, alpha1);

                        for (float alpha2 = para[DOF.alpha2][DataRange.min];
                        alpha2 <= para[DOF.alpha2][DataRange.max];
                        alpha2 += Mathf.Max(para[DOF.alpha2][DataRange.step], 1e-8f))
                        {
                            jointManager.SetJointValue(DOF.alpha2, alpha2);

                            for (float beta = para[DOF.beta][DataRange.min];
                            beta <= para[DOF.beta][DataRange.max];
                            beta += Mathf.Max(para[DOF.beta][DataRange.step], 1e-8f))
                            {
                                jointManager.SetJointValue(DOF.beta, beta);

                                currentCnt++;
                                datasetPanel.UpdateCurrentSampleCnt(currentCnt);

                                // yield return new WaitForSeconds(0.05f);
                                yield return null;

                                // Get a unique image name and data
                                string data = null;
                                string imgName = null;
                                byte[] image = null;

                                // If could save, save image here
                                if (IsValid)
                                {
                                    validCnt++;

                                    streamDataGenerator.GenerateStreamFileData(out data, out imgName, out image);

                                    // Save para data
                                    commonWriter.WriteLine(data);

                                    // Save the image into disk
                                    System.IO.File.WriteAllBytes(
                                            FolderName + '/' + imgName,
                                            image);
                                }
                            }
                        }
                    }
                }
            }
        }

        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Valid Image: " + validCnt, "Finish", 0);

        Debug.Log("Finish Generaing!");

    }

    /// <summary>
    /// Save a single image and store the para in the .csv file
    /// </summary>
    public void SaveSingleImage()
    {
        // Check whether can save this img currently
        if (!IsValid)
        {
            WinFormTools.MessageBox(IntPtr.Zero, "Finger not touched or overlapped", "Cannot Save Image", 0);
            return;
        }

         // Get a unique image name and data
        string imgName = null;
        string data = null;
        byte[] image = null;

        streamDataGenerator.GenerateStreamFileData(out data, out imgName, out image);
        
        // Save the data into disk
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName);

        // Write the data into csv file
        commonWriter.WriteLine(data);

        // Save the image into disk
        System.IO.File.WriteAllBytes(
                FolderName + '/' + imgName,
                image);

        // Close the writer
        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Saved Image!", "Finish", 0);
        // LoadCSVFile(csvName);
    }

    public void SearchGeneratingDataset()
    {
        Debug.Log("Start Searching...");

        StartCoroutine(SearchGeneratingCore());
    }

    IEnumerator SearchGeneratingCore()
    {
        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName);
        int validCnt = 0;

        // TODO: BFS DFS Search

        WinFormTools.MessageBox(IntPtr.Zero, "Valid Image: " + validCnt, "Finish", 0);

        Debug.Log("Finish Generaing!");
        yield return null;
    }

    StreamWriter CreateOrOpenFolderFile(string folderName, string csvFileName)
    {
        // Check whether the folder exists
        if (!Directory.Exists(folderName))
        {
            Directory.CreateDirectory(folderName);
        }

        string csvName = folderName + "/" + csvFileName;
        StreamWriter writer;
        if (File.Exists(csvName))
        {
            writer = new StreamWriter(csvName, true);
        }
        else
        {
            writer = new StreamWriter(csvName);
            writer.WriteLine(streamDataGenerator.GenerateStreamFileHeader());
        }
        return writer;
    }

    /// <summary>
    /// Load .csv file
    /// </summary>
    /// <param name="fileName"></param>
    void LoadCSVFile(string fileName)
    {
        StreamReader reader = new StreamReader(fileName);

        string[] texts = reader.ReadToEnd().Split("\n"[0]);
        reader.Close();

        foreach (var text in texts)
        {
            Debug.Log(text);
        }
    }
}
