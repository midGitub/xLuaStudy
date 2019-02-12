using System;
using UnityEngine;
using XLua;

public class LuaViewBehaviour : MonoBehaviour
{

    [CSharpCallLua]
    public delegate void delLuaAwake(GameObject obj);
    delLuaAwake luaAwake;

    [CSharpCallLua]
    public delegate void delLuaStart();
    delLuaStart luaStart;

    [CSharpCallLua]
    public delegate void delLuaUpdate();
    delLuaUpdate luaUpdate;

    [CSharpCallLua]
    public delegate void delLuaOnDestroy();
    delLuaOnDestroy luaOnDestroy;

    private LuaTable scriptEnv;
    private LuaEnv luaEnv;

    private void Awake()
    {
        Debug.LogError("awake");
        //获取全局的Lua环境变量
        luaEnv = LuaManager.luaEnv;

        scriptEnv = luaEnv.NewTable();

        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        string prefabName = name;
        //去掉克隆的关键字
        if (prefabName.Contains("(Clone)"))
        {
            prefabName = prefabName.Split(new string[] { "(Clone)" }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        prefabName = prefabName.Replace("pan_", "");
        name = prefabName;

        //  prefabName + ".awake"  要对应Lua脚本中View的方法
        luaAwake = scriptEnv.GetInPath<LuaViewBehaviour.delLuaAwake>(prefabName + ".Awake");
        luaStart = scriptEnv.GetInPath<LuaViewBehaviour.delLuaStart>(prefabName + ".Start");
        luaUpdate = scriptEnv.GetInPath<LuaViewBehaviour.delLuaUpdate>(prefabName + ".Update");
        luaOnDestroy = scriptEnv.GetInPath<LuaViewBehaviour.delLuaOnDestroy>(prefabName + ".OnDestroy");

        scriptEnv.Set("self", this);
        if (luaAwake != null)
        {
            luaAwake(this.gameObject);
        }
    }


    private void Start()
    {

        if (luaStart != null)
        {
            luaStart();
        }
    }

    private void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
    }


    private void OnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy();
        }
        luaAwake = null;
        luaOnDestroy = null;
        luaUpdate = null;
        luaStart = null;
    }

    public LuaTable GetLuaTable()
    {
        return scriptEnv.GetInPath<LuaTable>(name);
    }
}
