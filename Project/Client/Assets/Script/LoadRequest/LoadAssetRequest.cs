using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LoadAssetRequest : LoadRequest
{
    //是否在加载状态
    private bool isLoading = false;

    public LoadAssetRequest(Action<object> onLoadFinishCallBack)
    {

    }

    public override bool Load(Priority priority, out bool process)
    {
        string path = "UIStartView";
        //Debug.Log("加载UI窗体 ==" + path);
        //GameObject Canvas = GameObject.Find("Canvas");
        path = "Prefab/View/" + path;
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(path)) as GameObject;
        //obj.transform.SetParent(Canvas.transform, false);

        if (isLoading == true)
        {
            process = false;
            return true;
        }
        else
        {
            process = false;
            isLoading = true;
            switch (GameSetting.Instance.runType)
            {
                case RunType.PATCHER_SA_PS:
                    //AssetBundleLoader.LoadLuaInBundle
                    break;
                case RunType.NOPATCHER_SA:
                    break;
                case RunType.NOPATCHER_RES:
                    break;
            }
            
            return true;
        }
    }
}

