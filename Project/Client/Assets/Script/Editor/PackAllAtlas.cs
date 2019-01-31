using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;

public static class PackAllAtlas
{
    [MenuItem("Dev/PackAllAtlas", false, 0)]
    public static void PackAtlas()
    {
        List<string> allAtlasList = new List<string>();
        Dictionary<string, List<string>> spriteAtlasDict = new Dictionary<string, List<string>>();

        string path = "Assets/SpriteAtlas/Atlas";

        string[] allAtlasPaths = Helper.GetFiles(path, null, true, false);
        
        for (int i = 0; i < allAtlasPaths.Length; i++)
        {
            allAtlasPaths[i] = allAtlasPaths[i].Replace("\\", "/");
            if (!allAtlasPaths[i].Contains(".meta"))
            {
                allAtlasList.Add(allAtlasPaths[i]);
            }
        }

        for (int i = 0; i < allAtlasList.Count; i++)
        {
            string[] split = allAtlasList[i].Split('/');
            string atlasName = split[split.Length - 2];
            string picPath = split[split.Length - 1];

            if (!spriteAtlasDict.ContainsKey(atlasName))
            {
                List<string> list = new List<string>();
                spriteAtlasDict.Add(atlasName, list);
            }

            spriteAtlasDict[atlasName].Add(picPath);
        }

        string loadPicPath = "Assets/SpriteAtlas/Atlas/";
        string savePath = "Assets/SpriteAtlas/SpriteAtlas/";

        string[] deletePaths = Helper.GetFiles(savePath);

        for (int i = 0; i < deletePaths.Length; i++)
        {
            File.Delete(deletePaths[i]);
        }

        Helper.CheckPathExistence(savePath);
        foreach (KeyValuePair<string, List<string>> kv in spriteAtlasDict)
        {
            SpriteAtlas sa = new SpriteAtlas();
            AssetDatabase.CreateAsset(sa, savePath + kv.Key.ToString() + ".spriteAtlas");
            List<UnityEngine.Object> objectList = new List<UnityEngine.Object>();
            for (int i = 0; i < kv.Value.Count; i++)
            {
                UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(loadPicPath + kv.Key + "/" + kv.Value[i]);
                objectList.Add(obj);
            }

            sa.Add(objectList.ToArray());
            sa.SetIncludeInBuild(false);
        }
    }
}

