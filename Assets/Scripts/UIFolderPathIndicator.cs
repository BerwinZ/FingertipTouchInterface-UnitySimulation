using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFolderPathIndicator : Singleton<UIFolderPathIndicator>
{
    Text folderIndicator;

    // Start is called before the first frame update
    void Start()
    {
        folderIndicator = transform.GetComponent<Text>();
        DatasetManager.Instance.FolderNameChangePublisher += UpdateText;
        UpdateText(DatasetManager.Instance.FolderName);
    }

    public void UpdateText(string foldername)
    {
        folderIndicator.text = foldername;
    }
}
