using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class LoadSceneRequest : LoadRequest
{
    private string sceneName;
    private DataFrom from;
    private bool cacheFlag;
    public LoadSceneRequest(string sceneName, DataFrom from, Action<int> onLoadFinishCallBack)
    {
        this.sceneName = sceneName;
        this.from = from;
    }

    public override bool Load(AssetPriority priority)
    {
        if (DataFrom.STREAMINGASSETS == from || DataFrom.PERSISTENTDATAPATH == from)
        {
            AssetBundleLoader.LoadSceneAsync(sceneName);
        }
        else if (DataFrom.RESOURCES == from)
        {

        }
        return true;
    }
}

