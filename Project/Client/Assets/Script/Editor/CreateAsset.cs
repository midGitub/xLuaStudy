using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAsset : Editor
{
    // 在菜单栏创建功能项
    [MenuItem("Assets/Create/AssetFile/GameSetting.asset")]
    static void CreateGameSetting()
    {
        // 实例化类  Bullet
        ScriptableObject gameSetting = ScriptableObject.CreateInstance<GameSetting>();


        // 如果实例化 Bullet 类为空，返回
        if (!gameSetting)
        {
            Debug.LogWarning("GameSetting not found");
            return;
        }

        //拼接保存自定义资源（.asset） 路径

        Object select = Selection.activeObject;
        string path = string.Format(AssetDatabase.GetAssetPath(select) + "/{0}.asset", (typeof(GameSetting).ToString()));

        // 生成自定义资源到指定路径
        AssetDatabase.CreateAsset(gameSetting, path);
    }
}