using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    private static AssetBundleManager _instance;
    public static AssetBundleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerGroupObj = GameObject.Find("ManagerGroup");
                DontDestroyOnLoad(managerGroupObj);

                GameObject g = new GameObject();
                g.transform.SetParent(managerGroupObj.transform, false);

                AssetBundleManager loadAssetBundle = g.AddComponent<AssetBundleManager>();
                _instance = loadAssetBundle;
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        this.name = this.GetType().ToString();
    }
}

