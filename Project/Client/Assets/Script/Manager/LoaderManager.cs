using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderManager : SingletonBehaviour<LoaderManager>
{
    /// <summary>
    /// 等待加载队列
    /// </summary>
    private static LinkedListNode<LoadRequest>[] waitingLoadQueue;

    /// <summary>
    /// 等待实例化队列
    /// </summary>
    private static List<AssetBundleCache> waitingInsList;

    private static List<AssetBundleCache> cacheList;

    private static Dictionary<AssetType, bool> cacheFlagDict;

    public static ulong curFrameCount = 0;

    private int curHitCount = 0;
    public void Init()
    {
        //初始化加载队列
        waitingLoadQueue = new LinkedListNode<LoadRequest>[(int)Priority.PRIORITY_COUNT];
        for (int i = 0; i < (int)Priority.PRIORITY_COUNT; i++)
        {
            waitingLoadQueue[i] = new LinkedListNode<LoadRequest>(new LoadRequest());
            var list = new LinkedList<LoadRequest>();
            list.AddFirst(waitingLoadQueue[i]);
        }

        waitingInsList = new List<AssetBundleCache>();

        cacheList = new List<AssetBundleCache>();

        cacheFlagDict = new Dictionary<AssetType, bool>();
        cacheFlagDict[AssetType.UIPREFAB] = true;
        cacheFlagDict[AssetType.Texture] = true;
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
            //while (curNode != null)
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
        List<AssetBundleCache> deleteABCList = new List<AssetBundleCache>();
        for (int i = 0; i < waitingInsList.Count; i++)
        {
            if (curHitCount < GameSetting.Instance.MaxhitThresholdObject && Time.realtimeSinceStartup - nowTime <= GameSetting.Instance.frameTimeLimitObject)
            {
                AssetBundleCache curABC = waitingInsList[i];
                List<Action<int, UnityEngine.Object>> deleteActionList = new List<Action<int, UnityEngine.Object>>();
                for (int j = 0; j < curABC.onLoadFinishCallBackList.Count; j++)
                {
                    if (curHitCount < GameSetting.Instance.MaxhitThresholdObject && Time.realtimeSinceStartup - nowTime <= GameSetting.Instance.frameTimeLimitObject)
                    {
                        curHitCount++;
                        curABC.onLoadFinishCallBackList[j].Invoke((int)LocalCode.SUCCESS, curABC.obj);
                        deleteActionList.Add(curABC.onLoadFinishCallBackList[j]);
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

    private static LinkedListNode<AssetBundleCache> removeQNode(LinkedListNode<AssetBundleCache> node)
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

    public static void LoadAssetSync(string name, AssetType assetType, Action<int, UnityEngine.Object> onLoadFinishCallBack)
    {
        AssetBundleCache cache = null;

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
            cache = new AssetBundleCache();
            cache.name = name;
            cache.assetType = assetType;
            cache.onLoadFinishCallBackList.Add(onLoadFinishCallBack);
            cacheList.Add(cache);
        }

        Action<int, UnityEngine.Object> loadCallBack = (code, abObject) =>
        {
            if (cacheFlagDict[assetType])
            {
                cache.obj = abObject;
                //Debug.LogError("cache.onLoadFinishCallBackList.Count  " + cache.onLoadFinishCallBackList.Count);
                //for (int i = 0; i < cache.onLoadFinishCallBackList.Count; i++)
                //{

                //}
                //CoroutineManager.Instance.StartCoroutine(InvokeByCoroutine(cache.onLoadFinishCallBackList, abObject));

                waitingInsList.Add(cache);
            }
            else
            {
                onLoadFinishCallBack.Invoke(code, abObject);
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
                onLoadFinishCallBack.Invoke((int)LocalCode.SUCCESS, cache.obj);
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


    private static IEnumerator InvokeByCoroutine(List<Action<int, UnityEngine.Object>> actionList, UnityEngine.Object obje)
    {
        var cur = actionList.GetEnumerator();
        cur.MoveNext();
        while (cur.Current != null)
        {
            cur.Current.Invoke((int)LocalCode.SUCCESS, obje);
            cur.MoveNext();
            yield return new WaitForSeconds(0.1f);
        }
    }
}