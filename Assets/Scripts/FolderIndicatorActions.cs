using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FolderIndicatorActions : Singleton<FolderIndicatorActions>
{
    Text folderIndicator;

    // Start is called before the first frame update
    void Start()
    {
        folderIndicator = transform.GetComponent<Text>();
        ScreenShotManager.Instance.FolderNameChangePublisher += UpdateText;
        UpdateText(ScreenShotManager.Instance.FolderName);
    }

    public void UpdateText(string foldername)
    {
        folderIndicator.text = foldername;
    }
}
