public enum LocalCode
{
    SUCCESS,
    FAILED = 65536,
    CANCELED,
    TIMEOUT,
    ASYNC_BUSY,
    NEED_TASK,
    PATCHER_END,
    CurVerIsNewest,//当前版本已最新
    DownloadAllPackageVersionFault,//下载AllPackageVersion.json文件失败
    CurServerVerIsNewPackage,//当前服务器版本是新包，需要换包
    DownloadVersionJsonFault,//下载version.json错误
    DownloadAssetBundleFileFault,//下载总的AssetBundle错误
    SACopyToPDCoroutineSuccess,//从SA到PD复制成功
    SACopyToPDCoroutineFault,//从SA到PD复制失败
    CanNotFindVersionInCDN,//在资源库中找不到对应版本资源
    DownloadFileVersionJsonFault,//下载fileversion.json文件失败
}
