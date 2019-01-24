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
        if (GameSetting.Instance.runType == RunType.PATCHER_SA_PS)
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
        else if (code == (int)LocalCode.CurServerVerIsNewPackage)
        {
            Debug.LogError("下版本是全新整包，需要换包");
        }
    }

    public void Init()
    {
        float a = float.MaxValue;
        float b = float.MaxValue + 900.0f;
        Debug.LogError(a);
        Debug.LogError(b);
        Debug.LogError(a + b);
        Debug.LogError(b - a);
        Debug.LogError(a + b > a);
        int c = 0;
        Debug.LogError((int)(c + float.MaxValue));

        int d = 43534;
        Debug.LogError((int)(d + float.MaxValue));

        LoaderManager.Instance.Init();
        LuaManager.Instance.Init();
    }
}