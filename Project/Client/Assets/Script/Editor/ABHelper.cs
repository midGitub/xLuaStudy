using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ABHelper
{
    [MenuItem("ABHelper/SetBundleName", false, 0)]
    public static void SetBundleNameAll()
    {
        SetViewPrefabName();
    }

    [MenuItem("ABHelper/BuildAssetBundle", false, 1)]
    public static void BuildAssetBundle()
    {
        string dir = "AssetBundles";
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

    }


    #region 设置AssetBundleName
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
    #endregion

    #region Build各种资源的AssetBundle
    [MenuItem("ABHelper/BuildLuaAssetBundle", false, 1)]
    public static void BuildLuaAssetBundle()
    {
        BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle;

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

        AssetDatabase.Refresh();

        //use list to collect files whith *.bytes format 
        files = Directory.GetFiles("Assets/Lua/Out", "*.lua.bytes");
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        for (int i = 0; i < files.Length; i++)
        {
            UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(files[i]);
            AssetBundleBuild item = new AssetBundleBuild();
            item.assetBundleName = "lua";
            item.assetNames = new string[] { AssetDatabase.GetAssetPath(obj) };
            item.assetBundleVariant = "bytes";
            builds.Add(item);
        }

        ///buildpipeline with list 
        if (files.Length > 0)
        {
            string outPath = "AssetBundles/lua/";
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outPath, builds.ToArray(), options, BuildTarget.StandaloneWindows64);
            AssetDatabase.Refresh();

            if (manifest != null)
            {
                Debug.Log("Lua脚本打包成功");
            }
            else
            {
                Debug.Log("Lua脚本打包失败");
            }
        }
        else
        {
            Debug.Log("读取不到任何Lua文件");
        }
    }
    #endregion
}