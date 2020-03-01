using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Handle the user's input, including
/// 1. Save single image
/// 2. Exit the application
/// </summary>
public class ScreenShotManager : Singleton<ScreenShotManager>
{
    [Header("Screen Shot Settings")]
    public Camera cameraToTakeShot = null;
    public bool sameSizeWithWindow = false;

    [Header("Dataset Menu")]
    public GameObject datasetMenu;

    Text debugText;
    string foldername;
    public delegate void FolderNameChangeHandler(string str);
    public event FolderNameChangeHandler ChangeFolderName;


    // Start is called before the first frame update
    void Start()
    {
        debugText = GameObject.Find("DebugText").GetComponent<Text>();
        ChangeFolderName += OnChangeFolderName;
        ChangeFolderName("D:/Desktop/UnityData");
        // ChangeFolderName(Application.dataPath + "/Screenshots");
        datasetMenu.SetActive(false);
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
        datasetMenu.SetActive(true);
    }

    /// <summary>
    /// Close the dataset dialog or suspend the generating process
    /// </summary>
    public void CancelSettingsOrGenerating()
    {
        StopAllCoroutines();
        if (commonWriter != null)
        {
            commonWriter.Flush();
            commonWriter.Close();
        }
        DatasetManager.Instance.UpdateCurrentSampleCnt(0);
        // DatasetManager.Instance.UpdateTotalSampleCnt(0);
        datasetMenu.SetActive(false);
    }

    /// <summary>
    /// Start to generate dataset
    /// </summary>
    public void StartGenatingDataset()
    {
        Dictionary<string, Dictionary<string, float>> datasetPara = new Dictionary<string, Dictionary<string, float>>();

        if (DatasetManager.Instance.PackData(out datasetPara))
        {
            StartCoroutine(SaveImages(datasetPara));
        }

    }

