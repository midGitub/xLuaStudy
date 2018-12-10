using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LitJson;

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

        string[] allAssetBundlesName = manifest.GetAllAssetBundles();

        string jsonSavePath = "Assets/Resources/version.json";

        //  JsonWriter jsonWriter = new JsonWriter();
        // jsonWriter.WriteObjectStart();

        // jsonWriter.WritePropertyName("Version");//当前客户端初始版本
        // jsonWriter.Write(1);

        //  jsonWriter.WritePropertyName("ABHashGroup");
        //jsonWriter.WriteArrayStart();

        JsonData allJsonData = new JsonData() { };

        allJsonData["VersionCode"] = GameSetting.Instance.versionCode;
        allJsonData["ABHashGroup"] = new JsonData();
        for (int i = 0; i < allAssetBundlesName.Length; i++)
        {
            Hash128 hash = manifest.GetAssetBundleHash(allAssetBundlesName[i]);
            int hashCode = hash.GetHashCode();

            JsonData curABData = new JsonData();
            curABData[allAssetBundlesName[i]] = hashCode;

            allJsonData["ABHashGroup"].Add(curABData);
            //jsonWriter.WritePropertyName(allAssetBundlesName[i]);
            // jsonWriter.Write(allAssetBundlesName[i] + ":" + hashCode);
        }
        //jsonWriter.WriteArrayEnd();

        // jsonWriter.WriteObjectEnd();







        Stream sw = null;
        FileInfo fileInfo = new FileInfo(jsonSavePath);
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
        }

        CheckPathExistence(fileInfo.Directory.FullName);

        //如果此文件不存在则创建
        sw = fileInfo.Create();
        //写入
        string json = allJsonData.ToJson();
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(json.ToString());
        sw.Write(byteArray, 0, byteArray.Length);

        sw.Flush();
        //关闭流
        sw.Close();
        //销毁流
        sw.Dispose();

        Debug.Log("json" + "成功保存到本地~");
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
        string outPutByteRootPath = CheckPathExistence("Assets/LuaByte/");

        /// 清零掉这个 目录下的 所有文件夹以及文件 方便重新生成
        string[] byteDirectories = Directory.GetDirectories(outPutByteRootPath);

        for (int i = 0; i < byteDirectories.Length; i++)
        {
            FileUtil.DeleteFileOrDirectory(byteDirectories[i]);
        }

        ///将Lua文件夹内的文件复制到LuaByte文件夹，并且添加后缀，同时记录路径
        List<string> outPutBytePathList = new List<string>();
        string[] allDirectories = Directory.GetDirectories("Assets/Lua/");
        for (int i = 0; i < allDirectories.Length; i++)
        {
            string[] luaFiles = Directory.GetFiles(allDirectories[i] + "/", "*.lua", SearchOption.TopDirectoryOnly);
            for (int j = 0; j < luaFiles.Length; j++)
            {
                string fname = Path.GetFileName(luaFiles[j]);
                string[] pathSplit = luaFiles[j].Split('/');

                string outPutBytePath = string.Empty;
                for (int m = 0; m < pathSplit.Length - 1; m++)
                {
                    outPutBytePath += pathSplit[m] + "/";
                }

                outPutBytePath = CheckPathExistence(outPutBytePath.Replace("Lua", "LuaByte")) + fname + ".bytes";
                FileUtil.CopyFileOrDirectory(luaFiles[j], outPutBytePath);
                outPutBytePathList.Add(outPutBytePath);
            }
        }

        //设置AssetBundleName
        for (int i = 0; i < outPutBytePathList.Count; i++)
        {
            Debug.LogError(outPutBytePathList[i] + "  " + i);
            AssetImporter importer = AssetImporter.GetAtPath(outPutBytePathList[i]);
            if (importer != null)
            {
                string[] split = outPutBytePathList[i].Split('/');
                importer.assetBundleName = "lua/" + split[2];
            }

            importer = AssetImporter.GetAtPath("Assets/LuaByte/NewFolder/wwwrr.lua.bytes");

            if (EditorUtility.DisplayCancelableProgressBar("SetBundleName", "LuaBundleName --> Lua", (float)i / outPutBytePathList.Count))
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