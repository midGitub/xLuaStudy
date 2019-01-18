
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using XLua;

//Lua中调用该C#脚本  运行前XLua  Generate Code 运行一下
[LuaCallCSharp]
public class LuaHelperManager
{
    protected static LuaHelperManager mInstance;

    protected LuaHelperManager() { }

    public static LuaHelperManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new LuaHelperManager();
            }
            return mInstance;
        }
    }

    /// <summary>
    /// Xlua层加载UI面板
    /// </summary>
    /// <param name="path"> 路径 </param>
    /// <param name="OnCreate"> 创建出来的委托回调 </param>
    public void LoadUI(string path, XLuaCustomExport.OnCreate OnCreate)
    {
        Debug.Log("加载UI窗体 ==" + path);
        GameObject Canvas = GameObject.Find("Canvas");
        path = "Prefab/View/" + path;
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(path)) as GameObject;
        obj.transform.SetParent(Canvas.transform, false);
        if (OnCreate != null)
        {
            obj.AddComponent<LuaViewBehaviour>();
            OnCreate(obj);
        }
    }

    public void LoadLevel(string sceneName)
    {
        Debug.Log("加载场景  " + sceneName);
        string abPath = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/scene/game.unity3d";

        //AssetBundleManager.Instance.loadscene();
        LoaderManager.LoadSceneAsync("Game", DataFrom.PERSISTENTDATAPATH, null);
    }
}



/// <summary>
/// XLua的自定义拓展
/// </summary>
public static class XLuaCustomExport
{
    [CSharpCallLua]
    public delegate void OnCreate(GameObject obj);
}
