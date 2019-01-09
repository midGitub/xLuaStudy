public enum LocalCode
{
    Null = 0,
    SUCCESS = 1,
    FAILED = 2,
    CANCELED = 3,
    TIMEOUT = 4,
    ASYNC_BUSY = 5,
    NEED_TASK = 6,
    PATCHER_END = 7,
    CurVerIsNewest = 8,//当前版本已最新
    DownloadAllPackageVersionFault = 9,//下载AllPackageVersion.json文件失败
    CurServerVerIsNewPackage = 10,//当前服务器版本是新包，需要换包
    DownloadVersionJsonFault = 11,//下载version.json错误
    DownloadAssetBundleFileFault = 12,//下载总的AssetBundle错误
    SACopyToPDCoroutineSuccess = 13,//从SA到PD复制成功
    SACopyToPDCoroutineFault = 14,//从SA到PD复制失败
    CanNotFindVersionInCDN = 15,//在资源库中找不到对应版本资源
    DownloadFileVersionJsonFault = 16,//下载fileversion.json文件失败
}
