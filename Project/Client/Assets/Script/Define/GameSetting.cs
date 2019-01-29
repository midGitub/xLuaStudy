using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSetting : ScriptableObject
{
    private static GameSetting _instance;
    public static GameSetting Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load("Setting/GameSetting") as GameSetting;
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    [Header("运行方式")]
    public RunType runType;

    [Header("当前整包版本")]
    public int versionCode;

    [Header("当前帧最大加载AB个数")]
    public int MaxhitThresholdAB;

    [Header("最大加载AB帧时间片")]
    public float frameTimeLimitAB;

    [Header("当前帧最大实例化Object个数")]
    public int MaxhitThresholdObject;

    [Header("最大实例化帧时间片")]
    public float frameTimeLimitObject;
}

