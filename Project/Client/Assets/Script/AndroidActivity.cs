using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AndroidActivity
{
    private AndroidJavaClass _jc = null;
    public AndroidJavaClass jc
    {
        get
        {
            if (_jc == null)
            {
                _jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            }

            return _jc;
        }
    }

    private AndroidJavaObject _jo = null;

    public AndroidJavaObject jo
    {
        get
        {
            if (_jo == null)
            {
                _jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
            return _jo;
        }
    }

    public int add(int a, int b)
    {
        int n = jo.Call<int>("add", a, b);
        return n;
    }
}

