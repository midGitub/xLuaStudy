using System;
using System.Collections;
using UnityEngine;
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
    public static void LoadSceneAsync(string sceneName)
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

        CoroutineManager.Instance.StartCoroutine(LoadSceneByCoroutine(assetBundleName, sceneName, null));
    }

    private static IEnumerator LoadSceneByCoroutine(string bundlePath, string sceneName, Action<int> loadCB)
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

        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (loadCB != null)
        {
            loadCB.Invoke((int)LocalCode.SUCCESS);
        }
    }

    #endregion
}

