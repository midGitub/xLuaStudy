using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LitJson;

public class Launcher : MonoBehaviour
{
    private GameObject managerGroupObj;
    private int serverVersionCode = -1;

    private List<string> allSAFilePathList = new List<string>();
    private uint pathCount = 0;

    private UILoadingView uiLoadingView;
    private void Start()
    {

    }
    List<uint> ateste = new List<uint>();
    private void Awake()
    {
        GameObject canvas = GameObject.Find("Canvas");

        uiLoadingView = canvas.transform.FindChild("UILoadingView").GetComponent<UILoadingView>();

        string savePath = Application.persistentDataPath;

        string pfStr = string.Empty;
        string serverPath = string.Empty;
        string rootAssetName = string.Empty;
#if UNITY_EDITOR
        serverPath = "http://192.168.1.175/AssetBundleEditor";
        pfStr = "Editor";
        rootAssetName = "AssetBundleEditor";
#elif !UNITY_EDITOR && UNITY_ANDROID
        pfStr = "Android";
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

        List<VersionIsNewPackage> allPackageVersionList = new List<VersionIsNewPackage>();

        System.Action<System.Action<int>>[] tasks = new System.Action<System.Action<int>>[3];

        Action<int> finalCb = (code) =>
        {
            if (code == (int)LocalCode.SUCCESS)
            {
                Debug.Log("热更流程" + (LocalCode)code);
            }
            else
            {
                Debug.LogError("热更流程" + (LocalCode)code);
            }
        };

        tasks[0] = (cb) =>
        {
            //获取当前可升级到的版本（模拟访问PHP返回数据） 现在并不需要对数据做任何处理
            WWWForm wwwForm = new WWWForm();
            wwwForm.headers.Add("headersKey", "headersValue");
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("getVersion");
            wwwForm.AddBinaryData(byteArray.ToString(), byteArray);
            Debug.Log("请求服务器下发可运行版本 --- ");
            NetWorkManager.Instance.PostWebMSG("http://192.168.1.175/GetVersion:8080/", wwwForm, (www) =>
            {
                Debug.Log("收到服务器下发可运行版本 --- " + www.text);
                serverVersionCode = int.Parse(www.text);
                cb((int)LocalCode.SUCCESS);
            });
        };

        tasks[1] = (cb) =>
        {
            //拿到服务器版本之后 先判断当前版本是否需要热更(先判断presitantDataPath里面有没有这个文件，若没有 则使用GameSetting里面的VersionCode)
            //若需要热更新 去CDN（即本地服务器的AssetsBundle目录）下载AllPackageVersion.json
            //用于判断当前版本是整包版本还是热更版本

            int localVersion = -1;
            if (File.Exists(PathDefine.presitantABPath(pfStr) + "/version.json"))
            {
                VersionJsonObject versionJsonObj = Helper.LoadVersionJson(File.ReadAllText(PathDefine.presitantABPath(pfStr) + "/version.json"));
                localVersion = (int)versionJsonObj.version;
            }
            else
            {
                localVersion = GameSetting.Instance.versionCode;
            }

            if (serverVersionCode > localVersion)//判断版本
            {
                NetWorkManager.Instance.Download(PathDefine.serverPath(pfStr) + "AllPackageVersion.json", (www) =>
                {
                    if (string.IsNullOrEmpty(www.error))
                    {
                        JsonData jsonData = JsonMapper.ToObject(www.text);
                        int count = jsonData.Count;

                        for (int i = 0; i < count; i++)
                        {
                            JsonData verIsNewPkgJD = jsonData[i];

                            VersionIsNewPackage vinp = new VersionIsNewPackage();
                            vinp.version = uint.Parse(verIsNewPkgJD["Version"].ToJson());
                            vinp.isNewPackage = bool.Parse(verIsNewPkgJD["isNewPackage"].ToJson());

                            allPackageVersionList.Add(vinp);
                        }

                        //todo  下载完了AllPackageVersion.json  然后判断当前版本是否是热更版本



                        cb((int)LocalCode.SUCCESS);
                    }
                    else
                    {
                        cb((int)LocalCode.DownloadAllPackageVersionFail);
                    }
                });
            }
            else
            {
                cb((int)LocalCode.PATCHER_END);
            }
        };

        tasks[2] = (cb) =>
        {

        };

        AsyncHelper asyncHelper = new AsyncHelper();
        asyncHelper.Waterfall(tasks, finalCb);

        return;
        string url = serverPath + "/version.json";

        NetWorkManager.Instance.Download(url, (WWW www) =>
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

            VersionJsonObject serviceVersion = Helper.LoadVersionJson(www.text);

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
                // byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(content);
                // Helper.SaveAssetToLocalFile(Application.persistentDataPath, "version.json", byteArray);
            }

            if (shouldDownloadList.Count > 0)
            {
                AssetBundleManager.Instance.DownLoadAssetBundleByList(shouldDownloadList);
            }
        });
    }

    /// <summary>
    /// 将streamingAssets下的文件copy到从StreamingAssets至persistentDataPath
    /// 为什么不直接用io函数拷贝，原因在于streaming目录不支持，
    /// </summary>
    IEnumerator SACopyToPDCoroutine(Action<int> cb)
    {
        List<string>.Enumerator enumerator = allSAFilePathList.GetEnumerator();
        int curIndexInAllSAFilePathList = 0;
        while (enumerator.MoveNext() == true)
        {
            curIndexInAllSAFilePathList++;
            uiLoadingView.Refresh(curIndexInAllSAFilePathList, allSAFilePathList.Count, "正在复制文件(从StreamingAssets至persistentDataPath)");
            string sourcePath = enumerator.Current;
            string[] sourcePathGroup = sourcePath.Split('/');

            //解析源路径
            string posteriorSourcePath = string.Empty;
            for (int i = sourcePathGroup.Length - 1; i > 0; i--)
            {
                if (sourcePathGroup[i] == "StreamingAssets")
                {
                    break;
                }
                posteriorSourcePath = sourcePathGroup[i] + (i == sourcePathGroup.Length - 1 ? string.Empty : "/") + posteriorSourcePath;
            }

            string targetPath = Application.persistentDataPath + "/" + posteriorSourcePath;//构建目标路径
            string[] targetPathGroup = targetPath.Split('/');

            string needCheckDirectoryPath = string.Empty;//需要检查的文件夹地址
            for (int i = 0; i < targetPathGroup.Length - 1; i++)
            {
                needCheckDirectoryPath = needCheckDirectoryPath + (i == 0 ? string.Empty : "/") + targetPathGroup[i];
            }
            Helper.CheckPathExistence(needCheckDirectoryPath);

            WWW www = new WWW(GetStreamingPathPre() + sourcePath);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log("SACopyToPDCoroutine Error:" + www.error);
                if (cb != null)
                {
                    cb.Invoke((int)LocalCode.FAILED);
                }
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

        if (cb != null)
        {
            cb.Invoke((int)LocalCode.SUCCESS);
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