using System.IO;
using UnityEngine;

/// <summary>
/// 各种路径的定义
/// </summary>
public class PathDefine
{
    #region local

    /// <summary>
    /// 真机环境 StreamingAssetsPath
    /// </summary>
    /// <param name="pfStr"></param>
    /// <returns></returns>
    public static string StreamingAssetsPathByPF(string pfStr)
    {
        string path = string.Empty;

        if (pfStr == "Editor")
        {
            path = Application.streamingAssetsPath + "/" + pfStr + "/";
        }
        else if (pfStr == "Android")
        {
            path = "jar:file://" + Application.dataPath + "!/assets/Android/";
        }
        return path;
    }

    /// <summary>
    /// 打包环境 StreamingAssetsPath
    /// </summary>
    /// <param name="pfStr"></param>
    /// <returns></returns>
    public static string StreamingAssetsPath(string pfStr)
    {
        return Application.streamingAssetsPath + "/" + pfStr + "/";
    }

    public static string presitantABPath()
    {
        string path = string.Empty;
#if UNITY_EDITOR
        path = Application.persistentDataPath + "/" + Helper.GetPlatformString() + "/AssetsBundle/";
#elif !UNITY_EDITOR && UNITY_ANDROID
        path = Application.persistentDataPath + "/" +  Helper.GetPlatformString() + "/AssetsBundle/";
#endif
        return path;
    }

    #endregion

    #region server
    /// <summary>
    /// 服务器地址
    /// </summary>
    public static string serverPath(string pfStr) { return "http://192.168.1.175/AssetsBundle/" + pfStr + "/"; }
    public static string serverPath(string pfStr, int version) { return "http://192.168.1.175/AssetsBundle/" + pfStr + "/" + version + "/"; }

    /// <summary>
    /// 本地服务器地址
    /// </summary>
    /// <param name="version"></param>
    /// <param name="pfStr"></param>
    /// <returns></returns>
    public static string serverPathInLocal(string pfStr, int version) { return "../Server/AssetsBundle/" + pfStr + "/" + version + "/"; }

    /// <summary>
    /// 各平台AllPackageVersion.json文件位置
    /// </summary>
    /// <param name="pfStr"></param>
    /// <returns></returns>
    public static string serverPathInLocal_AllPackageVersion(string pfStr) { return "../Server/AssetsBundle/" + pfStr + "/" + "AllPackageVersion.json"; }

    public static string GetAssetUrl(string assetName)
    {
        string url = string.Empty;

        FileInfo fi = new FileInfo(presitantABPath() + "AssetsBundle/" + assetName);
        if (fi.Exists)
        {
            return fi.FullName;
        }
        else
        {
            return StreamingAssetsPathByPF(Helper.GetPlatformString()) + "AssetsBundle/" + assetName;
        }
    }

    #endregion
}
