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
    private int curVersionCode = -1;

    private List<string> allSAFilePathList = new List<string>();
    private uint pathCount = 0;
    private void Start()
    {

    }
    List<uint> ateste = new List<uint>();
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


        //System.Action<System.Action<int>>[] tasks = new System.Action<System.Action<int>>[2];

        //tasks[0] = (cb) =>
        // {
        //     WWWForm wwwForm = new WWWForm();
        //     wwwForm.headers.Add("headersKey", "headersValue");
        //     byte[] byteArray1 = System.Text.Encoding.UTF8.GetBytes("ddd");
        //     wwwForm.AddBinaryData("2222", byteArray1);
        //     NetWorkManager.Instance.PostWebMSG("http://192.168.1.175/GetVersion:8080/", wwwForm, (www) =>
        //     {
        //         Debug.LogError(www.text);
        //         cb((int)LocalCode.SUCCESS);
        //     });
        // };
        //return;

        string[] sPath = Directory.GetFiles(Application.streamingAssetsPath);
        string[] allSAFilePathGroup = Helper.GetFiles(Application.streamingAssetsPath, null, true, true);

        for (int i = 0; i < allSAFilePathGroup.Length; i++)
        {
            allSAFilePathGroup[i] = allSAFilePathGroup[i].Replace("\\", "/");
            if (!allSAFilePathGroup[i].Contains(".meta") && !allSAFilePathGroup[i].Contains(".manifest"))
            {
                allSAFilePathList.Add(allSAFilePathGroup[i]);
            }
        }

        StartCoroutine(WWWCopyCoroutine());

        return;
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
                Helper.SaveAssetToLocalFile(Application.persistentDataPath, "version.json", byteArray);
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

    /// <summary>
    /// 将streaming path 下的文件copy到对应用
    /// 为什么不直接用io函数拷贝，原因在于streaming目录不支持，
    /// 不管理是用getStreamingPath_for_www，还是Application.streamingAssetsPath，
    /// io方法都会说文件不存在
    /// </summary>
    /// <param name="fileName"></param>
    IEnumerator WWWCopyCoroutine()
    {
        List<string>.Enumerator enumerator = allSAFilePathList.GetEnumerator();
        while (enumerator.MoveNext() == true)
        {
            string sourcePath = enumerator.Current;
            string[] sourcePathGroup = sourcePath.Split('/');
            string posteriorSourcePath = string.Empty;
            for (int i = sourcePathGroup.Length - 1; i > 0; i--)
            {
                if (sourcePathGroup[i] == "StreamingAssets")
                {
                    break;
                }
                posteriorSourcePath = sourcePathGroup[i] + (i == sourcePathGroup.Length - 1 ? string.Empty : "/") + posteriorSourcePath;
            }


            string targetPath = Application.persistentDataPath + "/" + posteriorSourcePath;
            string[] targetPathGroup = targetPath.Split('/');

            string needCheckDirectoryPath = string.Empty;//需要检查的文件夹地址
            for (int i = 0; i < targetPathGroup.Length - 1; i++)
            {
                needCheckDirectoryPath = needCheckDirectoryPath + (i == 0 ? string.Empty : "/") + targetPathGroup[i];
            }
            Helper.CheckPathExistence(needCheckDirectoryPath);

            Debug.Log("des:" + targetPath);
            WWW www = new WWW(GetStreamingPathPre() + sourcePath);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log("www.error:" + www.error);
            }
            else
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                FileStream fsDes = File.Create(targetPath);
                fsDes.Write(www.bytes, 0, www.bytes.Length);
                fsDes.Flush();
                fsDes.Close();
            }
            www.Dispose();
        }

    }

    string GetStreamingPathPre()
    {
        string pre = string.Empty;
#if UNITY_EDITOR
        pre = "file://";
#elif UNITY_ANDROID
        pre = "";
#elif UNITY_IPHONE
	    pre = "file://";
#endif
        return pre;
    }
}