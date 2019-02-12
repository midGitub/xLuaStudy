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

    /// <summary>
    /// 等待实例化队列
    /// </summary>
    private static List<LoadingCache> waitingInsList;

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

        waitingInsList = new List<LoadingCache>();

        cacheList = new List<LoadingCache>();

        cacheFlagDict = new Dictionary<AssetType, bool>();
        cacheFlagDict[AssetType.UIPREFAB] = true;
        cacheFlagDict[AssetType.Texture] = true;

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

        //实例化Object
        nowTime = Time.realtimeSinceStartup;
        curHitCount = 0;
        List<LoadingCache> deleteABCList = new List<LoadingCache>();
        for (int i = 0; i < waitingInsList.Count; i++)
        {
            if (curHitCount < GameSetting.Instance.MaxhitThresholdObject && Time.realtimeSinceStartup - nowTime <= GameSetting.Instance.frameTimeLimitObject)
            {
                LoadingCache curABC = waitingInsList[i];
                List<Action<UnityEngine.Object>> deleteActionList = new List<Action<UnityEngine.Object>>();
                for (int j = 0; j < curABC.onLoadFinishCallBackList.Count; j++)
                {
                    if (curHitCount < GameSetting.Instance.MaxhitThresholdObject && Time.realtimeSinceStartup - nowTime <= GameSetting.Instance.frameTimeLimitObject)
                    {
                        curHitCount++;
                        try
                        {
                            curABC.onLoadFinishCallBackList[j].Invoke(curABC.obj);
                            deleteActionList.Add(curABC.onLoadFinishCallBackList[j]);
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = 0; j < deleteActionList.Count; j++)
                {
                    curABC.onLoadFinishCallBackList.Remove(deleteActionList[j]);
                }

                if (curABC.onLoadFinishCallBackList.Count == 0)
                {
                    deleteABCList.Add(curABC);
                }
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i < deleteABCList.Count; i++)
        {
            waitingInsList.Remove(deleteABCList[i]);
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

    public static void LoadAssetSync(string name, AssetType assetType, Action<UnityEngine.Object> onLoadFinishCallBack)
    {
        LoadingCache cache = null;

        for (int i = 0; i < cacheList.Count; i++)
        {
            if (cacheList[i].assetType == assetType && cacheList[i].name == name)
            {
                cache = cacheList[i];
                break;
            }
        }

        if (cacheFlagDict[assetType] && cache == null)
        {
            cache = new LoadingCache();
            cache.name = name;
            cache.assetType = assetType;
            cache.onLoadFinishCallBackList.Add(onLoadFinishCallBack);
            cacheList.Add(cache);
        }

        Action<UnityEngine.Object> loadCallBack = (abObject) =>
        {
            if (cacheFlagDict[assetType])
            {
                cache.obj = abObject;
                waitingInsList.Add(cache);
            }
            else
            {
                onLoadFinishCallBack.Invoke(abObject);
            }
        };

        if (cache != null)
        {
            if (cache.obj == null)
            {
                if (cache.loadFrameCount == 0)//同帧第一次加载
                {
                    cache.loadFrameCount = curFrameCount;
                    LoadAssetRequest req = new LoadAssetRequest(name, assetType, loadCallBack);
                    waitingLoadQueue[(int)Priority.ASSET].List.AddLast(req);
                }
                else if (cache.loadFrameCount == curFrameCount)
                {
                    if (!cache.onLoadFinishCallBackList.Contains(onLoadFinishCallBack))
                    {
                        cache.onLoadFinishCallBackList.Add(onLoadFinishCallBack);
                    }
                }
                else
                {
                    LoadAssetRequest req = new LoadAssetRequest(name, assetType, loadCallBack);
                    waitingLoadQueue[(int)Priority.ASSET].List.AddLast(req);
                }
            }
            else
            {
                onLoadFinishCallBack.Invoke(cache.obj);
            }
        }
        else
        {
            if (cache == null || cache.obj == null)
            {
                LoadAssetRequest req = new LoadAssetRequest(name, assetType, loadCallBack);
                waitingLoadQueue[(int)Priority.ASSET].List.AddLast(req);
            }
        }
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
        LoadAssetSync(name, AssetType.Texture, onLoadFinishCallBack);
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
        AssetBundleLoader.abList.Clear();
    }
}