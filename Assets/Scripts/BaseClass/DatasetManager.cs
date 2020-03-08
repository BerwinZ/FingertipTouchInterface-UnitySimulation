using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;


/// <summary>
/// Handle the user's input, including
/// 1. Save single image
/// 2. Save serveral images according to the settings from dataset panel
/// </summary>
public class DatasetManager : MonoBehaviour, IDatasetGeneratorAction
{
    Camera cameraToTakeShot;
    bool sameSizeWithWindow;
    public string FolderName { get; set; }
    public string CSVFileName { get; set; }
    IStreamGeneratorAction streamDataGenerator;
    IJointMangerAction jointManager;
    IPanelAction datasetPanel;
    
    // Start is called before the first frame update
    public void Initialize(Camera cameraToTakeShot,
                          bool sameSizeWithWindow,
                          IStreamGeneratorAction streamDataGenerator,
                          IJointMangerAction jointManager,
                          IPanelAction datasetPanel,
                          string folderName,
                          string csvFileName)
    {
        this.cameraToTakeShot = cameraToTakeShot;
        this.sameSizeWithWindow = sameSizeWithWindow;
        this.streamDataGenerator = streamDataGenerator;
        this.jointManager = jointManager;
        this.datasetPanel = datasetPanel;
        this.FolderName = folderName;
        this.CSVFileName = csvFileName;
    }

    /// <summary>
    /// Start to generate dataset
    /// </summary>
    public void StartGenatingDataset()
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

                                // If could save, save image here
                                if (streamDataGenerator.GenerateStreamFileData(out data, out imgName))
                                {
                                    validCnt++;

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
        // Get a unique image name and data
        string imgName = null;
        string data = null;

        // Check whether can save this img currently
        if (!streamDataGenerator.GenerateStreamFileData(out data, out imgName))
        {
            WinFormTools.MessageBox(IntPtr.Zero, "Finger not touched or overlapped", "Cannot Save Image", 0);
            return;
        }

        // Save the data into disk
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName);

        // Save the image into disk
        System.IO.File.WriteAllBytes(
                FolderName + '/' + imgName,
                CaptureScreen(cameraToTakeShot, sameSizeWithWindow));

        // Write the data into csv file
        commonWriter.WriteLine(data);

        // Close the writer
        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Saved Image!", "Finish", 0);
        // LoadCSVFile(csvName);
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


}
