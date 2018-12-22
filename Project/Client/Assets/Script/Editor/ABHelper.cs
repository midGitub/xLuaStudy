using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LitJson;

public static class ABHelper
{
    [MenuItem("ABHelper/BuildAssetBundleEditor", false, 0)]
    public static void BuildAssetBundleEditor()
    {
        SetBundleNameAll();
        string dir = Helper.CheckPathExistence(PathDefine.buildABPathEditor);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle;
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(dir, options, BuildTarget.StandaloneWindows64);

        string[] allAssetBundlesName = manifest.GetAllAssetBundles();

        JsonData allJsonData = new JsonData() { };

        allJsonData["VersionCode"] = GameSetting.Instance.versionCode;
        allJsonData["ABHashList"] = new JsonData();
        for (int i = 0; i < allAssetBundlesName.Length; i++)
        {
            Hash128 hash = manifest.GetAssetBundleHash(allAssetBundlesName[i]);
            int hashCode = hash.GetHashCode();

            JsonData curABData = new JsonData();
            curABData[allAssetBundlesName[i]] = hashCode;

            allJsonData["ABHashList"].Add(curABData);
        }

        string json = Helper.JsonTree(allJsonData.ToJson());
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(json.ToString());

        //存一份version
        string jsonSavePathLocal = PathDefine.buildVersionPathEditor;
        FileInfo fileInfoLocal = new FileInfo(jsonSavePathLocal);
        Helper.SaveAssetToLocalFile(Helper.CheckPathExistence(fileInfoLocal.Directory.FullName), fileInfoLocal.Name, byteArray);
    }

    [MenuItem("ABHelper/BuildAssetBundleLocalAndroid", false, 1)]
    public static void BuildAssetBundleLocalAndroid()
    {
        SetBundleNameAll();

        string dir = Helper.CheckPathExistence(PathDefine.buildABPathAndroid);
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle;
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
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    public static void SetLuaBundleName()
    {
        string outputByteRootPath = Helper.CheckPathExistence("Assets/LuaByte/");
        string originLuaRootPath = Helper.CheckPathExistence("Assets/Lua/");
        string[] allOutputByteFilePaths = Helper.GetFiles(outputByteRootPath, null, true);

        for (int i = 0; i < allOutputByteFilePaths.Length; i++)
        {
            allOutputByteFilePaths[i] = allOutputByteFilePaths[i].Replace('\\', '/');
            if (allOutputByteFilePaths[i].Contains("meta"))//meta文件作删除处理
            {
                FileUtil.DeleteFileOrDirectory(allOutputByteFilePaths[i]);
            }
            else
            {
                string[] a = allOutputByteFilePaths[i].Split('/');
                string fileNameSuffix = a[a.Length - 1];

                string[] b = fileNameSuffix.Split('.');
                string fileName = b[0];

                //判断原LUA目录是否包含打包输出目录的文件
                //如包含 则进一步比较其文件内容
                //如不包含 则删除
                string originLuaPath = allOutputByteFilePaths[i].Replace("LuaByte", "Lua").Replace(".bytes", string.Empty);

                if (File.Exists(originLuaPath))
                {
                    //如果文本对比不通过 则删除
                    if (!Helper.CompareFile(originLuaPath, allOutputByteFilePaths[i]) || !Helper.CompareFileEx(originLuaPath, allOutputByteFilePaths[i]))
                    {
                        FileUtil.DeleteFileOrDirectory(allOutputByteFilePaths[i]);
                    }
                }
                else
                {
                    FileUtil.DeleteFileOrDirectory(allOutputByteFilePaths[i]);
                }
            }
        }

        string[] allOutputDirectories = Helper.GetDirectories("Assets/LuaByte/");
        for (int i = 0; i < allOutputDirectories.Length; i++)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(allOutputDirectories[i]);
            if (di.GetFiles().Length + di.GetDirectories().Length == 0)
            {
                //目录为空
                FileUtil.DeleteFileOrDirectory(allOutputDirectories[i]);
            }
        }

        ///将Lua文件夹内改动过的文件复制到LuaByte文件夹，并且添加后缀，同时记录路径
        List<string> outputBytePathList = new List<string>();
        string[] originLuaPaths = Helper.GetFiles(originLuaRootPath, null, true);

        for (int i = 0; i < originLuaPaths.Length; i++)
        {
            originLuaPaths[i] = originLuaPaths[i].Replace('\\', '/');
            if (!originLuaPaths[i].Contains("meta"))//meta文件作删除处理
            {
                string copyPath = originLuaPaths[i].Replace(originLuaRootPath, outputByteRootPath) + ".bytes";

                string[] copyPathSplit = copyPath.Split('/');
                string needCheckCopyPath = string.Empty;
                for (int j = 0; j < copyPathSplit.Length - 1; j++)
                {
                    needCheckCopyPath += copyPathSplit[j] + "/";
                }

                Helper.CheckPathExistence(needCheckCopyPath);
                //在文件不存在的情况下才拷贝过去
                if (!File.Exists(copyPath))
                {
                    FileUtil.CopyFileOrDirectory(originLuaPaths[i], copyPath);
                }
                outputBytePathList.Add(copyPath);
            }
        }
        AssetDatabase.Refresh();

        //设置输出文件夹下的AssetBundleName
        for (int i = 0; i < outputBytePathList.Count; i++)
        {
            AssetImporter importer = AssetImporter.GetAtPath(outputBytePathList[i]);
            if (importer != null)
            {
                string[] split = outputBytePathList[i].Split('/');
                importer.assetBundleName = "lua/" + split[2];
            }
            else
            {
                Debug.LogError("can't not set AssetBundleName  -----   " + outputBytePathList[i]);
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    #endregion
}
