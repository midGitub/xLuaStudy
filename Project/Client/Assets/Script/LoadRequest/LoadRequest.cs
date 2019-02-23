using UnityEngine;

public class LoadRequest
{
    public string name;
    public virtual bool Load(Priority priority, out bool process)
    {
        process = false;
        return false;
    }
    
}

