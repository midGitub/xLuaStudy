using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasManager : SingletonBehaviour<AtlasManager>
{
    private Dictionary<string, SpriteAtlas> atlasDict;

    public void Init(Action<int> onFinishCallBack)
    {
        SpriteAtlasManager.atlasRequested += RequestAtlas;

        atlasDict = new Dictionary<string, SpriteAtlas>();

        Action<int, int> onLoadSingleFinishCallBack = (curCount, allCount) =>
        {
            UILoadingView.Instance.Refresh(curCount, allCount, "加载图集中脚本中");
        };

        Action<Dictionary<string, SpriteAtlas>> onLoadAllFinishCallBack = (allData) =>
        {
            this.atlasDict = allData;
            if (onFinishCallBack != null)
            {
                onFinishCallBack.Invoke((int)LocalCode.SUCCESS);
            }
        };

        LoaderManager.LoadAllAtlasSync(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
    }

    void OnDisable()
    {
        SpriteAtlasManager.atlasRequested -= RequestAtlas;
    }

    void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {
        callback(atlasDict[tag.ToLower()]);
    }
}
