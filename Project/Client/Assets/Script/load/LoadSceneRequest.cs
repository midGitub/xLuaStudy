using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class LoadSceneRequest : LoadRequest
{
    public Action<int> onLoadFinishCallback;

    private string sceneName;
    private DataFrom from;
    private bool cacheFlag;
    public LoadSceneRequest(string sceneName, DataFrom from, Action<int> onLoadFinishCallback)
    {
        this.timeout = float.MaxValue;
        this.sceneName = sceneName;
        this.from = from;
        this.onLoadFinishCallback = onLoadFinishCallback;
    }

    public override bool Load(AssetPriority priority, out bool process)
    {
        if (DataFrom.BUNDLE == from)
        {
            AssetBundleLoader.LoadSceneAsync(sceneName, onLoadFinishCallback);
        }
        else if (DataFrom.RESOURCES == from)
        {

        }
        process = true;
        return true;
    }
}

