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

    protected IJointMangerAction jointManager;
    protected IPanelAction datasetPanel;
    protected IStreamGeneratorAction streamDataGenerator;
    protected StreamWriter commonWriter;
    protected bool Processing { get; set; }
    protected bool IsValid { get; set; }

    // Start is called before the first frame update
    public virtual void Initialize(IStreamGeneratorAction streamDataGenerator,
                                    IJointMangerAction jointManager,
                                    ITouchManagerAction touchManager,
                                    IPanelAction datasetPanel,
                                    string folderName,
                                    string csvFileName)
    {
        this.streamDataGenerator = streamDataGenerator;
        this.jointManager = jointManager;
        this.datasetPanel = datasetPanel;
        this.FolderName = folderName;
        this.CSVFileName = csvFileName;

        // Subscribe the event of touch manager
        touchManager.OnTouchCalcFinish += (sender, e) =>
        {
            IsValid = e;
            Processing = false;
        };
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

        string csvName = Path.Combine(folderName, csvFileName);
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


    string _data = null;
    string _imgName = null;
    byte[] _image = null;
    public virtual IEnumerator SaveStreamDataToDisk()
    {
        if (commonWriter == null)
            yield break;

        yield return new WaitForEndOfFrame();

        streamDataGenerator.GenerateStreamFileData(
                out _data, out _imgName, out _image);

        // Save para data
        commonWriter.WriteLine(_data);
        commonWriter.Flush();

        // Save the image into disk
        System.IO.File.WriteAllBytes(
                Path.Combine(FolderName, _imgName), _image);
    }
}