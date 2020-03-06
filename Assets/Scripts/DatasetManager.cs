using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;
using System.Runtime.Serialization.Formatters.Binary;


public delegate void FolderNameChangeHandler(string str);
public delegate void DatasetPanelHandler(bool flag);

/// <summary>
/// Handle the user's input, including
/// 1. Save single image
/// 2. Save serveral images according to the settings from dataset panel
/// 3. Change the folder to save images
/// 4. Exit the application
/// </summary>
public class DatasetManager : Singleton<DatasetManager>
{
    [Header("Screen Shot Settings")]
    public Camera cameraToTakeShot = null;
    public bool sameSizeWithWindow = false;

    [Header("Dataset Menu")]
    public GameObject datasetPanel;
    public event DatasetPanelHandler DatasetPanelPublisher;

    public event FolderNameChangeHandler FolderNameChangePublisher;
    string foldername;
    public string FolderName
    {
        get => foldername;
        private set
        {
            foldername = value;
            FolderNameChangePublisher?.Invoke(value);
        }
    }

    StreamDataGeneratorProxy streamDataGenerator;

    Text debugText;

    // Start is called before the first frame update
    void Start()
    {
        debugText = GameObject.Find("DebugText").GetComponent<Text>();
        FolderName = "D:/Desktop/UnityData";
        // FolderName = Application.dataPath + "/Screenshots";
        datasetPanel.SetActive(false);

        streamDataGenerator = StreamDataGeneratorProxy.Instance;
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
    /// Open the dataset dialog
    /// </summary>
    public void OpenDatasetSettings()
    {
        datasetPanel.SetActive(true);
        DatasetPanelPublisher?.Invoke(true);
    }

    /// <summary>
    /// Close the dataset dialog or suspend the generating process
    /// </summary>
    public void CancelSettingsOrGenerating()
    {
        StopAllCoroutines();
        commonWriter?.Flush();
        commonWriter?.Close();
        DatasetPanel.Instance.UpdateCurrentSampleCnt(0);
        // DatasetPanel.Instance.UpdateTotalSampleCnt(0);

        datasetPanel.SetActive(false);
        DatasetPanelPublisher?.Invoke(false);
    }

    /// <summary>
    /// Start to generate dataset
    /// </summary>
    public void StartGenatingDataset()
    {
        Dictionary<DOF, Dictionary<DataRange, float>> datasetPara = new Dictionary<DOF, Dictionary<DataRange, float>>();

        if (DatasetPanel.Instance.PackData(out datasetPara))
        {
            StartCoroutine(GenerateDatasetCore(datasetPara));
        }

    }

    StreamWriter commonWriter;
    /// <summary>
    /// Iterate the parameters and save image and data to disk
    /// </summary>
    IEnumerator GenerateDatasetCore(Dictionary<DOF, Dictionary<DataRange, float>> para)
    {
        Debug.Log("Start Generaing...");

        // Check whether the folder exists
        if (!Directory.Exists(FolderName))
        {
            Directory.CreateDirectory(FolderName);
        }

        // Prepare the data file
        string dataName = FolderName + "/data.csv";
        if (File.Exists(dataName))
        {
            commonWriter = new StreamWriter(dataName, true);
        }
        else
        {
            commonWriter = new StreamWriter(dataName);
            commonWriter.WriteLine(streamDataGenerator.GenerateStreamFileHeader());
        }

        // Calculate the total number
        long totalCnt = 1;
        foreach (var joint in para.Keys)
        {
            totalCnt *= (para[joint][DataRange.step] == 0) ?
                1 : Convert.ToInt64((para[joint][DataRange.max] - para[joint][DataRange.min]) / para[joint][DataRange.step]);
        }
        DatasetPanel.Instance.UpdateTotalSampleCnt(totalCnt);

        // Start iteration
        long currentCnt = 0;
        long validCnt = 0;
        for (float gamma1 = para[DOF.gamma1][DataRange.min];
        gamma1 <= para[DOF.gamma1][DataRange.max];
        gamma1 += Mathf.Max(para[DOF.gamma1][DataRange.step], 1e-8f))
        {
            JointManager.Instance[DOF.gamma1] = gamma1;

            for (float gamma2 = para[DOF.gamma2][DataRange.min];
            gamma2 <= para[DOF.gamma2][DataRange.max];
            gamma2 += Mathf.Max(para[DOF.gamma2][DataRange.step], 1e-8f))
            {
                JointManager.Instance[DOF.gamma2] = gamma2;

                for (float gamma3 = para[DOF.gamma3][DataRange.min];
                gamma3 <= para[DOF.gamma3][DataRange.max];
                gamma3 += Mathf.Max(para[DOF.gamma3][DataRange.step], 1e-8f))
                {
                    JointManager.Instance[DOF.gamma3] = gamma3;

                    for (float alpha1 = para[DOF.alpha1][DataRange.min];
                    alpha1 <= para[DOF.alpha1][DataRange.max];
                    alpha1 += Mathf.Max(para[DOF.alpha1][DataRange.step], 1e-8f))
                    {
                        JointManager.Instance[DOF.alpha1] = alpha1;

                        for (float alpha2 = para[DOF.alpha2][DataRange.min];
                        alpha2 <= para[DOF.alpha2][DataRange.max];
                        alpha2 += Mathf.Max(para[DOF.alpha2][DataRange.step], 1e-8f))
                        {
                            JointManager.Instance[DOF.alpha2] = alpha2;

                            for (float beta = para[DOF.beta][DataRange.min];
                            beta <= para[DOF.beta][DataRange.max];
                            beta += Mathf.Max(para[DOF.beta][DataRange.step], 1e-8f))
                            {
                                JointManager.Instance[DOF.beta] = beta;

                                currentCnt++;
                                DatasetPanel.Instance.UpdateCurrentSampleCnt(currentCnt);

                                // yield return new WaitForSeconds(0.05f);
                                yield return null;

                                // If could save, save image here
                                if (streamDataGenerator.IsValid)
                                {
                                    validCnt++;

                                    // Get a unique image name and data
                                    string imgName = null;
                                    string data = streamDataGenerator.GenerateStreamFileData(out imgName);

                                    // Save the image into disk
                                    System.IO.File.WriteAllBytes(
                                            FolderName + '/' + imgName,
                                            CaptureScreen(cameraToTakeShot, sameSizeWithWindow));
                                    // Save para data
                                    commonWriter.WriteLine(data);
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
        if (!streamDataGenerator.IsValid)
        {
            WinFormTools.MessageBox(IntPtr.Zero, "Finger not touched or overlapped", "Cannot Save Image", 0);
            return;
        }

        // Check whether the folder exists
        if (!Directory.Exists(FolderName))
        {
            Directory.CreateDirectory(FolderName);
        }

        // Save the data into disk
        string dataName = FolderName + "/data.csv";
        StreamWriter writer;
        if (File.Exists(dataName))
        {
            writer = new StreamWriter(dataName, true);
        }
        else
        {
            writer = new StreamWriter(dataName);
            writer.WriteLine(streamDataGenerator.GenerateStreamFileHeader());
        }

        // Get a unique image name and data
        string imgName = null;
        string data = streamDataGenerator.GenerateStreamFileData(out imgName);

        // Save the image into disk
        System.IO.File.WriteAllBytes(
                FolderName + '/' + imgName,
                CaptureScreen(cameraToTakeShot, sameSizeWithWindow));

        writer.WriteLine(data);
        writer.Flush();
        writer.Close();

        WinFormTools.MessageBox(IntPtr.Zero, "Saved Image!", "Finish", 0);
        // LoadCSVFile(dataName);
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

    /// <summary>
    /// Capture the screen of the view of given camera and return the bytes
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    byte[] CaptureScreen(Camera cam, bool isFullSize)
    {
        // Create a rendering texture
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);

        // Assign the rendering texture to the camera
        cam.targetTexture = rt;
        if (isFullSize)
        {
            Rect oldRect = cam.rect;
            cam.rect = new Rect(0, 0, 1, 1);
            cam.Render();
            cam.rect = oldRect;
        }
        else
        {
            cam.Render();
        }
        RenderTexture.active = rt;

        // Create a texture 2D to store the image
        int imageWidth = isFullSize ? Screen.width : (int)(Screen.width * cam.rect.width);
        int imageHeight = isFullSize ? Screen.height : (int)(Screen.height * cam.rect.height);
        Texture2D screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        // Read pixels from rendering texture (rather than the screen) to texture 2D
        // Note. the source coordinate is the image coordinate ((0,0) is in top-right) rather than the pixel coordinate ((0,0) is in the bottom-left)
        int imagePosX = isFullSize ? 0 : (int)(Screen.width * cam.rect.x);
        int imagePosY = isFullSize ? 0 : (int)(Screen.height - (Screen.height * cam.rect.y + imageHeight));
        Rect rect = new Rect(imagePosX, imagePosY, imageWidth, imageHeight);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        // Unassign the rendering texture from the camera
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        return screenShot.EncodeToPNG();
    }

    /// <summary>
    /// Exit the application
    /// </summary>
    void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
