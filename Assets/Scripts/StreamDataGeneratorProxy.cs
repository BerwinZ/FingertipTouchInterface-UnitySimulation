using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class StreamDataGeneratorProxy : Singleton<StreamDataGeneratorProxy>, IStreamGeneratorAction
{
    IJointMangerAction jointManager;
    IFingerAction thumb;
    IFingerAction indexFinger;
    IStreamGeneratorAction generator;

    // Start is called before the first frame update
    void Start()
    {
        jointManager = JointManager.Instance;
        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        indexFinger = ScriptFind.FindTouchDetection(Finger.index);
        generator = new StreamDataGenerator(jointManager, thumb, indexFinger);
    }

    bool IsValid => thumb.IsTouching && !thumb.IsOverlapped;

    public string GenerateStreamFileHeader()
    {
        return generator.GenerateStreamFileHeader();
    }

    public bool GenerateStreamFileData(out string data, out string imgName)
    {
        if (IsValid)
        {
            return generator.GenerateStreamFileData(out data, out imgName);
        }
        else
        {
            data = null;
            imgName = null;
            return false;
        }
    }
}
