using UnityEngine;

/// <summary>
/// 各种路径的定义
/// </summary>
public class PathDefine
{
    #region local
    public static string localABPath(string pfStr) { return Application.streamingAssetsPath + "/" + pfStr + "/"; }
    #endregion

    #region server
    /// <summary>
    /// 服务器地址
    /// </summary>
    public static string serverPath(int version, string pfStr) { return "http://192.168.1.175/AssetsBundle/" + pfStr + "/" + version + "/"; }

    /// <summary>
    /// 本地服务器地址
    /// </summary>
    /// <param name="version"></param>
    /// <param name="pfStr"></param>
    /// <returns></returns>
    public static string serverPathInLocal(string pfStr, int version) { return "D:/LocalServer/AssetsBundle/" + pfStr + "/" + version + "/"; }

    /// <summary>
    /// 各平台AllPackageVersion.json文件位置
    /// </summary>
    /// <param name="pfStr"></param>
    /// <returns></returns>
    public static string serverPathInLocal_AllPackageVersion(string pfStr) { return "D:/LocalServer/AssetsBundle/" + pfStr + "/" + "AllPackageVersion.json"; }
    #endregion
}
