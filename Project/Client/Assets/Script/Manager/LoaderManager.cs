using System;
using System.Collections.Generic;
using UnityEngine;

public class LoaderManager : SingletonBehaviour<LoaderManager>
{
    /// <summary>
    /// 等待加载队列
    /// </summary>
    private static LinkedListNode<LoadRequest>[] waitingQueue;

    private int curHitCount = 0;
    public void Init()
    {
        //初始化加载队列
        waitingQueue = new LinkedListNode<LoadRequest>[(int)Priority.PRIORITY_COUNT];
        for (int i = 0; i < (int)Priority.PRIORITY_COUNT; i++)
        {
            waitingQueue[i] = new LinkedListNode<LoadRequest>(new LoadRequest());
            var list = new LinkedList<LoadRequest>();
            list.AddFirst(waitingQueue[i]);
        }
    }

    private void Update()
    {
        curHitCount = 0;
        for (int i = 0; i < (int)Priority.PRIORITY_COUNT; i++)
        {
            var curQueue = waitingQueue[i];

            //只剩下一个空头指针的时候调到下一个优先级
            if (curQueue.List.Count == 1)
            {
                continue;
            }

            bool isOverHit = curHitCount > GameSetting.Instance.MaxhitThreshold;
            bool isOverTime = Time.realtimeSinceStartup - FrameHelper.frameStartTime > GameSetting.Instance.frameTimeLimit;

            //不能超过当前最大加载数 以及 帧时间不能超过最大加载时间
            var curNode = curQueue.Next;
            while (curNode != null && curHitCount <= GameSetting.Instance.MaxhitThreshold && Time.realtimeSinceStartup - FrameHelper.frameStartTime <= GameSetting.Instance.frameTimeLimit)
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

    public static void LoadSceneAsync(string sceneName, Action<int> onLoadFinishCB)
    {
        LoadSceneRequest req = new LoadSceneRequest(sceneName, onLoadFinishCB);
        waitingQueue[(int)Priority.SCENE].List.AddLast(req);
    }

    public static void LoadAllLuaSync(Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        LoadLuaRequest req = new LoadLuaRequest(onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
        waitingQueue[(int)Priority.CODE].List.AddLast(req);
    }

    public static void LoadAssetSync()
    {
        LoadAssetRequest req = new LoadAssetRequest(null);
        waitingQueue[(int)Priority.ASSET].List.AddLast(req);
    }
}