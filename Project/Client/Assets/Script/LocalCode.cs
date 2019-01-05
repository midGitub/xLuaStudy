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
    DownloadAssetBundleFileFault,//下载总的AssetBundle错误
}
