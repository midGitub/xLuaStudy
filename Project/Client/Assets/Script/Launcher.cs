using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    PatcherManager patcherManager;

    private void Start()
    {
        PatcherManager.Instance.Check(CheckPatcherEnd);
    }

    private void CheckPatcherEnd(int code)
    {
        if (code == (int)LocalCode.SUCCESS)
        {
            Debug.Log("热更流程" + (LocalCode)code);
        }
        else
        {
            Debug.LogError("热更流程" + (LocalCode)code);
        }
    }
}