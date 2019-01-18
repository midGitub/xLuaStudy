using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    #region Instance
    private static CoroutineManager instance;

    public static CoroutineManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerGroup = Helper.GetManagerGroup();

                instance = managerGroup.GetComponentInChildren<CoroutineManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = managerGroup.transform;
                    go.name = typeof(CoroutineManager).Name;
                    instance = go.AddComponent<CoroutineManager>();
                }
            }
            return instance;
        }
    }
    #endregion
}

