using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

/// <summary>
/// This class is used to generate line of .csv data
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

    public bool GenerateStreamFileData(out string data, out string imgName)
    {
        imgName = GenerateImageName();
        data =
               jointManager.GetJointValue(DOF.gamma1).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.gamma2).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.gamma3).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.alpha1).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.alpha2).ToString("F2") + "," +
               jointManager.GetJointValue(DOF.beta).ToString("F2") + "," +
               thumb.TouchPosition.x.ToString("F2") + "," +
               thumb.TouchPosition.y.ToString("F2") + "," +
               indexFinger.TouchPosition.x.ToString("F2") + "," +
               indexFinger.TouchPosition.y.ToString("F2") + "," +
               imgName;
        return true;
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
        UnityEngine.Object.Destroy(rt);

        return screenShot.EncodeToPNG();
    }
}
