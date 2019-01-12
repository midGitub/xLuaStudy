package com.xluap.xluac;

import android.os.Bundle;

import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

public class unityproject extends UnityPlayerActivity {


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //setContentView(R.layout.activity_unityproject);
    }

    public int add(int a, int b) {
        return a + b;
    }

    public List<File> GetFile(File file) {
        List<File> mFileList = new ArrayList<File>();

        File[] fileArray = file.listFiles();
        for (File f : fileArray) {
            if (f.isFile()) {
                mFileList.add(f);
            } else {

                for (File f1 : GetFile(f)) {
                    mFileList.add(f1);
                }
            }
        }

        return mFileList;
    }

    public String[] GetPath1(String dir) {
        System.out.println("传入的地址  "+dir);
        File file = new File(dir);
        List<File> fileList = GetFile(file);
        List<String> nameList = new ArrayList<String>();
        for (File f : fileList) {
            nameList.add(f.getName());
        }

        return nameList.toArray(new  String[0]);
    }
}
