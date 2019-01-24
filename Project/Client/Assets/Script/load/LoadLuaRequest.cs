using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class LoadLuaRequest : LoadRequest
{
    private Action<int, int> onLoadSingleFinishCallBack;
    private Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack;
    private DataFrom from;

    public LoadLuaRequest(DataFrom from, Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        this.timeout = float.MaxValue;
        this.from = from;
        this.onLoadSingleFinishCallBack = onLoadSingleFinishCallBack;
        this.onLoadAllFinishCallBack = onLoadAllFinishCallBack;
    }

    public override bool Load(AssetPriority priority, out bool process)
    {
        if (DataFrom.BUNDLE == from)
        {
            AssetBundleLoader.LoadLuaInBundle(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
        }
        else if (DataFrom.RESOURCES == from)
        {
            AssetBundleLoader.LoadLuaInResources(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
        }
        process = true;
        return true;
    }
}

