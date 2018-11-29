using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ABHelper
{
    private static string localPath = "AssetBundle/";
    private static string servicePathEditor = "D:/LocalServer/AssetBundleEditor/";
    private static string servicePathAndroid = "D:/LocalServer/AssetBundleAndroid/";
    [MenuItem("ABHelper/BuildAssetBundle", false, 0)]
    public static void BuildAssetBundleEditor()
    {
        SetBundleNameAll();

        string dir = CheckPathExistence(servicePathEditor);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(dir, options, BuildTarget.StandaloneWindows64);
    }

    [MenuItem("ABHelper/BuildAssetBundleLocalAndroid", false, 1)]
    public static void BuildAssetBundleLocalAndroid()
    {
        SetBundleNameAll();

        string dir = CheckPathExistence(servicePathAndroid);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(dir, options, BuildTarget.Android);


    }

    #region 设置AssetBundleName
    public static void SetBundleNameAll()
    {
        SetViewPrefabName();
        SetLuaBundleName();
    }

    public static void SetViewPrefabName()
    {
        //Prefab
        string prefabPath = "Assets/Resources/Prefab/View";
        string[] paths = Directory.GetFiles(prefabPath, "*.prefab", SearchOption.TopDirectoryOnly)
             .Where(s => s.EndsWith(".prefab") || s.EndsWith(".bytes") || s.EndsWith(".txt"))
              .ToArray();
        for (int i = 0; i < paths.Length; i++)
        {
            AssetImporter importer = AssetImporter.GetAtPath(paths[i]);
            if (importer != null)
            {
                string bundleName = null;
                if (paths[i].EndsWith(".prefab"))
                {
                    bundleName = paths[i].Replace('\\', '/').Remove(0, "Assets/Resources/Prefab/".Length) + ".unity3d";
                }
                else if (paths[i].EndsWith(".bytes"))
                {
                    string tmp = paths[i].Replace('\\', '/').Remove(0, "Assets/Resources/Prefab/".Length);
                    tmp = tmp.Remove(tmp.LastIndexOf(".", StringComparison.Ordinal));
                    bundleName = tmp + ".unity3d";
                }
                else if (paths[i].EndsWith(".txt"))
                {
                    bundleName = null;
                }
                if (importer.assetBundleName != bundleName)
                {
                    importer.assetBundleName = bundleName;
                }
            }
            else
            {
                Debug.LogError("Prefab {0} load importer failed:" + paths[i]);
                return;
            }
            if (EditorUtility.DisplayCancelableProgressBar("SetBundleName", "PrefabBundleName --> Prefab", (float)i / paths.Length))
            {
                EditorUtility.ClearProgressBar();
                return;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    public static void SetLuaBundleName()
    {
        /// 清零掉这个 目录下的 * .bytes 文件 方便重新生成
        string[] files = Directory.GetFiles("Assets/Lua/Out", "*.lua.bytes");

        for (int i = 0; i < files.Length; i++)
        {
            FileUtil.DeleteFileOrDirectory(files[i]);
        }

        /// convert files whth *.lua format to .bytes  format
        /// then move to Application.dataPath + "/Lua/Out/"
        files = Directory.GetFiles("Assets/Lua/", "*.lua", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files.Length; i++)
        {
            string fname = Path.GetFileName(files[i]);
            FileUtil.CopyFileOrDirectory(files[i], "Assets/Lua/Out/" + fname + ".bytes");
        }

        //use list to collect files whith *.bytes format 
        files = Directory.GetFiles("Assets/Lua/Out", "*.lua.bytes");

        for (int i = 0; i < files.Length; i++)
        {
            AssetImporter importer = AssetImporter.GetAtPath(files[i]);
            if (importer != null)
            {
                importer.assetBundleName = "lua/lua";
            }

            if (EditorUtility.DisplayCancelableProgressBar("SetBundleName", "LuaBundleName --> Lua", (float)i / files.Length))
            {
                EditorUtility.ClearProgressBar();
                return;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    #endregion
    public static string CheckPathExistence(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        AssetDatabase.Refresh();
        return path;
    }
}