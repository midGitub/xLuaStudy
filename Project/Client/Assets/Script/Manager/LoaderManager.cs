using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class LoaderManager : SingletonBehaviour<LoaderManager>
{
    /// <summary>
    /// 等待加载队列
    /// </summary>
    private static LinkedListNode<LoadRequest>[] waitingLoadQueue;
    
    private static List<LoadingCache> cacheList;

    private static Dictionary<AssetType, bool> cacheFlagDict;

    public static ulong curFrameCount = 0;

    private int curHitCount = 0;
    public void Init(Action<int> initedCB)
    {
        //初始化加载队列
        waitingLoadQueue = new LinkedListNode<LoadRequest>[(int)Priority.PRIORITY_COUNT];
        for (int i = 0; i < (int)Priority.PRIORITY_COUNT; i++)
        {
            waitingLoadQueue[i] = new LinkedListNode<LoadRequest>(new LoadRequest());
            var list = new LinkedList<LoadRequest>();
            list.AddFirst(waitingLoadQueue[i]);
        }

        cacheList = new List<LoadingCache>();

        cacheFlagDict = new Dictionary<AssetType, bool>();
        cacheFlagDict[AssetType.UIPREFAB] = true;
        cacheFlagDict[AssetType.TEXTURE] = true;

        if (initedCB != null)
        {
            initedCB.Invoke((int)LocalCode.SUCCESS);
        }
    }

    private void Update()
    {
        curFrameCount++;

        //加载AB
        float nowTime = Time.realtimeSinceStartup;
        curHitCount = 0;
        for (int i = 0; i < (int)Priority.PRIORITY_COUNT; i++)
        {
            var curQueue = waitingLoadQueue[i];

            //只剩下一个空头指针的时候调到下一个优先级
            if (curQueue.List.Count == 1)
            {
                continue;
            }

            //不能超过当前最大加载数 以及 帧时间不能超过最大加载时间
            var curNode = curQueue.Next;
            while (curNode != null && curHitCount <= GameSetting.Instance.MaxhitThresholdAB && Time.realtimeSinceStartup - nowTime <= GameSetting.Instance.frameTimeLimitAB)
            {
                bool rm = false;
                try
                {
                    curNode.Value.Load((Priority)i, out rm);
                    curHitCount++;
                }
                finally
                {
                    if (rm)
                    {
                        curNode = removeQNode(curNode);
                    }
                    else
                    {
                        curNode = curNode.Next;
                    }
                }
            }
        }
    }

    private static LinkedListNode<LoadRequest> removeQNode(LinkedListNode<LoadRequest> node)
    {
        var needRm = node;
        node = node.Next;
        needRm.List.Remove(needRm);
        return node;
    }

    private static LinkedListNode<LoadingCache> removeQNode(LinkedListNode<LoadingCache> node)
    {
        var needRm = node;
        node = node.Next;
        needRm.List.Remove(needRm);
        return node;
    }

    public static void LoadSceneAsync(string sceneName, Action<int> onLoadFinishCB)
    {
        LoadSceneRequest req = new LoadSceneRequest(sceneName, onLoadFinishCB);
        waitingLoadQueue[(int)Priority.SCENE].List.AddLast(req);
    }

    public static void LoadAllLuaSync(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        LoadLuaRequest req = new LoadLuaRequest(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
        waitingLoadQueue[(int)Priority.CODE].List.AddLast(req);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="name"></param>
    /// <param name="assetType"></param>
    /// <param name="onLoadFinishCallBack"></param>
    public static void LoadAssetSync(string name, AssetType assetType, Action<UnityEngine.Object> onLoadFinishCallBack)
    {
        LoadAssetRequest req = new LoadAssetRequest(name, assetType, onLoadFinishCallBack);
        waitingLoadQueue[(int)Priority.ASSET].List.AddLast(req);
    }

    /// <summary>
    /// 加载UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onLoadFinishCallBcak"></param>
    public static void LoadUISync(string name, Action<UnityEngine.Object> onLoadFinishCallBcak)
    {
        LoadAssetSync(name, AssetType.UIPREFAB, onLoadFinishCallBcak);
    }

    public static void LoadTextureSync(string name, Action<UnityEngine.Object> onLoadFinishCallBack)
    {
        LoadAssetSync(name, AssetType.TEXTURE, onLoadFinishCallBack);
    }

    /// <summary>
    /// 读取图集
    /// </summary>
    public static void LoadAllAtlasSync(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, SpriteAtlas>> onLoadAllFinishCallBack)
    {
        LoadSpriteAtlasRequest req = new LoadSpriteAtlasRequest(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
        waitingLoadQueue[(int)Priority.ATLAS].List.AddLast(req);
    }

    public void OnDestroy()
    {
        AssetBundleLoader.abDict.Clear();
    }
}