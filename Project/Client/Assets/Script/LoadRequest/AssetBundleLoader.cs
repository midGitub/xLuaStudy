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
    /// 资源中加载所有Lua文件
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

    public static void LoadAsset(AssetType type, Action<int, object> onLoadFinishCallBack)
    {
        CheckInit();
        //if (GameSetting.Instance.runType == )
    }

    #endregion
}

