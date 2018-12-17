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
        //AssetBundleManager.Instance.DownLoadAssetsToLocalWithDependencies(serverPath, rootAssetName, "lua/Base", savePath, () =>
        //{
        //    AssetBundle ab = AssetBundleManager.Instance.GetLoadAssetFromLocalFile(rootAssetName, "lua/Base", "lua", Application.persistentDataPath);

        //    AssetBundleRequest assetBundleRequest = ab.LoadAllAssetsAsync();

        //    for (int i = 0; i < assetBundleRequest.allAssets.Length; i++)
        //    {
        //        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(assetBundleRequest.allAssets[i].ToString());
        //        Helper.SaveAssetToLocalFile(savePath + "/lua/", assetBundleRequest.allAssets[i].name, byteArray, byteArray.Length);
        //    }
        //});

        //AssetBundleManager.Instance.DownLoadAssetsToLocalWithDependencies(serverPath, rootAssetName, "lua/View", savePath, () =>
        //{
        //    AssetBundle ab = AssetBundleManager.Instance.GetLoadAssetFromLocalFile(rootAssetName, "lua/View", "lua", Application.persistentDataPath);

        //    AssetBundleRequest assetBundleRequest = ab.LoadAllAssetsAsync();

        //    for (int i = 0; i < assetBundleRequest.allAssets.Length; i++)
        //    {
        //        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(assetBundleRequest.allAssets[i].ToString());
        //        Helper.SaveAssetToLocalFile(savePath + "/lua/", assetBundleRequest.allAssets[i].name, byteArray, byteArray.Length);
        //    }
        //});

        //LuaManager.Instance.Init();

        string url = serverPath + "/version.json";

        NetWorkManager.Instance.Download(url, (string content) =>
        {
            //检测persistentDataPath地址里有没有这个文件,如果有 则将其作为本地版本
            VersionJsonObject localVersion = null;
            if (File.Exists(Application.persistentDataPath + "/version.json"))
            {
                localVersion = Helper.LoadVersionJson(File.ReadAllText(Application.persistentDataPath + "/version.json"));
            }
            else
            {
                localVersion = Helper.LoadVersionJson(Resources.Load("version").ToString());
            }

            string s = Resources.Load("version").ToString();

            VersionJsonObject serviceVersion = Helper.LoadVersionJson(content);

            List<string> shouldDownloadList = new List<string>();

            if (serviceVersion.version > localVersion.version)//服务器版本大于本地版本
            {
                //下面检测该下哪些Bundle
                foreach (ABNameHash singleServiceNameHash in serviceVersion.ABHashList)
                {
                    ABNameHash singleLocalNameHash = localVersion.ABHashList.Find(t => t.abName == singleServiceNameHash.abName);
                    if (singleLocalNameHash != null)
                    {
                        if (singleLocalNameHash.hashCode != singleServiceNameHash.hashCode)
                        {
                            shouldDownloadList.Add(singleServiceNameHash.abName);
                        }
                    }
                    else
                    {
                        shouldDownloadList.Add(singleServiceNameHash.abName);
                    }
                }

                //保存当前这份最新的文件
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(content);
                Helper.SaveAssetToLocalFile(Application.persistentDataPath, "version.json", byteArray, byteArray.Length);
            }

            if (shouldDownloadList.Count > 0)
            {
                AssetBundleManager.Instance.DownLoadAssetBundleByList(shouldDownloadList);
            }

        });
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