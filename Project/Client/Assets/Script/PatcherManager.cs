using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PatcherManager : MonoBehaviour
{
    private static PatcherManager instance;
    public static PatcherManager Instance
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

                instance = managerGroup.GetComponentInChildren<PatcherManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = managerGroup.transform;
                    go.name = typeof(PatcherManager).Name;
                    instance = go.AddComponent<PatcherManager>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// 所有在StreamingAsset里面的文件路径
    /// </summary>
    private List<string> allSAFilePathList = new List<string>();

    /// <summary>
    /// 当前版本文件
    /// </summary>
    private VersionJsonObject localVersionJsonObj = new VersionJsonObject();

    private UILoadingView uiLoadingView;
    public void Awake()
    {
        GameObject canvas = GameObject.Find("Canvas");
        uiLoadingView = canvas.transform.Find("UILoadingView").GetComponent<UILoadingView>();
    }

    /// <summary>
    /// 检查热更
    /// </summary>
    /// <param name="finalCB"></param>
    public void Check(Action<int> finalCB)
    {
        //平台名称
        string pfStr = Helper.GetPlatformString();

        //后台版本号
        int serverVersionCode = -1;

        VersionIsNewPackage server_vinp = null;//服务器版本对应的包体信息

        List<VersionIsNewPackage> allPackageVersionList = new List<VersionIsNewPackage>();

        System.Action<System.Action<int>>[] tasks = new System.Action<System.Action<int>>[5];

        tasks[0] = (cb) =>
        {
            //先用WWW加载本地version.json文件(Android环境下只能用WWW加载)
            //因为在复制的过程中需要用到这个文件
            string path = string.Empty;
            bool isExitsInPD =
                File.Exists(PathDefine.presitantABPath(pfStr) + "Version/version.json");
            if (isExitsInPD)
            {
                path = PathDefine.presitantABPath(pfStr) + "Version/version.json";
            }
            else
            {
                path = PathDefine.StreamingAssetsPathByPF(pfStr) + "Version/version.json";
            }

            WWW request = new WWW(path);
            if (string.IsNullOrEmpty(request.error))
            {
                localVersionJsonObj = Helper.LoadVersionJson(request.text);
                cb((int)LocalCode.SUCCESS);
            }
            else
            {
                cb((int)LocalCode.DownloadVersionJsonFault);
            }
        };

        tasks[1] = (cb) =>
        {
            //将streamingAssets下的文件copy到从StreamingAssets至persistentDataPath
            if (PlayerPrefs.GetInt("SACopyToPD" + GameSetting.Instance.versionCode, 0) == 0 || GameSetting.Instance.forceCopy == true)
            {
                StartCoroutine(SACopyToPDCoroutine(cb));
            }
            else
            {
                cb((int)LocalCode.SUCCESS);
            }
        };

        tasks[2] = (cb) =>
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

        tasks[3] = (cb) =>
        {
            //拿到服务器版本之后 先判断当前版本是否需要热更(先判断presitantDataPath里面有没有这个文件，若没有 则使用GameSetting里面的VersionCode)
            //若需要热更新 去CDN（即本地服务器的AssetsBundle目录）下载AllPackageVersion.json
            //用于判断当前版本是整包版本还是热更版本
            if (serverVersionCode > localVersionJsonObj.version)//判断服务器版本是否大于当前版本
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

                        // 下载完了AllPackageVersion.json 
                        // 根据服务器下发的版本去资源库取对应版本资源
                        // 资源库里面有一个文件记录当前版本是否热更版本的文件
                        // 然后判断当前资源库版本是否是热更版本

                        server_vinp = allPackageVersionList.Find(t => t.version == serverVersionCode);

                        if (server_vinp != null)
                        {
                            if (true == server_vinp.isNewPackage)
                            {
                                //不需要更新

                                //GameSetting.Instance.versionCode 这个值是在出包的时候设置的
                                //判断在PC上出整包的版本是否是服务器上的最新版本
                                if (GameSetting.Instance.versionCode == serverVersionCode)
                                {
                                    cb((int)LocalCode.PATCHER_END);
                                }
                                else
                                {
                                    //需要换包
                                    cb((int)LocalCode.CurServerVerIsNewPackage);
                                }
                            }
                            else
                            {
                                //需要更新

                                cb((int)LocalCode.SUCCESS);
                            }
                        }
                        else
                        {
                            //在资源库中找不到对应版本资源
                            cb((int)LocalCode.CanNotFindVersionInCDN);
                        }
                    }
                    else
                    {
                        cb((int)LocalCode.DownloadAllPackageVersionFault);
                    }
                });
            }
            else
            {
                //当前版本已是最新 无需更新
                cb((int)LocalCode.CurVerIsNewest);
            }
        };

        tasks[4] = (cb) =>
        {
            //到达此步  已经下载完AllPackageVersion.json 
            //本地的version.json文件在上面已经加载好了
            //需要再去服务器上拿最新的version.json文件
            //对比哪些文件需要下载

            NetWorkManager.Instance.Download(PathDefine.serverPath(pfStr, serverVersionCode) + "version.json", (WWW www) =>
            {
                VersionJsonObject serverVersionJson = Helper.LoadVersionJson(www.text);

                List<string> shouldDownloadList = new List<string>();

                if (serverVersionJson.version > localVersionJsonObj.version)//服务器版本大于本地版本
                {
                    //下面检测该下哪些Bundle
                    foreach (ABNameHash singleServiceNameHash in serverVersionJson.ABHashList)
                    {
                        ABNameHash singleLocalNameHash = localVersionJsonObj.ABHashList.Find(t => t.abName == singleServiceNameHash.abName);
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

                    //保存当前这份最新的 version.json 文件
                    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(www.text);
                    Helper.SaveAssetToLocalFile(PathDefine.presitantABPath(pfStr), "Version/version.json", byteArray);
                }

                if (shouldDownloadList.Count > 0)
                {
                    //拿到对应的fileversion  然后取版本号
                    //取到版本号之后开始从资源库上对应文件夹下载资源
                    string fileVersionPath = PathDefine.serverPath(pfStr, serverVersionCode) + "fileversion.json";
                    NetWorkManager.Instance.Download(fileVersionPath, (WWW wwwResult) =>
                    {
                        if (string.IsNullOrEmpty(wwwResult.error))
                        {
                            FileVersionJsonObject fileVersionJsonObject = Helper.LoadFileVersionJson(wwwResult.text);

                            List<VersionAndSize> vasList = new List<VersionAndSize>();
                            foreach (string name in shouldDownloadList)
                            {
                                VersionAndSize vas = fileVersionJsonObject.versionSizeList.Find(t => t.name == name);
                                vasList.Add(vas);
                            }

                            AssetBundleManager.Instance.DownLoadAssetBundleByList(vasList, pfStr, cb);
                        }
                        else
                        {
                            cb((int)LocalCode.DownloadFileVersionJsonFault);
                        }
                    });
                }
            });
        };

        AsyncHelper asyncHelper = new AsyncHelper();
        asyncHelper.Waterfall(tasks, finalCB);
    }

    /// <summary>
    /// 将streamingAssets下的文件copy到从StreamingAssets至persistentDataPath
    /// 为什么不直接用io函数拷贝，原因在于streaming目录不支持，
    /// </summary>
    private IEnumerator SACopyToPDCoroutine(Action<int> cb)
    {
        List<ABNameHash>.Enumerator enumerator = localVersionJsonObj.ABHashList.GetEnumerator();
        int curIndex = 0;
        while (enumerator.MoveNext())
        {
            string path = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" +
                          enumerator.Current.abName;
            WWW request = new WWW(path);

            Debug.LogError(path);

            yield return request;
            if (string.IsNullOrEmpty(request.error))
            {
                string savePath = Application.persistentDataPath + "/" + Helper.GetPlatformString() + "/AssetsBundle/" + enumerator.Current.abName;
                string[] nameSplit = enumerator.Current.abName.Split('/');

                byte[] byteArray = request.bytes;

                Helper.SaveAssetToLocalFile(savePath, byteArray);

                curIndex++;
                uiLoadingView.Refresh(curIndex, localVersionJsonObj.ABHashList.Count,
                    "正在复制文件(从StreamingAssets至persistentDataPath)");
            }
            else
            {
                Debug.LogError(request.error);
                cb.Invoke((int)LocalCode.SACopyToPDCoroutineFault);
            }
            request.Dispose();
            Debug.Log("enumerator.Current.abName  " + enumerator.Current.abName);
        }

        PlayerPrefs.SetInt("SACopyToPD" + GameSetting.Instance.versionCode, 1);
        if (cb != null)
        {
            cb.Invoke((int)LocalCode.SUCCESS);
        }
    }
}

