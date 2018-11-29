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
    public bool patcher;
}

