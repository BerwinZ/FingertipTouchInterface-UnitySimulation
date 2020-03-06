using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class StreamDataGenerator : IStreamGeneratorAction
{

    JointManager jointManager;
    TouchDetection thumb;
    TouchDetection indexFinger;

    public StreamDataGenerator(JointManager jointManager, TouchDetection thumb, TouchDetection indexFinger)
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

    public string GenerateStreamFileData(out string imgName)
    {
        imgName = GenerateImageName();
        string data =
               jointManager.Gamma1.ToString("F2") + "," +
               jointManager.Gamma2.ToString("F2") + "," +
               jointManager.Gamma3.ToString("F2") + "," +
               jointManager.Alpha1.ToString("F2") + "," +
               jointManager.Alpha2.ToString("F2") + "," +
               jointManager.Beta.ToString("F2") + "," +
               thumb.TouchPosition.x.ToString("F2") + "," +
               thumb.TouchPosition.y.ToString("F2") + "," +
               indexFinger.TouchPosition.x.ToString("F2") + "," +
               indexFinger.TouchPosition.y.ToString("F2") + "," +
               imgName;
        return data;

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
