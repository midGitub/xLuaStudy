using UnityEngine;
public class GameMgr : MonoBehaviour
{
    private void Awake()
    {
        gameObject.AddComponent<LuaMgr>();
    }

    private void Start()
    {
        LuaMgr.Instance.Init();
    }
}

