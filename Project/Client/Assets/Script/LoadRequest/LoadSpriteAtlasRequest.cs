using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

public class LoadSpriteAtlasRequest : LoadRequest
{
    private Dictionary<string, SpriteAtlas> allSpriteAtlasDict;

    private Action<int, int> onLoadSingleFinishCallBack;
    private Action<Dictionary<string, SpriteAtlas>> onLoadAllFinishCallBack;

    public LoadSpriteAtlasRequest(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, SpriteAtlas>> onLoadAllFinishCallBack)
    {
        this.onLoadSingleFinishCallBack = onLoadSingleFinishCallBack;
        this.onLoadAllFinishCallBack = onLoadAllFinishCallBack;
    }

    public override bool Load(Priority priority, out bool process)
    {
        allSpriteAtlasDict = new Dictionary<string, SpriteAtlas>();
        int dCount = 0;
        int allCount = 0;

        Action<Dictionary<string, SpriteAtlas>> dataLoadCallBack = (dict) =>
        {
            dCount++;
            foreach (KeyValuePair<string, SpriteAtlas> item in dict)
            {
                allSpriteAtlasDict.Add(item.Key, item.Value);
            }
            if (onLoadSingleFinishCallBack != null)
            {
                onLoadSingleFinishCallBack.Invoke(dCount, allCount);
            }
            if (dCount == allCount)
            {
                if (onLoadAllFinishCallBack != null)
                {
                    onLoadAllFinishCallBack.Invoke(allSpriteAtlasDict);
                }
            }
        };

        switch (GameSetting.Instance.runType)
        {
            case RunType.PATCHER_SA_PS:
                FileVersionJsonObject fileVersion = AssetBundleManager.Instance.fileVersionJsonObject;
                List<VersionAndSize> vasList = fileVersion.versionSizeList.FindAll(t => t.name.Contains("spriteatlas/"));
                allCount = vasList.Count;
                for (int i = 0; i < vasList.Count; i++)
                {
                    string saPath = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" + vasList[i].name; ;
                    string pdPath = PathDefine.presitantABPath() + vasList[i].name;
                    string path = vasList[i].version <= GameSetting.Instance.versionCode ? saPath : pdPath;

                    AssetBundleLoader.LoadSpriteAtlas(path, dataLoadCallBack);
                }
                break;
            case RunType.NOPATCHER_SA:
                Action<VersionJsonObject> loadVersionJsonCallBack = (versionJson) =>
                {
                    List<string> abNameList = new List<string>();
                    List<ABNameHash> abNameHashList = versionJson.ABHashList;
                    for (int i = 0; i < abNameHashList.Count; i++)
                    {
                        if (abNameHashList[i].abName.Contains("spriteatlas/"))
                        {
                            abNameList.Add(abNameHashList[i].abName);
                        }
                    }
                    allCount = abNameList.Count;
                    for (int i = 0; i < abNameList.Count; i++)
                    {
                        string path = PathDefine.StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" + abNameList[i];
                        AssetBundleLoader.LoadSpriteAtlas(path, dataLoadCallBack);
                    }
                };

                LoadLocalVersionJsonObject(loadVersionJsonCallBack);
                break;
            case RunType.NOPATCHER_RES:
                string[] spriteAtlasPath = Helper.GetFiles("Assets/SpriteAtlas/SpriteAtlas/", null, true, true);
                List<string> shouldLoadFileList = new List<string>();
                for (int i = 0; i < spriteAtlasPath.Length; i++)
                {
                    if (!spriteAtlasPath[i].Contains(".meta"))
                    {
                        shouldLoadFileList.Add(spriteAtlasPath[i]);
                    }
                }

                allCount = shouldLoadFileList.Count;
                for (int i = 0; i < shouldLoadFileList.Count; i++)
                {
                    ResourcesLoader.LoadSpriteAtlas(shouldLoadFileList[i], dataLoadCallBack);
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