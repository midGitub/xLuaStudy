using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public delegate void dlg_OnAssetBundleDownLoadOver();
/// <summary>
/// 加载AssetBundle
/// </summary>
public class AssetBundleManager : MonoBehaviour
{
    private UILoadingView uiLoadingView;

    private static AssetBundleManager instance;

    public static AssetBundleManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerGroup = GameObject.Find("ManagerGroup");
                if (managerGroup == null)
                {
                    managerGroup = new GameObject();
                    managerGroup.name = "ManagerGroup";
                }

                instance = managerGroup.GetComponentInChildren<AssetBundleManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = managerGroup.transform;
                    go.name = typeof(AssetBundleManager).Name;
                    instance = go.AddComponent<AssetBundleManager>();
                }
            }
            return instance;
        }
    }

    public void Awake()
    {
        GameObject canvas = GameObject.Find("Canvas");
        uiLoadingView = canvas.transform.Find("UILoadingView").GetComponent<UILoadingView>();
    }

    public IEnumerator DownLoadAssetsCoroutine(List<VersionAndSize> list, string pfStr, Action<int> cb)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string path = PathDefine.serverPath(pfStr, (int)list[i].version) + list[i].name;

            WWW wwwAsset = new WWW(path);

            yield return wwwAsset;

            if (string.IsNullOrEmpty(wwwAsset.error))
            {
                uiLoadingView.Refresh(i + 1, list.Count, "正在下载对应AB文件至本地");

                //保存到本地
                Helper.SaveAssetToLocalFile(PathDefine.presitantABPath(pfStr) + "AssetsBundle/", list[i].name, wwwAsset.bytes);
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                Debug.LogError(wwwAsset.error);
            }
        }

        if (cb != null)
        {
            cb((int)LocalCode.SUCCESS);
        }
    }

    public void DownLoadAssetBundleByList(List<VersionAndSize> list, string pfStr, Action<int> cb)
    {
        StartCoroutine(DownLoadAssetsCoroutine(list, pfStr, cb));
    }
}
