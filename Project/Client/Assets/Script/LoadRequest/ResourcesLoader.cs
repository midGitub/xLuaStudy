using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class ResourcesLoader
{
    public static void LoadLuaData(string path, Action<Dictionary<string, byte[]>> onLoadFinishCallBack)
    {
        Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>();
        byte[] byteArray = Helper.LoadFileData(path);
        string[] split = path.Replace('\\', '/').Split('/');
        string name = split[split.Length - 1].Replace(".lua", "").ToLower();
        dict.Add(name, byteArray);
        if (onLoadFinishCallBack != null)
        {
            onLoadFinishCallBack.Invoke(dict);
        }
    }
}

