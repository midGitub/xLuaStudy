using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Launcher : MonoBehaviour
{
    private GameObject managerGroupObj;

    private List<BaseManager> managerList = new List<BaseManager>();

    private void Awake()
    {
        managerGroupObj = GameObject.Find("ManagerGroup");
    }
    
    private void Start()
    {
        GameObject luaMgrObj = new GameObject();
        luaMgrObj.name = "LuaMgr";
        luaMgrObj.transform.SetParent(managerGroupObj.transform);
        luaMgrObj.AddComponent<LuaMgr>();
        LuaMgr.Instance.Init();
    }
}
