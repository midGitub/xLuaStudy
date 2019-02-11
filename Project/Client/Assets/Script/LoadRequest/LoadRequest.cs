using UnityEngine;

public class LoadRequest
{
    public virtual bool Load(Priority priority, out bool process)
    {
        process = false;
        return false;
    }
    
}

