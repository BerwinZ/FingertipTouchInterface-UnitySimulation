using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class StreamDataGeneratorProxy : Singleton<StreamDataGeneratorProxy>, IStreamGeneratorAction
{
    JointManager jointManager;
    TouchDetection thumb;
    TouchDetection indexFinger;
    IStreamGeneratorAction generator;

    // Start is called before the first frame update
    void Start()
    {
        jointManager = JointManager.Instance;
        thumb = ScriptFind.FindTouchDetection(Finger.thumb);
        indexFinger = ScriptFind.FindTouchDetection(Finger.index);
        generator = new StreamDataGenerator(jointManager, thumb, indexFinger);
    }

    public bool IsValid => thumb.IsTouching && !thumb.IsOverlapped;
 
    public string GenerateStreamFileHeader()
    {
        return generator.GenerateStreamFileHeader();
    }

    public string GenerateStreamFileData(out string imgName)
    {
        return generator.GenerateStreamFileData(out imgName);
    }
}
