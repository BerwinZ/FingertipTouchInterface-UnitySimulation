using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Camera cameraToShot = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            SaveImage();
        }
    }

    /// <summary>
    /// Save the image of the view of the cameraToShot to the file
    /// </summary>
    void SaveImage()
    {
        if(cameraToShot != null)
        {
            Texture2D screenShot = CaptureScreen(cameraToShot);

            // Save to the file
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = Application.dataPath + "/Screenshots/ScreenShot.png";
            System.IO.File.WriteAllBytes(filename, bytes);
            // Debug.Log("Saved in " + filename);
        }
    }

    /// <summary>
    /// Capture the screen of the view of given camera
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    Texture2D CaptureScreen(Camera cam)
    {
        // Create a rendering texture
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);

        // Assign the rendering texture to the camera
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        // Create a texture 2D to store the image
        int imageWidth = (int)(Screen.width * cam.rect.width);
        int imageHeight = (int)(Screen.height * cam.rect.height);
        Texture2D screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        // Read pixels from rendering texture (rather than the screen) to texture 2D
        int imagePosX = (int)(Screen.width * cam.rect.x);
        int imagePosY = (int)(Screen.height - (Screen.height * cam.rect.y + imageHeight));
        Rect rect = new Rect(imagePosX, imagePosY, imageWidth, imageHeight);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        // Unassign the rendering texture from the camera
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        return screenShot;
    }
}
