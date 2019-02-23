using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class ResourcesLoader
{
    #region

    public static void LoadScene(string sceneName, Action<int> onLoadFinishCallback)
    {
        CoroutineManager.Instance.StartCoroutine(LoadSceneByCoroutine(sceneName, onLoadFinishCallback));
    }

    private static IEnumerator LoadSceneByCoroutine(string sceneName, Action<int> onLoadFinishCallback)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return ao;
        if (ao.isDone && onLoadFinishCallback != null)
        {
            onLoadFinishCallback.Invoke((int)LocalCode.SUCCESS);
        }
        else
        {
            onLoadFinishCallback.Invoke((int)LocalCode.LOAD_SCENE_ERROR);
        }
    }

    #endregion
    /// <summary>
    /// 加载Lua文件
    /// </summary>
    public static void LoadLuaData(string path, Action<Dictionary<string, byte[]>> onLoadFinishCallBack)
    {
        Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>();
        byte[] byteArray = Helper.LoadFileData(path);
        string[] split = path.Replace('\\', '/').Split('/');
        string name = split[split.Length - 1].Replace(".lua", "").ToLower();
        dict.Add(name, byteArray);
        if (onLoadFinishCallBack != null)
        {
            onLoadFinishCallBack.Invoke(dict);
        }
    }

    public static void LoadSpriteAtlas(string path, Action<Dictionary<string, SpriteAtlas>> onLoadFinishCallBack)
    {
        Dictionary<string, SpriteAtlas> dict = new Dictionary<string, SpriteAtlas>();

        Debug.LogError(path);
        SpriteAtlas sa = null;
#if UNITY_EDITOR
        sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
#endif
        string[] split = path.Replace('\\', '/').Split('/');
        string name = split[split.Length - 1].Replace(".spriteatlas", "").ToLower();
        dict.Add(name, sa);
        if (onLoadFinishCallBack != null)
        {
            onLoadFinishCallBack.Invoke(dict);
        }
    }

    public static void LoadAsset(string name, AssetType type, Action<UnityEngine.Object> onLoadFinishCallBack)
    {
        string path = string.Empty;
        if (type == AssetType.UIPREFAB)
        {
            path = "Prefab/View/" + name;
        }

        CoroutineManager.Instance.StartCoroutine(LoadAssetByCoroutine(path, onLoadFinishCallBack));
    }

    private static IEnumerator LoadAssetByCoroutine(string path, Action<UnityEngine.Object> onLoadFinishCallBack)
    {
        ResourceRequest rr = Resources.LoadAsync(path);
        yield return rr;
        if (rr.isDone)
        {
            onLoadFinishCallBack.Invoke(rr.asset);
        }
    }
}

