using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
}

