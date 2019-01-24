using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class LoadLuaRequest : LoadRequest
{
    private Action<int, int> onLoadSingleFinishCallBack;
    private Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack;

    public LoadLuaRequest(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        this.timeout = float.MaxValue;
        this.onLoadSingleFinishCallBack = onLoadSingleFinishCallBack;
        this.onLoadAllFinishCallBack = onLoadAllFinishCallBack;
    }

    public override bool Load(AssetPriority priority, out bool process)
    {
        switch (GameSetting.Instance.runType)
        {
            case RunType.PATCHER_SA_PS:
                AssetBundleLoader.LoadLuaInBundle(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
                break;
            case RunType.NOPATCHER_SA:
                AssetBundleLoader.LoadLuaInBundle(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
                break;
            case RunType.NOPATCHER_RES:
                AssetBundleLoader.LoadLuaInResources(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
                break;
        }
        process = true;
        return true;
    }
}

