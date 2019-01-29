using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCache
{
    public AssetType assetType;
    public UnityEngine.Object obj;
    public string name;
    public ulong loadFrameCount = 0;
    public List<Action<UnityEngine.Object>> onLoadFinishCallBackList = new List<Action<UnityEngine.Object>>();
}

