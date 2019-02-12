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

        if (code == (int)LocalCode.SUCCESS || code == (int)LocalCode.CUR_VER_IS_NEWEST)
        {
            Init();
        }
        else if (code == (int)LocalCode.CUR_SERVERVER_IS_NEWPACKAGE)
        {
            Debug.LogError("下版本是全新整包，需要换包");
        }
    }

    public void Init()
    {
        Action<int> finishCallBack = (code) =>
        {
            if (code == (int)LocalCode.SUCCESS)
            {
                UILoadingView.Instance.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("初始化失败");
            }
        };

        System.Action<System.Action<int>>[] tasks = new System.Action<System.Action<int>>[3];
        tasks[0] = (cb) => { LoaderManager.Instance.Init(cb); };
        tasks[1] = (cb) => { AtlasManager.Instance.Init(cb); };
        tasks[2] = (cb) => { LuaManager.Instance.Init(cb); };
        AsyncHelper asyncHelper = new AsyncHelper();
        asyncHelper.Waterfall(tasks, finishCallBack);
    }

}