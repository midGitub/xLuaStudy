using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LoadAssetRequest : LoadRequest
{
    //是否在加载状态
    private bool process = false;
    private LoadStatus loadStatus = LoadStatus.UNLOAD;
    
    private AssetType assetType = new AssetType();
    private Action<UnityEngine.Object> onLoadFinishCallBack;
    public LoadAssetRequest(string name, AssetType assetType, Action<UnityEngine.Object> onLoadFinishCallBack)
    {
        this.name = name;
        this.assetType = assetType;
        this.onLoadFinishCallBack = onLoadFinishCallBack;
    }

    public override bool Load(Priority priority, out bool process)
    {
        process = this.process;
        if (loadStatus == LoadStatus.UNLOAD)
        {
            process = this.process;

            Action<UnityEngine.Object> onLoadCallBack = (abObject) =>
           {
               this.loadStatus = LoadStatus.LOADEND;
               this.process = true;
               onLoadFinishCallBack.Invoke(abObject);
           };

            switch (GameSetting.Instance.runType)
            {
                case RunType.PATCHER_SA_PS:
                    AssetBundleLoader.LoadAsset(name, assetType, onLoadCallBack);
                    break;
                case RunType.NOPATCHER_SA:
                    AssetBundleLoader.LoadAsset(name, assetType, onLoadCallBack);
                    break;
                case RunType.NOPATCHER_RES:
                    ResourcesLoader.LoadAsset(name, assetType, onLoadCallBack);
                    break;
            }

            loadStatus = LoadStatus.LOADING;
        }
        else if (loadStatus == LoadStatus.LOADING)
        {
            process = this.process;
        }
        else if (loadStatus == LoadStatus.LOADEND)
        {
            process = this.process;
        }

        return true;
    }
}

