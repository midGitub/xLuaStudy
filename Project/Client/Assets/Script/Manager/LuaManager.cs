using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using XLua;

[CSharpCallLua]
public class LuaManager : MonoBehaviour
{
    #region Instance
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
    #endregion

    public static LuaEnv luaEnv;

    private Dictionary<string, byte[]> allLuaDict = new Dictionary<string, byte[]>();

    public void Init()
    {
        LoadAllLuaAssetsBundle();
    }

    private void InitLuaEnv()
    {
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(LoadLuaFile);

        luaEnv.DoString("require('Main')");
        LuaFunction mainFunction = luaEnv.Global.Get<LuaFunction>("main");
        mainFunction.Call();
    }

    private byte[] LoadLuaFile(ref string filePath)
    {
        string name = filePath.ToLower();
        return allLuaDict[name];
    }

    //加载所有LUA的AB，将其内容存到字典内
    private void LoadAllLuaAssetsBundle()
    {
        FileVersionJsonObject fileversion = AssetBundleManager.Instance.fileVersionJsonObject;
        List<VersionAndSize> vasList = fileversion.versionSizeList.FindAll(t => t.name.Contains("lua/"));

        int dCount = 0;
        for (int i = 0; i < vasList.Count; i++)
        {
            string path = string.Empty;
            if (vasList[i].version > GameSetting.Instance.versionCode)
            {
                path = PathDefine.presitantABPath(Helper.GetPlatformString()) + "AssetsBundle/" + vasList[i].name;
#if UNITY_EDITOR
                path = PathDefine.presitantABPath(Helper.GetPlatformString()) + "AssetsBundle/" + vasList[i].name;
#elif !UNITY_EDITOR && UNITY_ANDROID
                path ="file://"+ PathDefine.presitantABPath(Helper.GetPlatformString()) + "AssetsBundle/" + vasList[i].name;
#endif
            }
            else
            {
                path = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" + vasList[i].name;
            }

            Action<UnityWebRequest> DownloadCB = (request) =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError("解压失败  ---- " + request.url);
                    Debug.LogError("解压失败  ---- " + request.error);
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
                if (dCount == vasList.Count)
                {
                    Debug.Log("加载LUA完成");
                    InitLuaEnv();
                }
            };
            Debug.LogError(path);
            UnityWebRequestManager.Instance.DownloadBuffer(path, DownloadCB);
        }
    }

    private void OnDestroy()
    {
        luaEnv.Dispose();
    }
}
