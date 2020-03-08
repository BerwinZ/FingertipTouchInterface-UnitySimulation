using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;

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

    IStreamGeneratorAction streamDataGenerator;
    IJointMangerAction jointManager;
    IPanelAction datasetPanelScript;
    IDatasetGeneratorAction datasetGenerator;

    public event DatasetPanelHandler OnDatasetPanelChange;
    public event FolderNameChangeHandler OnFolderNameChange;

    void Start()
    {
        streamDataGenerator = StreamDataGeneratorProxy.Instance;
        jointManager = JointManager.Instance;
        datasetPanelScript = datasetPanel.GetComponent<DatasetPanel>();

        FolderName = "D:/Desktop/UnityData";
        // FolderName = Application.dataPath + "/Screenshots";
        CSVFileName = "data.csv";

        OpenDatasetPanel(false);
        datasetGenerator = gameObject.AddComponent(typeof(DatasetManager)) as IDatasetGeneratorAction;
        Initialize(cameraToTakeShot,
                    sameSizeWithWindow,
                    streamDataGenerator,
                    jointManager,
                    datasetPanelScript,
                    FolderName,
                    CSVFileName);
    }

    public void Initialize(Camera cameraToTakeShot,
                    bool sameSizeWithWindow,
                    IStreamGeneratorAction streamDataGenerator,
                    IJointMangerAction jointManager,
                    IPanelAction datasetPanel,
                    string folderName,
                    string csvFileName)
    {
        datasetGenerator.Initialize(cameraToTakeShot,
                                    sameSizeWithWindow,
                                    streamDataGenerator,
                                    jointManager,
                                    datasetPanelScript,
                                    FolderName,
                                    CSVFileName);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitApplication();
        }

    }
    public void StartGenatingDataset()
    {
        datasetGenerator.StartGenatingDataset();
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