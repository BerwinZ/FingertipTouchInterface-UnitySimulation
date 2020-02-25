using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FolderIndicatorActions : MonoBehaviour
{
    Text folderIndicator;

    // Start is called before the first frame update
    void Start()
    {
        folderIndicator = transform.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        folderIndicator.text = ScreenShotManager.Instance.foldername;   
    }
}
