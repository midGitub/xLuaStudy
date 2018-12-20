using UnityEngine;

/// <summary>
/// 各种路径的定义
/// </summary>
public class PathDefine
{
    #region local
    /// <summary>
    /// 编辑器AB包路径
    /// </summary>
    public static string buildABPathEditor = Application.streamingAssetsPath + "/Editor/AssetsBundle/";

    /// <summary>
    /// 编辑器Version文件路径
    /// </summary>
    public static string buildVersionPathEditor = Application.streamingAssetsPath + "/Editor/Version/version.json";

    /// <summary>
    /// 安卓环境AB包路径
    /// </summary>
    public static string buildABPathAndroid = Application.streamingAssetsPath + "/Android/AssetsBundle/";

    /// <summary>
    /// 安卓环境Version文件路径
    /// </summary>
    public static string buildVersionPathAndroid = Application.streamingAssetsPath + "/Android/Version/version.json";
    #endregion

    #region server
    /// <summary>
    /// 服务器地址
    /// </summary>
    public static string serverPath = "http://192.168.1.175/";
    public static string serverPathInLocal = "D:/LocalServer/";

    /// <summary>
    /// 服务器编辑器环境下AB包地址
    /// </summary>
    public static string serverABPathEditor(int version) { return "D:/LocalServer/AssetsBundle/" + version + "/Editor/AssetsBundle/"; }

    /// <summary>
    /// 服务器编辑器环境下Version文件地址
    /// </summary>
    public static string serverVersionPathEditor(int version) { return "D:/LocalServer/AssetsBundle/" + version + "/Editor/Version/version.json"; }

    /// <summary>
    /// 服务器编辑器环境下FileVersion文件地址
    /// </summary>
    public static string serverFileVersionPathEditor(int version) { return "D:/LocalServer/AssetsBundle/" + version + "/Editor/FileVersion/fileversion.json"; }


    #endregion
}
