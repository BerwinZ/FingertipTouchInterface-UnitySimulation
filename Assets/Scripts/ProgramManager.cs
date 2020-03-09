using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;

/// <summary>
/// 1. Response to the UI interaction
///     1.1 Change folder
///     1.2 Open/close Panel
///     1.3 Start Generating
///     1.4 Stop Generating
/// 2. Response to the keyboard interaction
/// 3. Control genereator to generate dataset
/// </summary>
public class ProgramManager : Singleton<ProgramManager>, IDatasetGeneratorAction, IGameAction
{
    [Header("Screen Shot Settings")]
    [SerializeField]
    Camera cameraToTakeShot = null;

    [SerializeField]
    bool sameSizeWithWindow = false;

    [Header("Dataset Menu")]
    [SerializeField]
    GameObject datasetPanel = null;

    string foldername;
    public string FolderName
    {
        get => foldername;
        set
        {
            foldername = value;
            if (datasetGenerator != null)
            {
                datasetGenerator.FolderName = value;
            }
            OnFolderNameChange?.Invoke(value);
        }
    }

    string csvFileName;
    public string CSVFileName
    {
        get => csvFileName;
        set
        {
            csvFileName = value;
            if (datasetGenerator != null)
            {
                datasetGenerator.CSVFileName = value;
            }
        }
    }

    IJointMangerAction jointManager;
    IPanelAction datasetPanelScript;
    IFingerAction thumb;
    IFingerAction indexFinger;
    IStreamGeneratorAction streamDataGenerator;
    IDatasetGeneratorAction datasetGenerator;

    public event BooleanEventHandler OnDatasetPanelChange;
    public event StringEventHandler OnFolderNameChange;

    void Start()
    {
        jointManager = JointManager.Instance;
        datasetPanelScript = datasetPanel.GetComponent<DatasetPanel>();
        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        indexFinger = ScriptFind.FindTouchDetection(Finger.index);

        streamDataGenerator = new StreamDataGenerator
        (jointManager, thumb, indexFinger, cameraToTakeShot, sameSizeWithWindow);

        datasetGenerator = gameObject.AddComponent(typeof(DatasetGenerator)) as IDatasetGeneratorAction;
        datasetGenerator.Initialize(streamDataGenerator, jointManager, datasetPanelScript, thumb, FolderName, CSVFileName);

        OpenDatasetPanel(false);
        FolderName = "D:/Desktop/UnityData";
        // FolderName = Application.dataPath + "/Screenshots";
        CSVFileName = "data.csv";
    }

    public void Initialize(
                    IStreamGeneratorAction streamDataGenerator,
                    IJointMangerAction jointManager,
                    IPanelAction datasetPanel,
                    IFingerAction finger,
                    string folderName,
                    string csvFileName)
    {
        datasetGenerator.Initialize(streamDataGenerator, jointManager, datasetPanelScript, finger, folderName, csvFileName);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitApplication();
        }

    }
    public void StartGeneratingDataset()
    {
        datasetGenerator.StartGeneratingDataset();
    }
    public void StopCancelGenerating()
    {
        datasetGenerator.StopCancelGenerating();
        OpenDatasetPanel(false);
    }
    public void SaveSingleImage()
    {
        datasetGenerator.SaveSingleImage();
    }

    public void SearchGeneratingDataset()
    {
        datasetGenerator.SearchGeneratingDataset();
    }

    /// <summary>
    /// Open the dataset dialog
    /// </summary>
    public void OpenDatasetPanel(bool flag)
    {
        datasetPanel.SetActive(flag);
        OnDatasetPanelChange?.Invoke(flag);
    }

    /// <summary>
    /// Exit the application
    /// </summary>
    public void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Change the folder to store images
    /// </summary>
    public void ChangeFolderPath()
    {
        DirDialog dialog = new DirDialog();
        // dialog.pidlRoot = new IntPtr(1941411595568);
        dialog.pszDisplayName = new string(new char[2000]);
        dialog.lpszTitle = "Open Project";
        dialog.ulFlags = 0x00000040 | 0x00000010;

        IntPtr pidlPtr = OpenBroswerDialog.SHBrowseForFolder(dialog);

        char[] charArray = new char[2000];
        for (int i = 0; i < 2000; i++)
            charArray[i] = '\0';

        if (OpenBroswerDialog.SHGetPathFromIDList(pidlPtr, charArray))
        {
            string fullDirPath = new String(charArray);
            fullDirPath = fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));
            if (fullDirPath != "")
            {
                FolderName = fullDirPath;
            }
        }
    }

}