/// <summary>
/// 加载优先级
/// </summary>
public enum AssetPriority
{
    CODE,
    SCENE,
    UI,
    EFFECT,
    PRIORITY_COUNT
}

/// <summary>
/// 运行方式
/// </summary>
public enum RunType
{
    PATCHER_SA_PS,//走正常热更流程,读资源的时候会从SA和PS下拿
    NOPATCHER_SA,//不走热更流程，仅读打好的Bundle资源
    NOPATCHER_RES,//不走热更流程，从Assets内读取资源
}