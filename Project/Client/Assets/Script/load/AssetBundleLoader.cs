using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AssetBundleLoader
{
    private static AssetBundleManifest manifest;

    private static void CheckInit()
    {
        if (manifest != null)
        {
            return;
        }

        string mainBundleUrl = PathDefine.GetAssetUrl("AssetsBundle");
        AssetBundle mainBundle = AssetBundle.LoadFromFile(mainBundleUrl, 0);
        if (mainBundle == null)
        {
            Debug.LogError("找不到AssetsBundle  " + mainBundleUrl);
        }

        manifest = mainBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        mainBundle.Unload(false);
    }

    #region scene
    public static void LoadSceneAsync(string sceneName, Action<int> onLoadFinishCallBack)
    {
        CheckInit();

        string assetBundleName = string.Empty;

        string[] allBundlePath = manifest.GetAllAssetBundles();

        for (int i = 0; i < allBundlePath.Length; i++)
        {
            if (allBundlePath[i].Contains(sceneName.ToLower()))
            {
                assetBundleName = allBundlePath[i];
                break;
            }
        }

        CoroutineManager.Instance.StartCoroutine(LoadSceneByCoroutine(assetBundleName, sceneName, onLoadFinishCallBack));
    }

    private static IEnumerator LoadSceneByCoroutine(string bundlePath, string sceneName, Action<int> onLoadFinishCallBack)
    {
        string[] dependencies = manifest.GetAllDependencies(bundlePath);

        string assetURL = PathDefine.GetAssetUrl(bundlePath);
        var req = AssetBundle.LoadFromFileAsync(assetURL);
        yield return req;

        if (req == null)
        {
            Debug.LogError("加载场景Bundle失败  " + assetURL);
        }
        else
        {
            AssetBundle ab = req.assetBundle;
        }

        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return ao;
        if (ao.isDone && onLoadFinishCallBack != null)
        {
            onLoadFinishCallBack.Invoke((int)LocalCode.SUCCESS);
        }
    }

    #endregion

    #region lua

    /// <summary>
    /// bundle中加载所有Lua文件
    /// </summary>
    /// <param name="onLoadSingleFinishCallBack"></param>
    /// <param name="onLoadAllFinishCallBack"></param>
    public static void LoadLuaInBundle(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        Dictionary<string, byte[]> allLuaDict = new Dictionary<string, byte[]>();
        FileVersionJsonObject fileversion = AssetBundleManager.Instance.fileVersionJsonObject;
        List<VersionAndSize> vasList = fileversion.versionSizeList.FindAll(t => t.name.Contains("lua/"));

        int dCount = 0;
        for (int i = 0; i < vasList.Count; i++)
        {
            string path = string.Empty;
            if (vasList[i].version > GameSetting.Instance.versionCode)
            {
                path = PathDefine.presitantABPath() + vasList[i].name;
            }
            else
            {
                path = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" + vasList[i].name;
            }

            Action<UnityWebRequest> DownloadCB = (request) =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError("解压失败  ---- " + request.error + "  " + request.url);
                }
                else
                {
                    AssetBundle Bundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
                    System.Object[] objectList = Bundle.LoadAllAssets();
                    string[] allAssetNames = Bundle.GetAllAssetNames();
                    for (int j = 0; j < objectList.Length; j++)
                    {
                        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(objectList[j].ToString());

                        string fileName = allAssetNames[j].Replace("assets/luabyte/", "").Replace(".lua.bytes", "");
                        string[] split = fileName.Split('/');

                        allLuaDict.Add(split[1], byteArray);
                    }
                }

                dCount++;
                if (onLoadSingleFinishCallBack != null)
                {
                    onLoadSingleFinishCallBack.Invoke(dCount, vasList.Count);
                }

                if (dCount == vasList.Count)
                {
                    Debug.Log("加载LUA完成");
                    if (onLoadAllFinishCallBack != null)
                    {
                        onLoadAllFinishCallBack.Invoke(allLuaDict);
                    }
                }
            };
            UnityWebRequestManager.Instance.DownloadBuffer(path, DownloadCB);
        }
    }

    /// <summary>
    /// 资源中加载所有Lua文件
    /// </summary>
    public static void LoadLuaInResources(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        Dictionary<string, byte[]> allLuaDict = new Dictionary<string, byte[]>();
        string[] luaPath = Helper.GetFiles("Assets/Lua/", null, true, true);
        List<string> shouldLoadFileList = new List<string>();
        for (int i = 0; i < luaPath.Length; i++)
        {
            if (!luaPath[i].Contains(".meta"))
            {
                shouldLoadFileList.Add(luaPath[i]);
            }
        }

        for (int i = 0; i < shouldLoadFileList.Count; i++)
        {
            if (onLoadSingleFinishCallBack != null)
            {
                onLoadSingleFinishCallBack.Invoke(i, shouldLoadFileList.Count);
            }
            byte[] byteArray = Helper.LoadFileData(shouldLoadFileList[i]);
            string[] split = shouldLoadFileList[i].Replace('\\', '/').Split('/');
            string name = split[split.Length - 1].Replace(".lua", "").ToLower();
            allLuaDict.Add(name, byteArray);
        }

        if (onLoadAllFinishCallBack != null)
        {
            onLoadAllFinishCallBack.Invoke(allLuaDict);
        }
        Debug.Log("加载LUA完成");
    }

    #endregion
}