    StreamWriter commonWriter;
    /// <summary>
    /// Iterate the parameters and save image and data to disk
    /// </summary>
    IEnumerator SaveImages(Dictionary<string, Dictionary<string, float>> para)
    {
        Debug.Log("Start Generaing...");

        // Check whether the folder exists
        if (!Directory.Exists(foldername))
        {
            Directory.CreateDirectory(foldername);
        }

        // Prepare the data file
        string dataName = foldername + "/data.csv";
        if (File.Exists(dataName))
        {
            commonWriter = new StreamWriter(dataName, true);
        }
        else
        {
            commonWriter = new StreamWriter(dataName);
            commonWriter.WriteLine(JointManager.Instance.GenerateStreamHeader());
        }

        // Calculate the total number
        long totalCnt = 1;
        totalCnt *= (para["gamma1"]["step"] == 0) ? 1 : Convert.ToInt64((para["gamma1"]["max"] - para["gamma1"]["min"]) / para["gamma1"]["step"]);
        totalCnt *= (para["gamma2"]["step"] == 0) ? 1 : Convert.ToInt64((para["gamma2"]["max"] - para["gamma2"]["min"]) / para["gamma2"]["step"]);
        totalCnt *= (para["gamma3"]["step"] == 0) ? 1 : Convert.ToInt64((para["gamma3"]["max"] - para["gamma3"]["min"]) / para["gamma3"]["step"]);
        totalCnt *= (para["alpha1"]["step"] == 0) ? 1 : Convert.ToInt64((para["alpha1"]["max"] - para["alpha1"]["min"]) / para["alpha1"]["step"]);
        totalCnt *= (para["alpha2"]["step"] == 0) ? 1 : Convert.ToInt64((para["alpha2"]["max"] - para["alpha2"]["min"]) / para["alpha2"]["step"]);
        totalCnt *= (para["beta"]["step"] == 0) ? 1 : Convert.ToInt64((para["beta"]["max"] - para["beta"]["min"]) / para["beta"]["step"]);
        DatasetManager.Instance.UpdateTotalSampleCnt(totalCnt);

        // Start iteration
        long currentCnt = 0;
        long validCnt = 0;
        for (float gamma1 = para["gamma1"]["min"];
        gamma1 <= para["gamma1"]["max"];
        gamma1 += Mathf.Max(para["gamma1"]["step"], 1e-8f))
        {
            JointManager.Instance.UpdateParaValue(DOF.gamma1, gamma1);

            for (float gamma2 = para["gamma2"]["min"];
            gamma2 <= para["gamma2"]["max"];
            gamma2 += Mathf.Max(para["gamma2"]["step"], 1e-8f))
            {
                JointManager.Instance.UpdateParaValue(DOF.gamma2, gamma2);

                for (float gamma3 = para["gamma3"]["min"];
                gamma3 <= para["gamma3"]["max"];
                gamma3 += Mathf.Max(para["gamma3"]["step"], 1e-8f))
                {
                    JointManager.Instance.UpdateParaValue(DOF.gamma3, gamma3);

                    for (float alpha1 = para["alpha1"]["min"];
                    alpha1 <= para["alpha1"]["max"];
                    alpha1 += Mathf.Max(para["alpha1"]["step"], 1e-8f))
                    {
                        JointManager.Instance.UpdateParaValue(DOF.alpha1, alpha1);

                        for (float alpha2 = para["alpha2"]["min"];
                        alpha2 <= para["alpha2"]["max"];
                        alpha2 += Mathf.Max(para["alpha2"]["step"], 1e-8f))
                        {
                            JointManager.Instance.UpdateParaValue(DOF.alpha2, alpha2);

                            for (float beta = para["beta"]["min"];
                            beta <= para["beta"]["max"];
                            beta += Mathf.Max(para["beta"]["step"], 1e-8f))
                            {
                                JointManager.Instance.UpdateParaValue(DOF.beta, beta);

                                currentCnt++;
                                DatasetManager.Instance.UpdateCurrentSampleCnt(currentCnt);

                                yield return new WaitForSeconds(0.05f);

                                // If could save, save image here
                                if (JointManager.Instance.thumb.isTouching && !JointManager.Instance.thumb.isOverlapped)
                                {
                                    validCnt++;

                                    // Get a unique image name
                                    string imgName = GenerateImgName();
                                    // Save the image into disk
                                    System.IO.File.WriteAllBytes(
                                            foldername + '/' + imgName,
                                            CaptureScreen(cameraToTakeShot, sameSizeWithWindow));
                                    // Save para data
                                    commonWriter.WriteLine(JointManager.Instance.GenerateStreamData(imgName));
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
        if (JointManager.Instance.thumb.isTouching == false ||
           JointManager.Instance.thumb.isOverlapped == true)
        {
            WinFormTools.MessageBox(IntPtr.Zero, "Finger not touched or overlapped", "Cannot Save Image", 0);
            return;
        }

        // Check whether the folder exists
        if (!Directory.Exists(foldername))
        {
            Directory.CreateDirectory(foldername);
        }

        // Get a unique image name
        string imgName = GenerateImgName();

        // Save the image into disk
        System.IO.File.WriteAllBytes(
                foldername + '/' + imgName,
                CaptureScreen(cameraToTakeShot, sameSizeWithWindow));

        // Save the data into disk
        string dataName = foldername + "/data.csv";
        StreamWriter writer;
        if (File.Exists(dataName))
        {
            writer = new StreamWriter(dataName, true);
        }
        else
        {
            writer = new StreamWriter(dataName);
            writer.WriteLine(JointManager.Instance.GenerateStreamHeader());
        }
        writer.WriteLine(JointManager.Instance.GenerateStreamData(imgName));
        writer.Flush();
        writer.Close();

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
                OnChangeFolderName(fullDirPath);
            }
        }
    }

    void OnChangeFolderName(string str)
    {
        foldername = str;
    }

    /// <summary>
    /// Generate a unique PNG image name with timestamp
    /// </summary>
    /// <returns></returns>
    string GenerateImgName()
    {
        string filename = System.DateTime.Now + "_" + Time.time.ToString("F4");
        filename = filename.Replace('/', '_');
        filename = filename.Replace(' ', '_');
        filename = filename.Replace(':', '_');
        filename = filename.Replace('.', '_');
        filename += ".png";
        return filename;
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
