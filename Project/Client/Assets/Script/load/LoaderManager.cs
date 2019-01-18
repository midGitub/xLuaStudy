using System;
using System.Collections.Generic;
using UnityEngine;

public class LoaderManager : MonoBehaviour
{
    #region Instance
    private static LoaderManager instance;

    public static LoaderManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerGroup = Helper.GetManagerGroup();

                instance = managerGroup.GetComponentInChildren<LoaderManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = managerGroup.transform;
                    go.name = typeof(LoaderManager).Name;
                    instance = go.AddComponent<LoaderManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    /// <summary>
    /// 等待加载队列
    /// </summary>
    private static LinkedListNode<LoadRequest>[] waitingQueue;

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

            var curNode = curQueue.Next;
            while (curNode != null && curHitCount <= GameSetting.Instance.MaxhitThreshold)
            {
                try
                {
                    curNode.Value.Load((AssetPriority) i);
                }
                finally
                {
                    curNode = removeQNode(curNode);
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
}