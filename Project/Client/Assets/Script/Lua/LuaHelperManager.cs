
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

    private GameObject Canvas = GameObject.Find("Canvas");

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
        for (int k = 0; k < 10000; k++)
        {
            int kk = k;
            Action<UnityEngine.Object> onLoadFinishCallBack = (p) =>
            {
                GameObject obj = GameObject.Instantiate(p) as GameObject;
                obj.transform.SetParent(Canvas.transform, false);
                obj.name = kk.ToString();
                obj.SetActive(false);
                if (OnCreate != null)
                {
                    obj.AddComponent<LuaViewBehaviour>();
                    OnCreate(obj);
                }
            };

            LoaderManager.LoadUISync(path, onLoadFinishCallBack);
        }

        int a = 0;
    }

    public void LoadLevel(string sceneName)
    {
        Debug.Log("加载场景  " + sceneName);
        LoaderManager.LoadSceneAsync("Game", null);
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
