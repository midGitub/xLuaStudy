using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 记录帧开始时间等一系列帧处理相关问题，可用于帧平滑等处理
/// </summary>
public static class FrameHelper
{
    static bool inited = init();
    static int frameCount = Time.frameCount;

    static bool init()
    {
        frameStartTime = Time.realtimeSinceStartup;
        MonoManager.Instance.fixedUpdate += FixedUpdate;
        MonoManager.Instance.update += Update;
        return true;
    }

    /// <summary>
    /// 逻辑帧开始时间（非精确，仅供算法参考估算）
    /// </summary>
    public static float frameStartTime { get; private set; }

    private static void Update(MonoManager m)
    {
        refreshFrameStart();
    }

    private static void FixedUpdate(MonoManager m)
    {
        refreshFrameStart();
    }

    private static void refreshFrameStart()
    {
        if (frameCount != Time.frameCount)
        {
            frameStartTime = Time.realtimeSinceStartup;
            frameCount = Time.frameCount;
        }
    }
}


