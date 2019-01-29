using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleCache
{
    public AssetType assetType;
    public UnityEngine.Object obj;
    public string name;
    public ulong loadFrameCount = 0;
    public List<Action<int, UnityEngine.Object>> onLoadFinishCallBackList = new List<Action<int, UnityEngine.Object>>();
}

