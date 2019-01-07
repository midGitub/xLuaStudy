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
        //区分平台
        string pfStr = string.Empty;
        if (platform == 0) { pfStr = "Editor"; }
        else if (platform == 1) { pfStr = "Android"; }
        else if (platform == 2) { pfStr = "Ios"; }

        Debug.Log("正在上传" + pfStr + "平台的AssetsBundle");

        #region 处理AllPackageVersion.json文件
        JsonData allPackageVersionData = new JsonData() { };
        string allPackageVersionDataPath = PathDefine.serverPathInLocal_AllPackageVersion(pfStr);
        string pristinePkgVersionText = string.Empty;

        //新版本信息
        JsonData curPackageVersionData = new JsonData();
        curPackageVersionData["Version"] = version;
        curPackageVersionData["isNewPackage"] = isNewPackage;

        //检查版本文件(记录isnewpackage)路径是否有文件
        if (File.Exists(allPackageVersionDataPath))
        {
            //有就直接加载
            pristinePkgVersionText = File.ReadAllText(allPackageVersionDataPath, System.Text.Encoding.UTF8);

            JsonData jsonData = JsonMapper.ToObject(pristinePkgVersionText);
            for (int i = 0; i < jsonData.Count; i++)
            {
                allPackageVersionData.Add(jsonData[i]);
            }

            //检查当前版本是否已经在文件内
            bool versionIsInFile = false;
            for (int i = 0; i < allPackageVersionData.Count; i++)
            {
                if (int.Parse(allPackageVersionData[i]["Version"].ToString()) == version)
                {
                    allPackageVersionData[i]["isNewPackage"] = isNewPackage;
                    versionIsInFile = true;
                    break;
                }
            }

            if (!versionIsInFile)
            {
                allPackageVersionData.Add(curPackageVersionData);
            }
        }
        else
        {
            allPackageVersionData.Add(curPackageVersionData);
        }

        //将pkgJson数据转化成byte[]
        string pkgJson = Helper.JsonTree(allPackageVersionData.ToJson());
        byte[] pkgJsonByteArray = System.Text.Encoding.Default.GetBytes(pkgJson.ToString());

        FileInfo pkgFileInfo = new FileInfo(allPackageVersionDataPath);
        Helper.SaveAssetToLocalFile(Helper.CheckPathExistence(pkgFileInfo.Directory.FullName), pkgFileInfo.Name, pkgJsonByteArray);

        #endregion

        #region 移动有变动的AB到指定文件夹
        //先把对应版本文件夹删掉
        if (Directory.Exists(PathDefine.serverPathInLocal(pfStr, version)))
        {
            Helper.DeleteFiles(PathDefine.serverPathInLocal(pfStr, version));
        }

        //获取最后的版本，然后去对应文件夹查找相应的AssetBundle文件
        int serverRecentVersion = 0;
        if (isNewPackage)
        {
            serverRecentVersion = version;
        }
        else
        {
            if (allPackageVersionData.Count == 1)
            {
                //误操作提示
                Debug.LogError("上传热更版本前需要有一个整包版本,当前已退出上传AB流程,请检查版本");
                return;
            }
            serverRecentVersion = int.Parse(allPackageVersionData[allPackageVersionData.Count - 2]["Version"].ToString());
        }

        List<string> shouldMoveList = new List<string>();

        AssetBundle localAssetBundle = AssetBundle.LoadFromFile(PathDefine.localABPath(pfStr) + "AssetsBundle/AssetsBundle");
        AssetBundleManifest localABManifest = localAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        localAssetBundle.Unload(false);

        AssetBundle serverAssetBundle = null;
        AssetBundleManifest serverABManifest = null;
        if (!isNewPackage)
        {
            serverAssetBundle = AssetBundle.LoadFromFile(PathDefine.serverPathInLocal(pfStr, serverRecentVersion) + "AssetsBundle");
            serverABManifest = serverAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            serverAssetBundle.Unload(false);
        }

        string[] localAllABName = localABManifest.GetAllAssetBundles();

        for (int i = 0; i < localAllABName.Length; i++)
        {
            Hash128 localHash = localABManifest.GetAssetBundleHash(localAllABName[i]);
            Hash128 serverHash = new Hash128();
            if (serverABManifest != null)
            {
                serverHash = serverABManifest.GetAssetBundleHash(localAllABName[i]);
            }

            if (serverABManifest == null)//说明是新包，全部增加
            {
                shouldMoveList.Add(localAllABName[i]);
            }
            else if (localHash.GetHashCode() != serverHash.GetHashCode())//说明是改动过的
            {
                shouldMoveList.Add(localAllABName[i]);
            }
        }

        //上面的localABManifest.GetAllAssetBundles()没有包含这个文件，补上
        //这个文件每次都会被改动到
        shouldMoveList.Add("AssetsBundle");

        //移动AB包
        for (int i = 0; i < shouldMoveList.Count; i++)
        {
            string targetPath = PathDefine.serverPathInLocal(pfStr, version) + shouldMoveList[i];

            string[] targetPathGroup = targetPath.Split('/');

            string needCheckDirectoryPath = string.Empty;//需要检查的文件夹地址
            for (int j = 0; j < targetPathGroup.Length - 1; j++)
            {
                needCheckDirectoryPath = needCheckDirectoryPath + (j == 0 ? string.Empty : "/") + targetPathGroup[j];
            }
            Helper.CheckPathExistence(needCheckDirectoryPath);

            byte[] fileByte = File.ReadAllBytes(PathDefine.localABPath(pfStr) + "AssetsBundle/" + shouldMoveList[i]);

            FileStream fsDes = File.Create(targetPath);
            fsDes.Write(fileByte, 0, fileByte.Length);
            fsDes.Flush();
            fsDes.Close();
        }
        #endregion

        #region 创建fileversion.json文件
        JsonData newFileInfoJsonData = new JsonData() { };
        //判断是否全新整包
        if (isNewPackage)
        {
            for (int i = 0; i < shouldMoveList.Count; i++)
            {
                //拼接AB版本文件，用于记录每个AB的版本
                byte[] b = File.ReadAllBytes(PathDefine.localABPath(pfStr) + "AssetsBundle/" + shouldMoveList[i]);
                JsonData jd = CreteFileVersionItemJson(shouldMoveList[i], version, b.Length);
                newFileInfoJsonData.Add(jd);
            }
        }
        else
        {
            string recentFileInfoText = File.ReadAllText(PathDefine.serverPathInLocal(pfStr, serverRecentVersion) + "fileversion.json", System.Text.Encoding.UTF8);
            newFileInfoJsonData = JsonMapper.ToObject(recentFileInfoText);

            //检查替换
            for (int i = 0; i < newFileInfoJsonData.Count; i++)
            {
                for (int j = 0; j < shouldMoveList.Count; j++)
                {
                    if (shouldMoveList[j] == newFileInfoJsonData[i]["name"].ToString())
                    {
                        //下面需要补充info
                        byte[] b = File.ReadAllBytes(PathDefine.localABPath(pfStr) + "AssetsBundle/" + shouldMoveList[j]);
                        newFileInfoJsonData[i]["info"][0]["version"] = version;
                        newFileInfoJsonData[i]["info"][0]["size"] = b.Length;
                        break;
                    }
                }
            }

            //检查新增
            for (int i = 0; i < shouldMoveList.Count; i++)
            {
                bool isAdd = true;
                for (int j = 0; j < newFileInfoJsonData.Count; j++)
                {
                    //检查新增的文件名是否在fileversion文件内
                    if (shouldMoveList[i] == newFileInfoJsonData[j]["name"].ToString())
                    {
                        isAdd = false;
                        break;
                    }
                }

                if (isAdd)
                {
                    byte[] b = File.ReadAllBytes(PathDefine.localABPath(pfStr) + "AssetsBundle/" + shouldMoveList[i]);
                    JsonData jd = CreteFileVersionItemJson(shouldMoveList[i], version, b.Length);
                    newFileInfoJsonData.Add(jd);
                }
            }
        }

        string verJson = Helper.JsonTree(newFileInfoJsonData.ToJson());
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(verJson.ToString());

        //保存版本文件
        string saveFileVersionPath = PathDefine.serverPathInLocal(pfStr, version) + "fileversion.json";
        if (File.Exists(saveFileVersionPath))
        {
            File.Delete(saveFileVersionPath);
            AssetDatabase.Refresh();
        }
        FileInfo verFileInfo = new FileInfo(saveFileVersionPath);
        Helper.SaveAssetToLocalFile(Helper.CheckPathExistence(verFileInfo.Directory.FullName), verFileInfo.Name, byteArray);

        #endregion

        #region 创建version.json文件 用于对比HASH 下载
        JsonData versionJsonData = new JsonData() { };

        versionJsonData["VersionCode"] = version;
        versionJsonData["ABHashList"] = new JsonData();
        for (int i = 0; i < localAllABName.Length; i++)
        {
            Hash128 localHash = localABManifest.GetAssetBundleHash(localAllABName[i]);
            JsonData curABData = new JsonData();
            curABData[localAllABName[i]] = localHash.GetHashCode();

            versionJsonData["ABHashList"].Add(curABData);
        }

        //添加当前总AB文件的HASH对比
        JsonData assetsBundleJD = new JsonData();
        assetsBundleJD["AssetsBundle"] = localABManifest.GetHashCode();
        versionJsonData["ABHashList"].Add(assetsBundleJD);

        string json = Helper.JsonTree(versionJsonData.ToJson());
        byte[] versionJsonByteArray = System.Text.Encoding.Default.GetBytes(json.ToString());

        //存一份version
        string jsonSavePathLocal = PathDefine.serverPathInLocal("Editor", version) + "version.json";
        FileInfo fileInfoLocal = new FileInfo(jsonSavePathLocal);
        Helper.SaveAssetToLocalFile(Helper.CheckPathExistence(fileInfoLocal.Directory.FullName), fileInfoLocal.Name, versionJsonByteArray);
        #endregion
        Debug.Log("上传完毕");
    }

    /// <summary>
    /// 创建单个fileversion信息项
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ver"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private JsonData CreteFileVersionItemJson(string name, int ver, int length)
    {
        JsonData jsonData = new JsonData();
        {
            jsonData["name"] = name;
            jsonData["info"] = new JsonData();
            {
                JsonData curABData = new JsonData();
                curABData["version"] = ver;
                curABData["size"] = length;
                jsonData["info"].Add(curABData);
            }
        }
        return jsonData;
    }
}