using UnityEngine;

public class LoadRequest
{
    /// <summary>
    /// 资源加载申请过期时间
    /// </summary>
    protected float timeout { get; set; }

    public virtual bool Load(Priority priority, out bool process)
    {
        process = false;
        return false;
    }

    protected bool isTimeout()
    {
        if (timeout <= Time.realtimeSinceStartup)
        {
            return true;
        }
        return false;
    }
}

