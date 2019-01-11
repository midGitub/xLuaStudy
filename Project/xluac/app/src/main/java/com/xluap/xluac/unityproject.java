package com.xluap.xluac;

import android.os.Bundle;
import  com.unity3d.player.UnityPlayerActivity;

public class unityproject extends UnityPlayerActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //setContentView(R.layout.activity_unityproject);
    }

    public int add(int a,int b)
    {
        return a+b;
    }
}
