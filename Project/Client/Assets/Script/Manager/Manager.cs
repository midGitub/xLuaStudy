using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance = null;

    public void Awake()
    {
        Instance = this;
    }
}

