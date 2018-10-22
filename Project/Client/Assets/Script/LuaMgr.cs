using System.IO;
using UnityEngine;
using XLua;

public class LuaMgr : MonoBehaviour
{
    public static LuaMgr Instance;
    public static LuaEnv luaEnv;
    /// <summary>
    /// lua文件保存地址
    /// </summary>
    private string luaFilePath;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(LoadLuaFile);
#if UNITY_EDITOR
        luaFilePath = "Assets/Lua";
#elif UNITY_ANDROID && !UNITY_EDITOR
        luaFilePath = Application.streamingAssetsPath;
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
        string absPath = luaFilePath + "/" + filePath + ".lua";
        return System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(absPath));
    }

    private void OnDestroy()
    {
        luaEnv.Dispose();
    }
}
