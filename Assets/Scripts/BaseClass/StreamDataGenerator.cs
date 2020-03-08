using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class StreamDataGenerator : IStreamGeneratorAction
{

    IJointMangerAction jointManager;
    IFingerAction thumb;
    IFingerAction indexFinger;

    public StreamDataGenerator(IJointMangerAction jointManager, IFingerAction thumb, IFingerAction indexFinger)
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
}
