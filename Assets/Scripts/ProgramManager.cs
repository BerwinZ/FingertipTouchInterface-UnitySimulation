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
    GameObject iteratedPanelObj = null;

    [SerializeField]
    GameObject searchPanelObj = null;


    string foldername;
    public string FolderName
    {
        get => foldername;
        set
        {
            foldername = value;
            // Pass to the dataset generator
            if (datasetGenerator != null)
            {
                datasetGenerator.FolderName = value;
            }
            OnFolderNameChange?.Invoke(this, value);
        }
    }

    string csvFileName;
    public string CSVFileName
    {
        get => csvFileName;
        set
        {
            csvFileName = value;
            // Pass to the dataset generator
            if (datasetGenerator != null)
            {
                datasetGenerator.CSVFileName = value;
            }
        }
    }

    // Pass to dataset generator
    IJointMangerAction jointManager;
    ITouchManagerAction touchManger;
    IPanelAction iteratedPanelScript;
    IPanelAction searchPanelScript;
    DatasetGeneratorBase datasetGenerator;

    // Pass to stream data generator
    IFingerAction thumb;
    IFingerAction indexFinger;
    IStreamGeneratorAction streamDataGenerator;

    // Event for outside class
    public event EventHandler<bool> OnDatasetPanelChange;
    public event EventHandler<String> OnFolderNameChange;

    void Start()
    {
        // Since No dataset generator created here (wait when the user select the generator mode), we could set the name without hesitation
        FolderName = "D:/Desktop/UnityData";
        // FolderName = Application.dataPath + "/Screenshots";
        CSVFileName = "data.csv";

        jointManager = JointManager.Instance;
        touchManger = transform.GetComponent<TouchManager>();
        iteratedPanelScript = iteratedPanelObj.GetComponent<IteratedPanel>();
        searchPanelScript = searchPanelObj.GetComponent<SearchPanel>();

        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        indexFinger = ScriptFind.FindTouchDetection(Finger.index);
        streamDataGenerator = new StreamDataGenerator
        (jointManager, thumb, indexFinger, cameraToTakeShot, sameSizeWithWindow);

        TurnOnOffPanel(DatasetType.Iterated, false);
        TurnOnOffPanel(DatasetType.Search, false);

        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return new WaitForSeconds(0.5f);
        OnFolderNameChange?.Invoke(this, FolderName);
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitApplication();
        }
    }


    /// <summary>
    /// For unity editor assign
    /// </summary>
    /// <param name="type"></param>
    /// <param name="flag"></param>
    public void TurnOnPanel(int type)
    {
        TurnOnOffPanel((DatasetType)type, true);
    }

    void TurnOffAllPanel()
    {
        TurnOnOffPanel(DatasetType.Single, false);
        TurnOnOffPanel(DatasetType.Iterated, false);
        TurnOnOffPanel(DatasetType.Search, false);
    }

    /// <summary>
    /// Open the dataset dialog
    /// </summary>
    public void TurnOnOffPanel(DatasetType type, bool flag)
    {
        switch (type)
        {
            case DatasetType.Iterated:
                iteratedPanelObj.SetActive(flag);
                break;
            case DatasetType.Search:
                searchPanelObj.SetActive(flag);
                break;
            default:
                break;
        }
        OnDatasetPanelChange?.Invoke(this, flag);
    }

    /// <summary>
    /// For unity editor assign
    /// </summary>
    /// <param name="type"></param>
    public void ConfigureGenerator(int type)
    {
        ConfigureGenerator((DatasetType)type);
    }

    public void ConfigureGenerator(DatasetType type)
    {
        if (datasetGenerator != null)
        {
            Destroy(datasetGenerator);
            datasetGenerator = null;
        }

        IPanelAction activePanelScript = null;
        switch (type)
        {
            case DatasetType.Single:
                datasetGenerator = gameObject.AddComponent(typeof(SingleDatasetGenerator)) as DatasetGeneratorBase;
                break;
            case DatasetType.Iterated:
                datasetGenerator = gameObject.AddComponent(typeof(IteratedDatasetGenerator)) as DatasetGeneratorBase;
                activePanelScript = iteratedPanelScript;
                break;
            case DatasetType.Search:
                datasetGenerator = gameObject.AddComponent(typeof(SearchDatasetGenerator)) as DatasetGeneratorBase;
                activePanelScript = searchPanelScript;
                break;
            default:
                break;
        }
        datasetGenerator?.Initialize(
                                    streamDataGenerator,
                                    jointManager,
                                    touchManger,
                                    activePanelScript,
                                    FolderName,
                                    CSVFileName);
        StartGeneratingDataset();
    }

    public void StartGeneratingDataset()
    {
        datasetGenerator?.StartGeneratingDataset();
    }

    public void StopOrCancelGeneratingDataset()
    {
        if (datasetGenerator != null)
        {
            datasetGenerator?.StopOrCancelGeneratingDataset();
            Destroy(datasetGenerator);
            datasetGenerator = null;
        }
        TurnOffAllPanel();
    }


    /// <summary>
    /// Exit the application
    /// </summary>
    public void ExitApplication()
    {
        datasetGenerator?.StopOrCancelGeneratingDataset();
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