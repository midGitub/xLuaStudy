using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

public class LoadSceneRequest : LoadRequest
{
    public Action<int> onLoadFinishCallBack;

    private string sceneName;
    private bool cacheFlag;
    public LoadSceneRequest(string sceneName, Action<int> onLoadFinishCallBack)
    {
        this.sceneName = sceneName;
        this.onLoadFinishCallBack = onLoadFinishCallBack;
    }

    public override bool Load(Priority priority, out bool process)
    {
        switch (GameSetting.Instance.runType)
        {
            case RunType.PATCHER_SA_PS:
                AssetBundleLoader.LoadSceneAsync(sceneName, onLoadFinishCallBack);
                break;
            case RunType.NOPATCHER_SA:
                AssetBundleLoader.LoadSceneAsync(sceneName, onLoadFinishCallBack);
                break;
            case RunType.NOPATCHER_RES:
                ResourcesLoader.LoadScene(sceneName, onLoadFinishCallBack);
                break;
        }
        process = true;
        return true;
    }
}

