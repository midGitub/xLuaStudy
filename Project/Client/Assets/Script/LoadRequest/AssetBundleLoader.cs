using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class AssetBundleLoader
{
    private static AssetBundleManifest manifest;

    public static Dictionary<string, AssetBundle> abDict = new Dictionary<string, AssetBundle>();
    public static Dictionary<string, GameObject> gameObjectPool = new Dictionary<string, GameObject>();
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

        req.assetBundle.Unload(false);
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
                AssetBundle bundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
                System.Object[] objectList = bundle.LoadAllAssets();
                string[] allAssetNames = bundle.GetAllAssetNames();
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

                abDict.Add(path, bundle);
            }
        };

        UnityWebRequestManager.Instance.DownloadBuffer(path, DownloadCB);
    }

    public static void LoadSpriteAtlas(string path, Action<Dictionary<string, SpriteAtlas>> onLoadFinishCallBack)
    {
        Action<UnityWebRequest> DownloadCB = (request) =>
        {
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError("解压失败  ---- " + request.error + "  " + request.url);
            }
            else
            {
                Dictionary<string, SpriteAtlas> curABLuaDict = new Dictionary<string, SpriteAtlas>();
                AssetBundle Bundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
                System.Object[] objectList = Bundle.LoadAllAssets();
                string[] allAssetNames = Bundle.GetAllAssetNames();

                for (int j = 0; j < objectList.Length; j++)
                {
                    SpriteAtlas sa = objectList[j] as SpriteAtlas;

                    string fileName = allAssetNames[j].Replace("assets/spriteatlas/spriteatlas/", "").Replace(".spriteatlas", "");
                    string[] split = fileName.Split('/');

                    curABLuaDict.Add(split[0], sa);
                }

                if (onLoadFinishCallBack != null)
                {
                    onLoadFinishCallBack.Invoke(curABLuaDict);
                }

                objectList = null;
                curABLuaDict.Clear();
                abDict.Add(path, Bundle);
            }
        };

        UnityWebRequestManager.Instance.DownloadBuffer(path, DownloadCB);
    }

    #endregion

    #region asset

    public static void LoadAsset(string name, AssetType type, Action<UnityEngine.Object> onLoadFinishCallBack)
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
            else if (type == AssetType.TEXTURE)
            {
                if (allBundleList[i].Contains("texture/") && allBundleList[i].Contains(name.ToLower()))
                {
                    bundlePath = allBundleList[i];
                    break;
                }
            }
        }

        CoroutineManager.Instance.StartCoroutine(LoadAssetByCoroutine(bundlePath, name, onLoadFinishCallBack));
    }

    private static IEnumerator LoadAssetByCoroutine(string bundlePath, string name, Action<UnityEngine.Object> onLoadFinishCallBack)
    {
        string assetURL = PathDefine.GetAssetUrl(bundlePath);

        AssetBundle ab = null;

        AssetBundleCreateRequest req = null;
        if (abDict.ContainsKey(bundlePath))
        {
            ab = abDict[bundlePath];
        }
        else
        {
            Debug.LogError("assetURL  " + assetURL);
            req = AssetBundle.LoadFromFileAsync(assetURL);
            ab = req.assetBundle;
            abDict.Add(bundlePath, ab);
        }

        yield return ab;

        if (ab == null)
        {
            Debug.LogError("加载  " + bundlePath + "  失败----");
            onLoadFinishCallBack.Invoke(null);
        }
        else
        {
            UnityEngine.GameObject o = null;
            if (gameObjectPool.ContainsKey(name))
            {
                o = gameObjectPool[name];
            }
            else
            {
                o = ab.LoadAsset<GameObject>(name);
                gameObjectPool.Add(name, o);
            }

            //这里应该继续加载所有依赖项
            string[] dependencies = manifest.GetAllDependencies(bundlePath);
            List<AssetBundle> abList = new List<AssetBundle>();
            if (dependencies.Length > 0)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    int index = i;

                    string depPath = PathDefine.GetAssetUrl(dependencies[i]);
                    var de = AssetBundle.LoadFromFileAsync(depPath);
                    yield return de;
                    abList.Add(de.assetBundle);
                }
                onLoadFinishCallBack.Invoke(o);

                for (int i = 0; i < abList.Count; i++)
                {
                    abList[i].Unload(false);
                }
            }
            else
            {
                onLoadFinishCallBack.Invoke(o);
            }
        }
    }

    #endregion
}