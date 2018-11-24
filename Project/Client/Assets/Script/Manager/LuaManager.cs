using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : BaseManager
{
    public static LuaEnv luaEnv;
    /// <summary>
    /// lua文件保存地址
    /// </summary>
    private string luaFilePath;
    protected override void Awake()
    {
        base.Awake();
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(LoadLuaFile);
#if UNITY_EDITOR
        //luaFilePath = "Assets/Lua/";
        luaFilePath = Application.persistentDataPath + "/lua/";
#elif UNITY_ANDROID && !UNITY_EDITOR
         luaFilePath = Application.persistentDataPath + "/lua/";
#endif
    }

    public void Init()
    {
        //设置xlua脚本的路径   路径根据实际需求修改
        luaEnv.DoString("require('main')");
        LuaFunction mainFunction = luaEnv.Global.Get<LuaFunction>("main");
        mainFunction.Call();
    }

    private byte[] LoadLuaFile(ref string filePath)
    {
        string absPath = luaFilePath + filePath + ".lua";
        return System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(absPath));
    }

    private void OnDestroy()
    {
        luaEnv.Dispose();
    }
}
