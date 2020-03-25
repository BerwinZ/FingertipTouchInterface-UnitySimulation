using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using Common;
using System;


/// <summary>
/// This class involves following functions:
/// 1. Generate single image dataset
///     1.1 Check the validation
///     1.2 Generate stream data and save in disk
/// 2. Generate several image dataset according to the search space
///     2.1 Get parameters settings from panel
///     2.2 Iterates each step, check the validation;
///         If Valid, generate stream data and save in disk
///         If not, pass
///     2.3 Update UI indicator
///     2.4 Provide a interface to stop the process any time
/// </summary>
public class IteratedDatasetGenerator : DatasetGeneratorBase
{
    /// <summary>
    /// Start to generate dataset
    /// </summary>
    public override void StartGeneratingDataset()
    {
        Dictionary<DOF, Dictionary<DataRange, float>> datasetPara = new Dictionary<DOF, Dictionary<DataRange, float>>();

        if (datasetPanel.PackData(out datasetPara))
        {
            StartCoroutine(GenerateDatasetCore(datasetPara));
        }
    }

    /// <summary>
    /// Iterate the parameters and save image and data to disk
    /// </summary>
    IEnumerator GenerateDatasetCore(
        Dictionary<DOF, Dictionary<DataRange, float>> para)
    {
        Debug.Log("Start Generaing...");

        // Prepare the data file
        commonWriter = CreateOrOpenFolderFile(FolderName, CSVFileName, streamDataGenerator);

        // Calculate the total number
        long totalCnt = 1;
        foreach (var joint in para.Keys)
        {
            totalCnt *= (para[joint][DataRange.step] == 0) ?
                1 : Convert.ToInt64((para[joint][DataRange.max] - para[joint][DataRange.min]) / para[joint][DataRange.step]);
        }
        datasetPanel.UpdateTotalSampleCnt(totalCnt);

        // Start iteration
        long currentCnt = 0;
        long validCnt = 0;
        float[] DOFS = new float[6];
        for (float gamma1 = para[DOF.gamma1][DataRange.min];
        gamma1 <= para[DOF.gamma1][DataRange.max];
        gamma1 += Mathf.Max(para[DOF.gamma1][DataRange.step], 1e-8f))
        {
            DOFS[0] = gamma1;

            for (float gamma2 = para[DOF.gamma2][DataRange.min];
            gamma2 <= para[DOF.gamma2][DataRange.max];
            gamma2 += Mathf.Max(para[DOF.gamma2][DataRange.step], 1e-8f))
            {
                DOFS[1] = gamma2;

                for (float gamma3 = para[DOF.gamma3][DataRange.min];
                gamma3 <= para[DOF.gamma3][DataRange.max];
                gamma3 += Mathf.Max(para[DOF.gamma3][DataRange.step], 1e-8f))
                {
                    DOFS[2] = gamma3;

                    for (float alpha1 = para[DOF.alpha1][DataRange.min];
                    alpha1 <= para[DOF.alpha1][DataRange.max];
                    alpha1 += Mathf.Max(para[DOF.alpha1][DataRange.step], 1e-8f))
                    {
                        DOFS[3] = alpha1;

                        for (float alpha2 = para[DOF.alpha2][DataRange.min];
                        alpha2 <= para[DOF.alpha2][DataRange.max];
                        alpha2 += Mathf.Max(para[DOF.alpha2][DataRange.step], 1e-8f))
                        {
                            DOFS[4] = alpha2;

                            for (float beta = para[DOF.beta][DataRange.min];
                            beta <= para[DOF.beta][DataRange.max];
                            beta += Mathf.Max(para[DOF.beta][DataRange.step], 1e-8f))
                            {
                                DOFS[5] = beta;

                                // Check current para, set the joint value
                                Processing = true;
                                jointManager.SetJointsValues(DOFS);

                                // After set the joint, need use yield return here, 
                                // otherwise the joints won't be updated in Unity
                                yield return new WaitWhile(() => Processing);

                                currentCnt++;
                                datasetPanel.UpdateCurrentSampleCnt(currentCnt);
                                yield return null;

                                // If could save, save image here
                                if (IsValid)
                                {
                                    validCnt++;
                                    yield return StartCoroutine(SaveStreamDataToDisk());
                                }
                            }
                        }
                    }
                }
            }
        }

        commonWriter.Flush();
        commonWriter.Close();
        commonWriter = null;

        WinFormTools.MessageBox(IntPtr.Zero, "Valid Image: " + validCnt, "Finish", 0);

        Debug.Log("Finish Generaing!");

    }
}
