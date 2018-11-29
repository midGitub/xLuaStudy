using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class Launcher : MonoBehaviour
{
    private GameObject managerGroupObj;



    private void Start()
    {

    }

    private void Awake()
    {
        GameObject canvas = GameObject.Find("Canvas");

        string savePath = Application.persistentDataPath;

        string serverPath = string.Empty;
        string rootAssetName = string.Empty;
#if UNITY_EDITOR
        serverPath = "http://192.168.1.175/AssetBundleEditor";
        rootAssetName = "AssetBundleEditor";
#elif !UNITY_EDITOR && UNITY_ANDROID
        serverPath = "http://192.168.1.175/AssetBundleAndroid";
          rootAssetName = "AssetBundleAndroid";
#endif
        AssetBundleManager.Instance.DownLoadAssetsToLocalWithDependencies(serverPath, rootAssetName, "lua/lua", savePath, () =>
        {
            AssetBundle ab = AssetBundleManager.Instance.GetLoadAssetFromLocalFile(rootAssetName, "lua/lua", "lua", Application.persistentDataPath);

            AssetBundleRequest assetBundleRequest = ab.LoadAllAssetsAsync();
            
            for (int i = 0; i < assetBundleRequest.allAssets.Length; i++)
            {
                //assetBundleRequest.allAssets[i].

                Stream sw = null;
                FileInfo fileInfo = new FileInfo(savePath + "/lua/" + assetBundleRequest.allAssets[i].name);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
                CheckPath(fileInfo.Directory.FullName);
                //如果此文件不存在则创建
                sw = fileInfo.Create();
                //写入
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(assetBundleRequest.allAssets[i].ToString());
                sw.Write(byteArray, 0, byteArray.Length);

                sw.Flush();
                //关闭流
                sw.Close();
                //销毁流
                sw.Dispose();
            }
        });

        LuaManager.Instance.Init();
    }




    public void Update()
    {

    }

    public void CheckPath(string path)
    {
        string[] s = path.Split('\\');
        path = path.Replace('\\', '/');
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary> 
    /// 将一个object对象序列化，返回一个byte[]         
    /// </summary> 
    /// <param name="obj">能序列化的对象</param>         
    /// <returns></returns> 
    public static byte[] ObjectToBytes(object obj)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            return ms.GetBuffer();
        }
    }

}

public class aaa
{
    public static void bbb()
    {
        GameObject a = AssetBundleManager.Instance.gameObject;
    }
}