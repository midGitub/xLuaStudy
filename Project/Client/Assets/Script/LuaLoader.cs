using UnityEngine;
using System.IO;
using XLua;

public class LuaLoader : MonoBehaviour
{
    /// <summary>
    /// lua文件保存地址
    /// </summary>
    private string luaFilePath;

    LuaEnv luaenv = null;
    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        luaFilePath = "Assets/Lua";
#elif UNITY_ANDROID && !UNITY_EDITOR
        luaFilePath = Application.streamingAssetsPath;
#endif

        luaenv = new LuaEnv();
        luaenv.AddLoader(LoadLuaFile);

        luaenv.DoString("require('main')");

        LuaFunction mainFunction = luaenv.Global.Get<LuaFunction>("main");
        mainFunction.Call();
    }

    private void OnDestroy()
    {
        luaenv.Dispose();
    }

    private byte[] LoadLuaFile(ref string filePath)
    {
        string absPath = luaFilePath + "/" + filePath + ".lua";
        return System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(absPath));
    }
}
