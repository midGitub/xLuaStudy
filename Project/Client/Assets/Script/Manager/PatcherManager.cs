﻿using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class PatcherManager : SingletonBehaviour<PatcherManager>
{
    public List<string> shouldDownloadList = new List<string>();

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

        System.Action<System.Action<int>>[] tasks = new System.Action<System.Action<int>>[4];

        tasks[0] = (cb) =>
        {
            //先用UnityWebRequest加载本地version.json文件
            string path = string.Empty;
            bool isExitsInPD = File.Exists(PathDefine.presitantABPath() + "Version/version.json");
            Debug.Log("isExitsInPD  " + isExitsInPD);
            if (isExitsInPD)
            {
                path = PathDefine.presitantABPath() + "Version/version.json";
            }
            else
            {
                path = PathDefine.StreamingAssetsPathByPF(pfStr) + "Version/version.json";
            }

            uiLoadingView.Refresh(0, 1, "读取本地版本信息文件中");

            Action<UnityWebRequest> DownloadCB = (request) =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                    cb((int)LocalCode.DOWNLOAD_VERSIONJSON_FAULT);
                }
                else
                {
                    localVersionJsonObj = Helper.LoadVersionJson(request.downloadHandler.text);
                    cb((int)LocalCode.SUCCESS);
                }
            };

            UnityWebRequestManager.Instance.DownloadBuffer(path, DownloadCB);
        };

        tasks[1] = (cb) =>
        {
            //获取当前可升级到的版本（模拟访问PHP返回数据） 现在并不需要对数据做任何处理
            WWWForm wwwForm = new WWWForm();
            wwwForm.headers.Add("headersKey", "headersValue");
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("getVersion");
            wwwForm.AddBinaryData(byteArray.ToString(), byteArray);
            Debug.Log("请求服务器下发可运行版本");
            UnityWebRequestManager.Instance.Post("http://192.168.1.175/GetVersion:8080/", wwwForm, (request) =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                    cb((int)LocalCode.REQ_SERVER_VERSION_CODE_FAULT);
                }
                else
                {
                    Debug.Log(" unitywebrequest 收到服务器下发可运行版本 版本号为:" + request.downloadHandler.text);
                    serverVersionCode = int.Parse(request.downloadHandler.text);
                    cb((int)LocalCode.SUCCESS);
                }
            });
        };

        tasks[2] = (cb) =>
        {
            //拿到服务器版本之后 先判断当前版本是否需要热更(先判断presitantDataPath里面有没有这个文件，若没有 则使用GameSetting里面的VersionCode)
            //若需要热更新 去CDN（即本地服务器的AssetsBundle目录）下载AllPackageVersion.json

            Action<UnityWebRequest> DownloadCB = (request) =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                    cb((int)LocalCode.DOWNLOAD_ALL_PACKAGEVERSION_FAULT);
                }
                else
                {
                    JsonData jsonData = JsonMapper.ToObject(request.downloadHandler.text);
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
                                cb((int)LocalCode.CUR_SERVERVER_IS_NEWPACKAGE);
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
                        cb((int)LocalCode.CAN_NOT_FIND_VERSION_IN_CDN);
                    }
                }
            };

            //用于判断当前版本是整包版本还是热更版本
            if (serverVersionCode > localVersionJsonObj.version)//判断服务器版本是否大于当前版本
            {
                UnityWebRequestManager.Instance.DownloadBuffer(PathDefine.serverPath(pfStr) + "AllPackageVersion.json", DownloadCB);
            }
            else
            {
                //当前版本已是最新 无需更新
                cb((int)LocalCode.CUR_VER_IS_NEWEST);
            }
        };

        tasks[3] = (cb) =>
        {
            //到达此步  已经下载完AllPackageVersion.json 
            //本地的version.json文件在上面已经加载好了
            //需要再去服务器上拿最新的version.json文件
            //对比哪些文件需要下载

            Action<UnityWebRequest> DownloadCB = (request) =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError(request.error + "  -----  下载服务端上的Version.json失败");
                    cb((int)LocalCode.DOWNLOAD_VERSIONJSON_FAULT);
                }
                else
                {
                    VersionJsonObject serverVersionJson = Helper.LoadVersionJson(request.downloadHandler.text);

                    if (serverVersionJson.version > localVersionJsonObj.version) //服务器版本大于本地版本
                    {
                        //下面检测该下哪些Bundle
                        foreach (ABNameHash singleServiceNameHash in serverVersionJson.ABHashList)
                        {
                            VersionJsonObject aa = localVersionJsonObj;
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
                        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(request.downloadHandler.text);
                        Helper.SaveAssetToLocalFile(PathDefine.presitantABPath(), "Version/version.json",
                            byteArray);
                        localVersionJsonObj = serverVersionJson;
                        GameSetting.Instance.versionCode = (int)serverVersionJson.version;
                    }

                    if (shouldDownloadList.Count > 0)
                    {
                        //拿到对应的fileversion  然后取版本号
                        //取到版本号之后开始从资源库上对应文件夹下载资源
                        string fileVersionPath = PathDefine.serverPath(pfStr, serverVersionCode) + "fileversion.json";

                        Action<UnityWebRequest> DownloadFileVersionCB = (fileVersionRequest) =>
                        {
                            if (fileVersionRequest.isNetworkError || fileVersionRequest.isHttpError)
                            {
                                Debug.LogError(fileVersionRequest.error);
                                cb((int)LocalCode.DOWNLOAD_FILEVERSIONJSON_FAULT);
                            }
                            else
                            {
                                FileVersionJsonObject fileVersionJsonObject = Helper.LoadFileVersionJson(fileVersionRequest.downloadHandler.text);
                                //保存当前这份最新的 fileversion.json 文件
                                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(fileVersionRequest.downloadHandler.text);
                                Helper.SaveAssetToLocalFile(PathDefine.presitantABPath(), "FileVersion/fileversion.json", byteArray);

                                List<VersionAndSize> vasList = new List<VersionAndSize>();
                                foreach (string name in shouldDownloadList)
                                {
                                    Debug.LogError("download AB -----  " + name);
                                    VersionAndSize vas = fileVersionJsonObject.versionSizeList.Find(t => t.name == name);
                                    vasList.Add(vas);
                                }

                                AssetBundleManager.Instance.DownLoadAssetBundleByList(vasList, pfStr, cb);
                            }
                        };

                        UnityWebRequestManager.Instance.DownloadBuffer(fileVersionPath, DownloadFileVersionCB);
                    }
                    else
                    {
                        Debug.LogError("没有可更新的");
                    }
                }
            };

            UnityWebRequestManager.Instance.DownloadBuffer(PathDefine.serverPath(pfStr, serverVersionCode) + "version.json", DownloadCB);
        };

        AsyncHelper asyncHelper = new AsyncHelper();
        asyncHelper.Waterfall(tasks, finalCB);
    }
}

