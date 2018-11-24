using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Launcher : MonoBehaviour
{
    private GameObject managerGroupObj;

    private List<BaseManager> managerList = new List<BaseManager>();

    private void Awake()
    {
        managerGroupObj = GameObject.Find("ManagerGroup");
        DontDestroyOnLoad(managerGroupObj);
    }

    private void Start()
    {
        //GameObject luaMgrObj = new GameObject();
        //luaMgrObj.name = "LuaManager";
        //luaMgrObj.transform.SetParent(managerGroupObj.transform);
        //luaMgrObj.AddComponent<LuaManager>();
        //LuaManager.Instance.Init();

        GameObject canvas = GameObject.Find("Canvas");

        string savePath = Application.persistentDataPath;

        string serverPath = string.Empty;

#if UNITY_EDITOR
        serverPath = "http://192.168.1.175/AssetBundleEditor";
#elif !UNITY_EDITOR && UNITY_ANDROID
        serverPath = "http://192.168.1.175/AssetBundleAndroid";
#endif

        LoadAssetBundle.Instance.DownLoadAssets2LocalWithDependencies(serverPath, "AssetBundleEditor", "view/uirootview.prefab.unity3d", savePath, () =>
        {
            GameObject obj = LoadAssetBundle.Instance.GetLoadAssetFromLocalFile("AssetBundleEditor", "view/uirootview.prefab.unity3d", "uirootview.prefab", Application.persistentDataPath);
            GameObject view = GameObject.Instantiate(obj);
            view.transform.SetParent(canvas.transform, false);
        });

    }
}
// Use this for initialization

//5.0版本打包时候选中需要打包的东西然后设置右下角名称,同个/设置多集目录,后面的框标记后缀(后缀不重要)
//打包时候的目标文件夹,假设目标文件夹名称为"WJJ",那么会生成"WJJ"和"WJJ.manifest"两个文件
//其中WJJ.manifest文件没有用,只是用来看的,WJJ是一个assetbundle包,里面包含了整个文件夹的依赖信息
//可以先加载这个东西,然后获取到依赖关系后逐步加载



