using System;
using System.Collections;
using UnityEngine;

public class NetWorkManager : MonoBehaviour
{
    private static NetWorkManager instance;

    public static NetWorkManager Instance
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

                instance = managerGroup.GetComponentInChildren<NetWorkManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = managerGroup.transform;
                    go.name = typeof(NetWorkManager).Name;
                    instance = go.AddComponent<NetWorkManager>();
                }
            }
            return instance;
        }
    }

    public void PostWebMSG(string url, WWWForm form, Action<WWW> callBack)
    {
        StartCoroutine(postWebMsg(url, form, callBack));
    }

    private IEnumerator postWebMsg(string url, WWWForm form, Action<WWW> callBack)
    {
        if (form != null)
        {
            using (WWW www = new WWW(url, form))
            {
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    if (callBack != null)
                    {
                        callBack.Invoke(www);
                    }
                }
                else
                {
                    Debug.LogError(url + "----->Error");
                }
            }
        }
    }

    public void Download(string url, Action<WWW> callBack)
    {
        StartCoroutine(postWebMsg(url, callBack));
    }

    private IEnumerator postWebMsg(string url, Action<WWW> callBack)
    {
        using (WWW www = new WWW(url))
        {
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                if (callBack != null)
                {
                    callBack.Invoke(www);
                }
            }
            else
            {
                Debug.LogError(url + "----->Error");
            }
        }
    }
}

