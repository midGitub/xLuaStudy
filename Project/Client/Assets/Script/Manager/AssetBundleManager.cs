using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public delegate void dlg_OnAssetBundleDownLoadOver();
/// <summary>
/// 加载AssetBundle
/// </summary>
public class AssetBundleManager : MonoBehaviour
{
    #region Instance
    private static AssetBundleManager instance;

    public static AssetBundleManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerGroup = GameObject.Find("ManagerGroup");
                if (managerGroup == null)
                {
                    managerGroup = new GameObject();
                    managerGroup.name = "ManagerGroup";
                }

                instance = managerGroup.GetComponentInChildren<AssetBundleManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = managerGroup.transform;
                    go.name = typeof(AssetBundleManager).Name;
                    instance = go.AddComponent<AssetBundleManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    private FileVersionJsonObject _fileVersionJsonObject = null;

    public FileVersionJsonObject fileVersionJsonObject
    {
        get
        {
            if (_fileVersionJsonObject == null)
            {
                string path = PathDefine.presitantABPath(Helper.GetPlatformString()) + "FileVersion/fileversion.json";
                string str = Helper.byteArrayToString(Helper.LoadFileData(path));
                _fileVersionJsonObject = Helper.LoadFileVersionJson(str);
            }
            return _fileVersionJsonObject;
        }
    }

    private UILoadingView uiLoadingView;
    private int downloadIndex = 0;

    public void Awake()
    {
        GameObject canvas = GameObject.Find("Canvas");
        uiLoadingView = canvas.transform.Find("UILoadingView").GetComponent<UILoadingView>();
    }

    public void DownLoadAssetBundleByList(List<VersionAndSize> list, string pfStr, Action<int> cb)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string path = PathDefine.serverPath(pfStr, (int)list[i].version) + list[i].name;

            int index = i;
            Action<UnityWebRequest> DownloadCB = (request) =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError(request.error + "  -----  " + path);
                    cb((int)LocalCode.DownloadBundleFault);
                }
                else
                {
                    downloadIndex++;
                    uiLoadingView.Refresh(downloadIndex, list.Count, "正在下载对应AB文件至本地");

                    if (cb != null && downloadIndex == list.Count)
                    {
                        cb((int)LocalCode.SUCCESS);
                    }
                }
            };

            UnityWebRequestManager.Instance.DownloadFile(path, PathDefine.presitantABPath(pfStr) + "AssetsBundle/" + list[i].name, DownloadCB);
        }
    }
}
