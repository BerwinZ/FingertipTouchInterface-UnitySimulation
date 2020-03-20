using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

/// <summary>
/// This class is used to generate 
/// 1. Head line of .csv data
/// 2. Line of .csv data
/// 3. Image byte
/// </summary>
public class StreamDataGenerator : IStreamGeneratorAction
{

    IJointMangerAction jointManager;
    IFingerAction thumb;
    IFingerAction indexFinger;
    Camera cam;
    bool isFullSize;


    public StreamDataGenerator(
                IJointMangerAction jointManager, 
                IFingerAction thumb, 
                IFingerAction indexFinger,
                Camera cam,
                bool isFullSize)
    {
        this.jointManager = jointManager;
        this.thumb = thumb;
        this.indexFinger = indexFinger;
        this.cam = cam;
        this.isFullSize = isFullSize;
        ConfigureRenderSettings();
    }

    RenderTexture _rt;
    Rect _oldRect, _tmpRect, _imgSaveRect;
    Texture2D _screenShot;
    void ConfigureRenderSettings()
    {
        // Create a rendering texture
        _rt = new RenderTexture(Screen.width, Screen.height, 0);

        // Store the previous rect settings
        _oldRect = cam.rect;
        _tmpRect = new Rect(0, 0, 1, 1);

        // Set the size of texture 2D
        int imageWidth = isFullSize ? Screen.width : (int)(Screen.width * cam.rect.width);
        int imageHeight = isFullSize ? Screen.height : (int)(Screen.height * cam.rect.height);

        // Create a texture 2D to store the image
        _screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        // Create the rect for read pixel
        int imagePosX = isFullSize ? 0 : (int)(Screen.width * cam.rect.x);
        int imagePosY = isFullSize ? 0 : (int)(Screen.height - (Screen.height * cam.rect.y + imageHeight));
        _imgSaveRect = new Rect(imagePosX, imagePosY, imageWidth, imageHeight);
    }

    public string GenerateStreamFileHeader()
    {
        string header =
                        "Gamma1," +
                        "Gamma2," +
                        "Gamma3," +
                        "Alpha1," +
                        "Alpha2," +
                        "Beta," +
                        "thumb_x," +
                        "thumb_y," +
                        "index_x," +
                        "index_y," +
                        "ImgName";
        return header;
    }

    public void GenerateStreamFileData(
            out string csvData, out string imgName, out byte[] image)
    {
        imgName = GenerateImageName();
        image = CaptureScreen();
        csvData =
               jointManager.GetJointValue(DOF.gamma1).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.gamma2).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.gamma3).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.alpha1).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.alpha2).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.beta).ToString("F2") + "," +
               thumb.TouchPosition.x.ToString("F0") + "," +
               thumb.TouchPosition.y.ToString("F0") + "," +
               indexFinger.TouchPosition.x.ToString("F0") + "," +
               indexFinger.TouchPosition.y.ToString("F0") + "," +
               imgName;
    }

    /// <summary>
    /// Generate a unique PNG image name with timestamp
    /// </summary>
    /// <returns></returns>
    string GenerateImageName()
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
    byte[] CaptureScreen()
    {
        // Assign the rendering texture to the camera
        cam.targetTexture = _rt;
        if (isFullSize)
        {
            cam.rect = _tmpRect;
            cam.Render();
            cam.rect = _oldRect;
        }
        else
        {
            cam.Render();
        }
        RenderTexture.active = _rt;

        // Read pixels from rendering texture (rather than the screen) to texture 2D
        // Note. the source coordinate is the image coordinate ((0,0) is in top-right) rather than the pixel coordinate ((0,0) is in the bottom-left)
        _screenShot.ReadPixels(_imgSaveRect, 0, 0);
        _screenShot.Apply();

        // Unassign the rendering texture from the camera
        cam.targetTexture = null;
        RenderTexture.active = null;

        return _screenShot.EncodeToPNG();
    }
}
