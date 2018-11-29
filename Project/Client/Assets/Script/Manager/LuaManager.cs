using System.IO;
using UnityEngine;
using XLua;

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
            luaFilePath = Application.persistentDataPath + "/lua/";
        }
        else
        {
            luaFilePath = "Assets/Lua/";
        }
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
