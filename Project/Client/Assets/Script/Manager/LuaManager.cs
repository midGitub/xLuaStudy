using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using XLua;

[CSharpCallLua]
public class LuaManager : SingletonBehaviour<LuaManager>
{
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
        Action<int, int> onLoadSingleFinishCallBack = (curCount, allCount) =>
        {
            UILoadingView.Instance.Refresh(curCount, allCount, "初始化LUA脚本中");
        };

        Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack = (allData) =>
        {
            this.allLuaDict = allData;
            InitLuaEnv();
        };

        LoaderManager.LoadAllLuaSync(
            GameSetting.Instance.from,
            onLoadSingleFinishCallBack,
            onLoadAllFinishCallBack);
    }

    private void OnDestroy()
    {
        luaEnv.Dispose();
    }
}
