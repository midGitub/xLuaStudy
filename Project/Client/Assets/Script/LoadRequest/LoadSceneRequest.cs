using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

public class LoadSceneRequest : LoadRequest
{
    public Action<int> onLoadFinishCallback;

    private string sceneName;
    private bool cacheFlag;
    public LoadSceneRequest(string sceneName, Action<int> onLoadFinishCallback)
    {
        this.timeout = float.MaxValue;
        this.sceneName = sceneName;
        this.onLoadFinishCallback = onLoadFinishCallback;
    }

    public override bool Load(Priority priority, out bool process)
    {
        switch (GameSetting.Instance.runType)
        {
            case RunType.PATCHER_SA_PS:
                AssetBundleLoader.LoadSceneAsync(sceneName, onLoadFinishCallback);
                break;
            case RunType.NOPATCHER_SA:
                AssetBundleLoader.LoadSceneAsync(sceneName, onLoadFinishCallback);
                break;
            case RunType.NOPATCHER_RES:
                SceneManager.LoadScene("Game");
                break;
        }
        process = true;
        return true;
    }
}

