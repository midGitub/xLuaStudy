using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    PatcherManager patcherManager;

    private void Start()
    {
        if (GameSetting.Instance.patcher)
        {
            PatcherManager.Instance.Check(CheckPatcherEnd);
        }
        else
        {
            Init();
        }
    }

    /// <summary>
    /// 检查热更完成回调
    /// </summary>
    /// <param name="code"></param>
    private void CheckPatcherEnd(int code)
    {
        Debug.Log("热更流程" + (LocalCode)code);

        if (code == (int)LocalCode.SUCCESS || code == (int)LocalCode.CurVerIsNewest)
        {
            Init();
        }
    }

    public void Init()
    {
        LuaManager.Instance.Init();
    }
}