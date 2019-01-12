using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

[CSharpCallLua]
public class LuaManager : MonoBehaviour
{
    private static LuaManager instance;

    public static LuaManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerGroup = GameObject.Find("ManagerGroup");
                if (managerGroup == null)
                {
                    managerGroup = new GameObject();
                    managerGroup.name = "ManagerGroup";
                }

                instance = managerGroup.GetComponentInChildren<LuaManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = managerGroup.transform;
                    go.name = typeof(LuaManager).Name;
                    instance = go.AddComponent<LuaManager>();
                }
            }
            return instance;
        }
    }

    public static LuaEnv luaEnv;

    /// <summary>
    /// lua文件保存地址
    /// </summary>
    private string luaFilePath;
    protected void Awake()
    {
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(LoadLuaFile);
#if UNITY_EDITOR
        if (GameSetting.Instance.patcher == true)//判断是否走热更AB逻辑
        {
            luaFilePath = PathDefine.presitantABPath(Helper.GetPlatformString()) + "LuaFile";
            Debug.LogError(luaFilePath);
        }
        else
        {
            luaFilePath = "Assets/Lua/";
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
              luaFilePath = PathDefine.presitantABPath(Helper.GetPlatformString()) + "LuaFile";
#endif
    }

    public void Init()
    {
        //这里有可优化的地方 可以只解压那些修改过的LUAAssetBundle 暂不修改
        DecompressionLuaAssetBundle();
    }

    private byte[] LoadLuaFile(ref string filePath)
    {
        string name = filePath.ToLower();
        byte[] byteArray = null;

        string[] allLuaPaths = Helper.GetFiles(luaFilePath, null, true);

        for (int i = 0; i < allLuaPaths.Length; i++)
        {
            allLuaPaths[i] = allLuaPaths[i].ToLower();
            if (!allLuaPaths[i].Contains("meta"))
            {
                if (allLuaPaths[i].Contains(name))
                {
                    byteArray = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(allLuaPaths[i]));
                    break;
                }
            }
        }

        return byteArray;
    }

    /// <summary>
    /// 解压在路径里面的Lua AssetBundle
    /// </summary>
    public void DecompressionLuaAssetBundle()
    {
        StartCoroutine(LoadLuaAssetBundle(LoadLuaAssetBundleEnd));
    }

    private void LoadLuaAssetBundleEnd()
    {
        luaEnv.DoString("require('Main')");
        LuaFunction mainFunction = luaEnv.Global.Get<LuaFunction>("main");
        mainFunction.Call();
    }

    /// <summary>
    /// 加载lua脚本
    /// </summary>
    /// <param name="cb"></param>
    /// <returns></returns>
    public IEnumerator LoadLuaAssetBundle(System.Action cb)
    {
        string luaABPath = PathDefine.presitantABPath(Helper.GetPlatformString()) + "AssetsBundle/AssetsBundle";

        AssetBundle ab = AssetBundle.LoadFromFile(luaABPath);
        AssetBundleManifest manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] allPath = manifest.GetAllAssetBundles();
        List<string> luaAssetBundlePathList = new List<string>();
        for (int i = 0; i < allPath.Length; i++)
        {
            string[] split = allPath[i].Split('/');
            if (split[0] == "lua")
            {
                luaAssetBundlePathList.Add(allPath[i]);
            }
        }

        for (int i = 0; i < luaAssetBundlePathList.Count; i++)
        {
            string path = "file://" + PathDefine.presitantABPath(Helper.GetPlatformString()) + "AssetsBundle/" + luaAssetBundlePathList[i];
            Debug.Log("路径  " + path);
            using (WWW www = new WWW(path))
            {
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    AssetBundle singleAB = www.assetBundle;
                    Object[] objectList = singleAB.LoadAllAssets();
                    string[] allAssetNames = singleAB.GetAllAssetNames();
                    for (int j = 0; j < objectList.Length; j++)
                    {
                        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(objectList[j].ToString());

                        string savePath = PathDefine.presitantABPath(Helper.GetPlatformString()) + "LuaFile/";

                        string fileName = allAssetNames[j].Replace("assets/luabyte/", "").Replace(".lua.bytes", ".lua");

                        Helper.SaveAssetToLocalFile(savePath, fileName, byteArray);
                    }
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }
        }

        if (cb != null)
        {
            cb.Invoke();
        }
    }


    private void OnDestroy()
    {
        luaEnv.Dispose();
    }
}
