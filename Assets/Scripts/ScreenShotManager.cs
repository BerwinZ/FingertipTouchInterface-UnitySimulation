﻿using System.Collections;
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
/// 2. Exit the application
/// </summary>
public class ScreenShotManager : Singleton<ScreenShotManager>
{
    [Header("Screen Shot Settings")]
    public Camera cameraToTakeShot = null;
    public bool sameSizeWithWindow = false;

    Text debugText;
    public string foldername { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        debugText = GameObject.Find("DebugText").GetComponent<Text>();
        foldername = Application.dataPath + "/Screenshots";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveImage();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitApplication();
        }
    }

    /// <summary>
    /// Save the image of the view of the cameraToTakeShot to the file
    /// </summary>
    void SaveImage()
    {
        // Actions in disk
        string filename = foldername + "/ScreenShot.png";

        // Check whether the folder exists
        if (!Directory.Exists(foldername))
        {
            Directory.CreateDirectory(foldername);
        }


        System.IO.File.WriteAllBytes(filename,
                                     CaptureScreen(cameraToTakeShot, sameSizeWithWindow));

        // Debug.Log("Saved in " + filename);
        // debugText.text = "Saved in " + filename;

    }

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
                foldername = fullDirPath;
            }
        }
    }

    /// <summary>
    /// Capture the screen of the view of given camera
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

    void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}