using System;
using System.Collections.Generic;
using UnityEngine;

public class LoaderManager : SingletonBehaviour<LoaderManager>
{
    /// <summary>
    /// 等待加载队列
    /// </summary>
    private static LinkedListNode<LoadRequest>[] waitingQueue;

    /// <summary>
    /// 帧时间片控制
    /// </summary>
    public static float frameTimeLimit { get; set; }

    private int curHitCount = 0;
    public void Init()
    {
        //初始化加载队列
        waitingQueue = new LinkedListNode<LoadRequest>[(int)AssetPriority.PRIORITY_COUNT];
        for (int i = 0; i < (int)AssetPriority.PRIORITY_COUNT; i++)
        {
            waitingQueue[i] = new LinkedListNode<LoadRequest>(new LoadRequest());
            var list = new LinkedList<LoadRequest>();
            list.AddFirst(waitingQueue[i]);
        }
        frameTimeLimit = 0.5f;
    }

    private void Update()
    {
        curHitCount = 0;
        for (int i = 0; i < (int)AssetPriority.PRIORITY_COUNT; i++)
        {
            var curQueue = waitingQueue[i];

            //只剩下一个空头指针的时候调到下一个优先级
            if (curQueue.List.Count == 1)
            {
                continue;
            }

            bool isOverHit = curHitCount > GameSetting.Instance.MaxhitThreshold;
            bool isOverTime = Time.realtimeSinceStartup - FrameHelper.frameStartTime > frameTimeLimit;

            //不能超过当前最大加载数 以及 帧时间不能超过最大加载时间
            var curNode = curQueue.Next;
            while (curNode != null && !isOverHit && !isOverTime)
            {
                bool rm = false;
                try
                {
                    curNode.Value.Load((AssetPriority)i, out rm);
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

    public static void LoadSceneAsync(string sceneName, DataFrom from, Action<int> onLoadFinishCB)
    {
        LoadSceneRequest req = new LoadSceneRequest(sceneName, from, onLoadFinishCB);
        waitingQueue[(int)AssetPriority.SCENE].List.AddLast(req);
    }

    public static void LoadAllLuaSync(DataFrom from, Action<int, int> onLoadSingleFinishCallBack, Action<Dictionary<string, byte[]>> onLoadAllFinishCallBack)
    {
        LoadLuaRequest req = new LoadLuaRequest(from, onLoadSingleFinishCallBack, onLoadAllFinishCallBack);
        waitingQueue[(int)AssetPriority.CODE].List.AddLast(req);
    }
}