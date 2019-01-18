using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
                GameObject managerGroup = Helper.GetManagerGroup();

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
                string path = PathDefine.presitantABPath() + "FileVersion/fileversion.json";
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
                    cb((int)LocalCode.DOWNLOAD_BUNDLE_FAULT);
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

            UnityWebRequestManager.Instance.DownloadFile(path, PathDefine.presitantABPath() + "AssetsBundle/" + list[i].name, DownloadCB);
        }
    }

    public void loadscene()
    {
        StartCoroutine(load());
    }

    IEnumerator load()
    {
        string abPath = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/scene/game.unity3d";
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(abPath,0);
        yield return abcr.assetBundle;
        AssetBundle ab = abcr.assetBundle;


        AsyncOperation ao = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
    }
}
