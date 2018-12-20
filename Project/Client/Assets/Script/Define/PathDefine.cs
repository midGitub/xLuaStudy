using UnityEngine;

/// <summary>
/// 各种路径的定义
/// </summary>
public class PathDefine
{
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
}
