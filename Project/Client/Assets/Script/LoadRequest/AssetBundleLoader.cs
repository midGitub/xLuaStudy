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
            Debug.LogError(LocalCode.DOWNLOAD_ASSETBUNDLEFILE_FAULT);
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
        //todo 这里应该继续加载所有依赖项
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
        else
        {
            onLoadFinishCallBack.Invoke((int)LocalCode.LOAD_SCENE_ERROR);
        }
    }

    #endregion

    #region lua

    /// <summary>
    /// 加载Lua文件
    /// </summary>
    public static void LoadLuaData(string path, Action<Dictionary<string, byte[]>> onLoadFinishCallBack)
    {
        Action<UnityWebRequest> DownloadCB = (request) =>
        {
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError("解压失败  ---- " + request.error + "  " + request.url);
            }
            else
            {
                Dictionary<string, byte[]> curABLuaDict = new Dictionary<string, byte[]>();
                AssetBundle Bundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
                System.Object[] objectList = Bundle.LoadAllAssets();
                string[] allAssetNames = Bundle.GetAllAssetNames();
                for (int j = 0; j < objectList.Length; j++)
                {
                    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(objectList[j].ToString());

                    string fileName = allAssetNames[j].Replace("assets/luabyte/", "").Replace(".lua.bytes", "");
                    string[] split = fileName.Split('/');

                    curABLuaDict.Add(split[1], byteArray);
                }

                if (onLoadFinishCallBack != null)
                {
                    onLoadFinishCallBack.Invoke(curABLuaDict);
                }
            }
        };

        UnityWebRequestManager.Instance.DownloadBuffer(path, DownloadCB);
    }

    #endregion

    #region asset

    public static void LoadAsset(string name, AssetType type, Action<int, UnityEngine.Object> onLoadFinishCallBack)
    {
        CheckInit();

        //这里需要通过AssetType 去区别加载数据的名字
        string bundlePath = string.Empty;

        string[] allBundleList = manifest.GetAllAssetBundles();

        for (int i = 0; i < allBundleList.Length; i++)
        {
            if (type == AssetType.UIPREFAB)
            {
                if (allBundleList[i].Contains("view/") && allBundleList[i].Contains(name.ToLower()))
                {
                    bundlePath = allBundleList[i];
                    break;
                }
            }
        }

        CoroutineManager.Instance.StartCoroutine(LoadAssetByCoroutine(bundlePath, name, onLoadFinishCallBack));
    }

    private static IEnumerator LoadAssetByCoroutine(string bundlePath, string name, Action<int, UnityEngine.Object> onLoadFinishCallBack)
    {
        //todo 这里应该继续加载所有依赖项
        string[] dependencies = manifest.GetAllDependencies(bundlePath);

        string assetURL = PathDefine.GetAssetUrl(bundlePath);
        var req = AssetBundle.LoadFromFileAsync(assetURL);
        yield return req;

        if (req == null)
        {
            Debug.LogError("加载  " + bundlePath + "  失败----");
            onLoadFinishCallBack.Invoke((int)LocalCode.LOAD_SCENE_ERROR, null);
        }
        else
        {
            AssetBundle ab = req.assetBundle;
            UnityEngine.GameObject o = ab.LoadAsset<GameObject>(name);
            ab.Unload(false);
            onLoadFinishCallBack.Invoke((int)LocalCode.SUCCESS, o);
        }
    }

    #endregion
}