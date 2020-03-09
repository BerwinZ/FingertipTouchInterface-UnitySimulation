using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;

public abstract class DatasetGeneratorBase : MonoBehaviour, IDatasetGeneratorAction
{
    public string FolderName { get; set; }
    public string CSVFileName { get; set; }

    protected bool IsValid => finger.IsTouching && !finger.IsOverlapped;
    protected IFingerAction finger;
    protected IJointMangerAction jointManager;
    protected IPanelAction datasetPanel;
    protected IStreamGeneratorAction streamDataGenerator;
    protected StreamWriter commonWriter;

    // Start is called before the first frame update
    public virtual void Initialize(IStreamGeneratorAction streamDataGenerator,
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

    public abstract void StartGeneratingDataset();

    public virtual void StopOrCancelGeneratingDataset()
    {
        StopAllCoroutines();
        commonWriter?.Flush();
        commonWriter?.Close();
        datasetPanel.UpdateCurrentSampleCnt(0);
    }

    public virtual StreamWriter CreateOrOpenFolderFile(string folderName, string csvFileName, IStreamGeneratorAction streamGenerator)
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
            writer.WriteLine(streamGenerator.GenerateStreamFileHeader());
        }
        return writer;
    }

    /// <summary>
    /// Load .csv file
    /// </summary>
    /// <param name="fileName"></param>
    public virtual void LoadCSVFile(string fileName)
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