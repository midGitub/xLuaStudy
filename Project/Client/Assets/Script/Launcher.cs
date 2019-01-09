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
        if (GameSetting.Instance.patcher)
        {
            PatcherManager.Instance.Check(CheckPatcherEnd);
        }
        else
        {

        }
    }

    /// <summary>
    /// 检查热更完成回调
    /// </summary>
    /// <param name="code"></param>
    private void CheckPatcherEnd(int code)
    {
        Debug.Log("热更流程" + (LocalCode)code);
        switch ((LocalCode)code)
        {
            case LocalCode.SUCCESS:
                break;
            case LocalCode.FAILED:
                break;
            case LocalCode.PATCHER_END:
                break;
            case LocalCode.CurVerIsNewest:
                break;
            case LocalCode.DownloadAllPackageVersionFault:
                break;
            case LocalCode.CurServerVerIsNewPackage:
                break;
            case LocalCode.DownloadVersionJsonFault:
                break;
            case LocalCode.DownloadAssetBundleFileFault:
                break;
            case LocalCode.SACopyToPDCoroutineSuccess:
                break;
            case LocalCode.SACopyToPDCoroutineFault:
                break;
            case LocalCode.CanNotFindVersionInCDN:
                break;
            case LocalCode.DownloadFileVersionJsonFault:
                break;
            default:
                Debug.LogError("热更流程" + (LocalCode)code);
                break;
        }
    }
}