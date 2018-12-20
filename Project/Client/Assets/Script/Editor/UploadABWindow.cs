using LitJson;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 上传本地AssetsBundle至服务器
/// </summary>
public class UploadABWindow : EditorWindow
{
    private static UploadABWindow uploadABWindow;
    private int version = 0;
    private int platform = 0;
    private bool isNewPackage = false;//是否全新新包
    [MenuItem("UploadAssetsBundle/UploadABToServer", false, 10)]
    public static void OpenUploadAssetsBundleToServerView()
    {
        uploadABWindow = GetWindow<UploadABWindow>();
        uploadABWindow.maxSize = new Vector2(170, 150);
        uploadABWindow.minSize = new Vector2(170, 150);
        uploadABWindow.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUIStyle.none);
        {
            EditorGUILayout.PrefixLabel("所需上传AB的版本号");
            version = EditorGUILayout.IntField(version, GUILayout.Width(150));

            GUIContent[] guiContentList = new GUIContent[3];

            for (int i = 0; i < guiContentList.Length; i++)
            {
                guiContentList[i] = new GUIContent();
            }

            guiContentList[0].text = "Editor";
            guiContentList[1].text = "Android";
            guiContentList[2].text = "Ios";
            platform = EditorGUILayout.Popup(platform, guiContentList, GUILayout.Width(150));

            isNewPackage = GUILayout.Toggle(isNewPackage, "是否全新整包");

            if (GUILayout.Button("上传", GUILayout.Width(100)))
            {
                Upload();
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 上传AB 上传的时候需要生成AB版本文件
    /// </summary>
    private void Upload()
    {
        string sourcePath = string.Empty;
        string assetBundlePath = string.Empty;
        string serverPath = string.Empty;
        string serverPathInLocal = string.Empty;

        if (platform == 0)
        {
            Debug.LogError("正在上传Editor平台的AssetsBundle");
            sourcePath = PathDefine.buildABPathEditor;
            assetBundlePath = Application.streamingAssetsPath + "/Editor/AssetsBundle/";
            serverPath = PathDefine.serverPath + "AssetsBundle/" + version + "/Editor/";
            serverPathInLocal = PathDefine.serverPathInLocal + "AssetsBundle/" + version + "/Editor/";
        }
        else if (platform == 1)
        {
            Debug.LogError("正在上传Android平台的AssetsBundle");
        }
        else if (platform == 2)
        {
            Debug.LogError("正在上传Ios平台的AssetsBundle");
        }

        //先找出需要移动的AB文件
        List<string> shouldMoveList = new List<string>();
        string[] allAB = Helper.GetFiles(assetBundlePath, null, true, true);
        for (int i = 0; i < allAB.Length; i++)
        {
            allAB[i] = allAB[i].Replace("\\", "/");
            if (!allAB[i].Contains(".meta") && !allAB[i].Contains(".manifest"))
            {
                //计算AssetBundleName
                string name = string.Empty;
                string[] pathGroup = allAB[i].Split('/');

                int index = 0;
                for (int j = 0; j < pathGroup.Length; j++)
                {
                    if (pathGroup[j] == "AssetsBundle")
                    {
                        index = j + 1;
                        break;
                    }
                }
                for (int m = index; m < pathGroup.Length; m++)
                {
                    name = name + (m == index ? string.Empty : "/") + pathGroup[m];
                }

                shouldMoveList.Add(name);
            }
        }

        //判断是全新整包
        if (isNewPackage == true)
        {
            JsonData allJsonData = new JsonData() { };

            for (int i = 0; i < shouldMoveList.Count; i++)
            {
                //拼接AB版本文件，用于记录每个AB的版本
                JsonData curJsonData = new JsonData();
                {
                    curJsonData["name"] = shouldMoveList[i];
                    curJsonData["info"] = new JsonData();
                    {
                        byte[] b = File.ReadAllBytes(assetBundlePath + shouldMoveList[i]);
                        JsonData curABData = new JsonData();
                        curABData["version"] = version;
                        curABData["size"] = b.Length;
                        curJsonData["info"].Add(curABData);
                    }
                    allJsonData.Add(curJsonData);
                }
            }

            string verJson = Helper.JsonTree(allJsonData.ToJson());
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(verJson.ToString());

            //保存版本文件
            if (File.Exists(PathDefine.serverFileVersionPathEditor(version)))
            {
                File.Delete(PathDefine.serverFileVersionPathEditor(version));
                AssetDatabase.Refresh();
            }
            FileInfo verFileInfo = new FileInfo(PathDefine.serverFileVersionPathEditor(version));
            Helper.SaveAssetToLocalFile(Helper.CheckPathExistence(verFileInfo.Directory.FullName), verFileInfo.Name, byteArray);

            //修改游戏包体版本文件
            string allPackageVersionDataPath = PathDefine.serverPathInLocal + "AssetsBundle/AllPackageVersion.json";
            string pristinePkgVersionText = string.Empty;

            JsonData allPackageVersionData = new JsonData() { };
            if (File.Exists(allPackageVersionDataPath))
            {
                pristinePkgVersionText = File.ReadAllText(allPackageVersionDataPath, System.Text.Encoding.UTF8);

                JsonData jsonData = JsonMapper.ToObject(pristinePkgVersionText);
                int count = jsonData.Count;
                for (int i = 0; i < count; i++)
                {
                    allPackageVersionData.Add(jsonData[i]);
                }
            }

            bool isInJson = false;
            for (int i = 0; i < allPackageVersionData.Count; i++)
            {
                if (int.Parse(allPackageVersionData[i]["Version"].ToString()) == version)
                {
                    allPackageVersionData[i]["isNewPackage"] = isNewPackage;
                    isInJson = true;
                    break;
                }
            }

            if (isInJson == false)
            {
                JsonData curPackageVersionData = new JsonData();
                curPackageVersionData["Version"] = version;
                curPackageVersionData["isNewPackage"] = isNewPackage;
                allPackageVersionData.Add(curPackageVersionData);
            }

            //将pkgJson数据转化成byte[]

            string pkgJson = Helper.JsonTree(allPackageVersionData.ToJson());
            byte[] pkgJsonByteArray = System.Text.Encoding.Default.GetBytes(pkgJson.ToString());

            //保存游戏包体版本文件(用于判断包体版本是否首包，后续手机更新时会用到)
            if (File.Exists(allPackageVersionDataPath))
            {
                File.Delete(allPackageVersionDataPath);
                AssetDatabase.Refresh();
            }
            FileInfo pkgFileInfo = new FileInfo(allPackageVersionDataPath);
            Helper.SaveAssetToLocalFile(Helper.CheckPathExistence(pkgFileInfo.Directory.FullName), pkgFileInfo.Name, pkgJsonByteArray);
        }
        else
        {

        }

        //移动AB包
        for (int i = 0; i < shouldMoveList.Count; i++)
        {
            string targetPath = serverPathInLocal + shouldMoveList[i];

            string[] targetPathGroup = targetPath.Split('/');

            string needCheckDirectoryPath = string.Empty;//需要检查的文件夹地址
            for (int j = 0; j < targetPathGroup.Length - 1; j++)
            {
                needCheckDirectoryPath = needCheckDirectoryPath + (j == 0 ? string.Empty : "/") + targetPathGroup[j];
            }
            Helper.CheckPathExistence(needCheckDirectoryPath);

            byte[] fileByte = File.ReadAllBytes(assetBundlePath + shouldMoveList[i]);

            FileStream fsDes = File.Create(targetPath);
            fsDes.Write(fileByte, 0, fileByte.Length);
            fsDes.Flush();
            fsDes.Close();
        }
    }
}

