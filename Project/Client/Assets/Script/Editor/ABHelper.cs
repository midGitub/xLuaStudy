using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ABHelper
{
    [MenuItem("ABHelper/SetBundleName", false, 0)]
    public static void SetBundleNameAll()
    {
        //Prefab
        string prefabPath = "Assets/Resources/Prefab/View";
        string[] paths = Directory.GetFiles(prefabPath, "*.prefab", SearchOption.TopDirectoryOnly)
             .Where(s => s.EndsWith(".prefab") || s.EndsWith(".bytes") || s.EndsWith(".txt"))
              .ToArray();
        for (int i = 0; i < paths.Length; i++)
        {
            AssetImporter importer = AssetImporter.GetAtPath(paths[i]);
            if (importer != null)
            {
                string bundleName = null;
                if (paths[i].EndsWith(".prefab"))
                {
                    bundleName = paths[i].Replace('\\', '/').Remove(0, "Assets/Resources/Prefab/".Length) + ".unity3d";
                }
                else if (paths[i].EndsWith(".bytes"))
                {
                    string tmp = paths[i].Replace('\\', '/').Remove(0, "Assets/Resources/Prefab/".Length);
                    tmp = tmp.Remove(tmp.LastIndexOf(".", StringComparison.Ordinal));
                    bundleName = tmp + ".unity3d";
                }
                else if (paths[i].EndsWith(".txt"))
                {
                    bundleName = null;
                }
                if (importer.assetBundleName != bundleName)
                {
                    importer.assetBundleName = bundleName;
                }
            }
            else
            {
                Debug.LogError("Prefab {0} load importer failed:" + paths[i]);
                return;
            }
            if (EditorUtility.DisplayCancelableProgressBar("SetBundleName", "PrefabBundleName --> Prefab", (float)i / paths.Length))
            {
                EditorUtility.ClearProgressBar();
                return;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
}