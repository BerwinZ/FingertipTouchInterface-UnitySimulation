using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

/// <summary>
/// Get path name from the GameManager
/// </summary>
public class UIFolderPathIndicator : MonoBehaviour
{
    Text folderIndicator;
    IGameAction gameManager;

    // Start is called before the first frame update
    void Start()
    {
        folderIndicator = transform.GetComponent<Text>();
        gameManager = ProgramManager.Instance;
        gameManager.OnFolderNameChange += UpdateText;
    }

    public void UpdateText(string foldername)
    {
        folderIndicator.text = foldername;
    }
}
