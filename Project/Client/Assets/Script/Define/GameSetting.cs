using System;
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

    [Header("是否开启热更新")]
    public bool patcher;

    [Header("当前整包版本")]
    public int versionCode;

    [Header("是否强制开启SA到PD的复制")]
    public bool forceCopy;

    [Header("资源来源")]
    public DataFrom from;

    [Header("当前帧最大加载个数")]
    public int MaxhitThreshold;
}

