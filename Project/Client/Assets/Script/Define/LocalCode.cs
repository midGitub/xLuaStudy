public enum LocalCode
{
    NULL = 0,
    SUCCESS = 1,
    FAILED = 2,
    CANCELED = 3,
    TIMEOUT = 4,
    ASYNC_BUSY = 5,
    NEED_TASK = 6,
    PATCHER_END = 7,
    CUR_VER_IS_NEWEST = 8,//当前版本已最新
    DOWNLOAD_ALL_PACKAGEVERSION_FAULT = 9,//下载AllPackageVersion.json文件失败
    CUR_SERVERVER_IS_NEWPACKAGE = 10,//当前服务器版本是新包，需要换包
    DOWNLOAD_VERSIONJSON_FAULT = 11,//下载version.json错误
    DOWNLOAD_ASSETBUNDLEFILE_FAULT = 12,//下载总的AssetBundle错误
    CAN_NOT_FIND_VERSION_IN_CDN = 13,//在资源库中找不到对应版本资源
    DOWNLOAD_FILEVERSIONJSON_FAULT = 14,//下载fileversion.json文件失败
    REQ_SERVER_VERSION_CODE_FAULT = 15,//向服务器请求版本号失败
    DOWNLOAD_BUNDLE_FAULT = 16,//下载Bundle报错
    LOADER_BUSY = 17,//读取繁忙
    ASSET_FAULT = 18,//加载资源失败
    LOAD_SCENE_ERROR = 19,//读取场景出错
}
