using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LoadLuaRequest : LoadRequest
{
    private Dictionary<string, byte[]> allLuaDict = new Dictionary<string, byte[]>();

    private Action<int, int> onLoadSingleFinishCallBack;
    private Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack;

    public LoadLuaRequest(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        this.timeout = float.MaxValue;
        this.onLoadSingleFinishCallBack = onLoadSingleFinishCallBack;
        this.onLoadAllFinishCallBack = onLoadAllFinishCallBack;
    }

    public override bool Load(Priority priority, out bool process)
    {
        allLuaDict = new Dictionary<string, byte[]>();
        int dCount = 0;
        int allCount = 0;
        Action<Dictionary<string, byte[]>> dataLoadCallBack = (dict) =>
        {
            dCount++;
            foreach (KeyValuePair<string, byte[]> item in dict)
            {
                allLuaDict.Add(item.Key, item.Value);
            }
            if (onLoadSingleFinishCallBack != null)
            {
                onLoadSingleFinishCallBack.Invoke(dCount, allCount);
            }
            if (dCount == allCount)
            {
                if (onLoadAllFinishCallBack != null)
                {
                    onLoadAllFinishCallBack.Invoke(allLuaDict);
                }
            }
        };

        switch (GameSetting.Instance.runType)
        {
            case RunType.PATCHER_SA_PS:
                FileVersionJsonObject fileversion = AssetBundleManager.Instance.fileVersionJsonObject;
                List<VersionAndSize> vasList = fileversion.versionSizeList.FindAll(t => t.name.Contains("lua/"));
                allCount = vasList.Count;
                for (int i = 0; i < vasList.Count; i++)
                {
                    string saPath = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" + vasList[i].name; ;
                    string pdPath = PathDefine.presitantABPath() + vasList[i].name;
                    string path = vasList[i].version <= GameSetting.Instance.versionCode ? saPath : pdPath;

                    AssetBundleLoader.LoadLuaData(path, dataLoadCallBack);
                }
                break;
            case RunType.NOPATCHER_SA:
                Action<VersionJsonObject> loadVersionJsonCallBack = (versionJson) =>
                {
                    List<string> abNameList = new List<string>();
                    List<ABNameHash> abNameHashList = versionJson.ABHashList;
                    for (int i = 0; i < abNameHashList.Count; i++)
                    {
                        if (abNameHashList[i].abName.Contains("lua/"))
                        {
                            abNameList.Add(abNameHashList[i].abName);
                        }
                    }
                    allCount = abNameList.Count;
                    for (int i = 0; i < abNameList.Count; i++)
                    {
                        string path = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" + abNameList[i];
                        AssetBundleLoader.LoadLuaData(path, dataLoadCallBack);
                    }
                };

                LoadLocalVersionJsonObject(loadVersionJsonCallBack);
                break;
            case RunType.NOPATCHER_RES:
                string[] luaPath = Helper.GetFiles("Assets/Lua/", null, true, true);
                List<string> shouldLoadFileList = new List<string>();
                for (int i = 0; i < luaPath.Length; i++)
                {
                    if (!luaPath[i].Contains(".meta"))
                    {
                        shouldLoadFileList.Add(luaPath[i]);
                    }
                }

                allCount = shouldLoadFileList.Count;
                for (int i = 0; i < shouldLoadFileList.Count; i++)
                {
                    ResourcesLoader.LoadLuaData(shouldLoadFileList[i], dataLoadCallBack);
                }

                break;
        }
        process = true;
        return true;
    }

    private void LoadLocalVersionJsonObject(Action<VersionJsonObject> onLoadVersionCallBack)
    {
        string versionPath = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "Version/version.json";

        Action<UnityWebRequest> DownloadCB = (request) =>
        {
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                if (onLoadVersionCallBack != null)
                {
                    onLoadVersionCallBack.Invoke(Helper.LoadVersionJson(request.downloadHandler.text));
                }
            }
        };

        UnityWebRequestManager.Instance.DownloadBuffer(versionPath, DownloadCB);
    }
}

