using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Helper
{
    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <returns></returns>
    public static byte[] LoadFileData(string fileUrl)
    {
        FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
        byte[] buffur = new byte[fs.Length];

        fs.Read(buffur, 0, buffur.Length);
        fs.Close();
        return buffur;
    }

    /// 将文件模型创建到本地
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="info"></param>
    /// <param name="length"></param>
    public static void SaveAssetToLocalFile(string path, string name, byte[] info)
    {
        Stream sw = null;
        FileInfo fileInfo = new FileInfo(path + "/" + name);
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
        }
        Helper.CheckPathExistence(fileInfo.Directory.FullName);
        //如果此文件不存在则创建
        sw = fileInfo.Create();
        //写入
        sw.Write(info, 0, info.Length);

        sw.Flush();
        //关闭流
        sw.Close();
        //销毁流
        sw.Dispose();
    }

    public static void SaveAssetToLocalFile(string path, byte[] info)
    {
        Stream sw = null;
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
        }
        Helper.CheckPathExistence(fileInfo.Directory.FullName);
        //如果此文件不存在则创建
        sw = fileInfo.Create();
        //写入
        sw.Write(info, 0, info.Length);

        sw.Flush();
        //关闭流
        sw.Close();
        //销毁流
        sw.Dispose();
    }

    /// <summary>
    /// 检查文件路径是否存在，不存在则创建
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string CheckPathExistence(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        return path;
    }

    /// <summary>
    /// 逐一比较字节数
    /// </summary>
    /// <param name="firstFile"></param>
    /// <param name="secondFile"></param>
    /// <returns></returns>
    public static bool CompareFile(string firstFile, string secondFile)
    {
        if (!File.Exists(firstFile) || !File.Exists(secondFile))
        {
            return false;
        }
        if (firstFile == secondFile)
        {
            return true;
        }
        int firstFileByte = 0;
        int secondFileByte = 0;
        FileStream secondFileStream = new FileStream(secondFile, FileMode.Open);
        FileStream firstFileStream = new FileStream(firstFile, FileMode.Open);
        if (firstFileStream.Length != secondFileStream.Length)
        {
            firstFileStream.Close();
            secondFileStream.Close();
            return false;
        }
        do
        {
            firstFileByte = firstFileStream.ReadByte();
            secondFileByte = secondFileStream.ReadByte();
        } while ((firstFileByte == secondFileByte) && (firstFileByte != -1));
        firstFileStream.Close();
        secondFileStream.Close();
        return (firstFileByte == secondFileByte);
    }

    /// <summary>
    /// 逐行比较内容
    /// </summary>
    /// <param name="firstFile"></param>
    /// <param name="secondFile"></param>
    /// <returns></returns>
    public static bool CompareFileEx(string firstFile, string secondFile)
    {
        if (!File.Exists(firstFile) || !File.Exists(secondFile))
        {
            return false;
        }
        if (firstFile == secondFile)
        {
            return true;
        }
        string firstFileLine = string.Empty;
        string secondFileLine = string.Empty;
        StreamReader secondFileStream = new StreamReader(secondFile, Encoding.Default);
        StreamReader firstFileStream = new StreamReader(firstFile, Encoding.Default);
        do
        {
            firstFileLine = firstFileStream.ReadLine();
            secondFileLine = secondFileStream.ReadLine();
        } while ((firstFileLine == secondFileLine) && (firstFileLine != null));
        firstFileStream.Close();
        secondFileStream.Close();
        return (firstFileLine == secondFileLine);
    }

    #region JSON
    /// <summary>
    /// JSON字符串格式化
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string JsonTree(string json)
    {
        int level = 0;
        var jsonArr = json.ToArray();  // Using System.Linq;
        string jsonTree = string.Empty;
        for (int i = 0; i < json.Length; i++)
        {
            char c = jsonArr[i];
            if (level > 0 && '\n' == jsonTree.ToArray()[jsonTree.Length - 1])
            {
                jsonTree += TreeLevel(level);
            }
            switch (c)
            {
                case '[':
                    jsonTree += c + "\n";
                    level++;
                    break;
                case ',':
                    jsonTree += c + "\n";
                    break;
                case ']':
                    jsonTree += "\n";
                    level--;
                    jsonTree += TreeLevel(level);
                    jsonTree += c;
                    break;
                default:
                    jsonTree += c;
                    break;
            }
        }
        return jsonTree;
    }

    /// <summary>
    /// 树等级
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private static string TreeLevel(int level)
    {
        string leaf = string.Empty;
        for (int t = 0; t < level; t++)
        {
            leaf += "\t";
        }
        return leaf;
    }
    #endregion

    /// <summary>
    /// 获取指定目录中的匹配项（文件或目录）
    /// </summary>
    /// <param name="dir">要搜索的目录</param>
    /// <param name="regexPattern">项名模式（正则）。null表示忽略模式匹配，返回所有项</param>
    /// <param name="recurse">是否搜索子目录</param>
    /// <param name="throwEx">是否抛异常</param>
    /// <returns></returns>
    public static string[] GetFileSystemEntries(string dir, string regexPattern = null, bool recurse = false, bool throwEx = false)
    {
        List<string> lst = new List<string>();

        try
        {
            foreach (string item in Directory.GetFileSystemEntries(dir))
            {
                try
                {
                    if (regexPattern == null || Regex.IsMatch(Path.GetFileName(item), regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    {
                        lst.Add(item);
                    }

                    //递归
                    if (recurse && (File.GetAttributes(item) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        lst.AddRange(GetFileSystemEntries(item, regexPattern, true));
                    }
                }
                catch
                {
                    if (throwEx)
                    {
                        throw;
                    }
                }
            }
        }
        catch
        {
            if (throwEx)
            {
                throw;
            }
        }

        return lst.ToArray();
    }

    /// <summary>
    /// 获取指定目录中的匹配文件
    /// </summary>
    /// <param name="dir">要搜索的目录</param>
    /// <param name="regexPattern">文件名模式（正则）。null表示忽略模式匹配，返回所有文件</param>
    /// <param name="recurse">是否搜索子目录</param>
    /// <param name="throwEx">是否抛异常</param>
    /// <returns></returns>
    public static string[] GetFiles(string dir, string regexPattern = null, bool recurse = false, bool throwEx = false)
    {
        List<string> lst = new List<string>();

        try
        {
            foreach (string item in Directory.GetFileSystemEntries(dir))
            {
                try
                {
                    bool isFile = (File.GetAttributes(item) & FileAttributes.Directory) != FileAttributes.Directory;

                    if (isFile && (regexPattern == null || Regex.IsMatch(Path.GetFileName(item), regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline)))
                    {
                        lst.Add(item);
                    }

                    //递归
                    if (recurse && !isFile)
                    {
                        lst.AddRange(GetFiles(item, regexPattern, true));
                    }
                }
                catch
                {
                    if (throwEx)
                    {
                        throw;
                    }
                }
            }
        }
        catch
        {
            if (throwEx)
            {
                throw;
            }
        }

        return lst.ToArray();
    }

    /// <summary>
    /// 获取指定目录中的匹配目录
    /// </summary>
    /// <param name="dir">要搜索的目录</param>
    /// <param name="regexPattern">目录名模式（正则）。null表示忽略模式匹配，返回所有目录</param>
    /// <param name="recurse">是否搜索子目录</param>
    /// <param name="throwEx">是否抛异常</param>
    /// <returns></returns>
    public static string[] GetDirectories(string dir, string regexPattern = null, bool recurse = false, bool throwEx = false)
    {
        List<string> lst = new List<string>();

        try
        {
            foreach (string item in Directory.GetDirectories(dir))
            {
                try
                {
                    if (regexPattern == null || Regex.IsMatch(Path.GetFileName(item), regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    {
                        lst.Add(item);
                    }

                    //递归
                    if (recurse)
                    {
                        lst.AddRange(GetDirectories(item, regexPattern, true));
                    }
                }
                catch
                {
                    if (throwEx)
                    {
                        throw;
                    }
                }
            }
        }
        catch
        {
            if (throwEx)
            {
                throw;
            }
        }

        return lst.ToArray();
    }

    /// <summary>
    /// 删除文件夹及子文件内文件
    /// </summary>
    /// <param name="str"></param>
    public static void DeleteFiles(string str)
    {
        DirectoryInfo fatherFolder = new DirectoryInfo(str);
        //删除当前文件夹内文件
        FileInfo[] files = fatherFolder.GetFiles();
        foreach (FileInfo file in files)
        {
            //string fileName = file.FullName.Substring((file.FullName.LastIndexOf("\\") + 1), file.FullName.Length - file.FullName.LastIndexOf("\\") - 1);
            string fileName = file.Name;
            try
            {
                if (!fileName.Equals("index.dat"))
                {
                    File.Delete(file.FullName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("删除文件夹出错 ---" + ex);
            }
        }
        //递归删除子文件夹内文件
        foreach (DirectoryInfo childFolder in fatherFolder.GetDirectories())
        {
            DeleteFiles(childFolder.FullName);
        }
    }

    /// <summary>
    /// 加载版本信息文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static VersionJsonObject LoadVersionJson(string data)
    {
        VersionJsonObject versionJsonObject = new VersionJsonObject();

        JsonData jsonData = JsonMapper.ToObject(data);
        versionJsonObject.version = uint.Parse(jsonData["VersionCode"].ToString());

        int count = jsonData["ABHashList"].Count;

        for (int i = 0; i < count; i++)
        {
            JsonData abhashData = jsonData["ABHashList"][i];

            ABNameHash abNameHash = new ABNameHash();
            abNameHash.abName = abhashData.Keys.ToArray()[0];
            abNameHash.hashCode = int.Parse(abhashData[0].ToJson());
            versionJsonObject.ABHashList.Add(abNameHash);
        }

        return versionJsonObject;
    }

    /// <summary>
    /// 加载资源版本文件
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FileVersionJsonObject LoadFileVersionJson(string data)
    {
        FileVersionJsonObject fileVersionJsonObject = new FileVersionJsonObject();

        JsonData jsonData = JsonMapper.ToObject(data);

        for (int i = 0; i < jsonData.Count; i++)
        {
            VersionAndSize vas = new VersionAndSize();
            vas.name = jsonData[i]["name"].ToString();
            vas.size = uint.Parse(jsonData[i]["info"][0]["size"].ToString());
            vas.version = uint.Parse(jsonData[i]["info"][0]["version"].ToString());

            fileVersionJsonObject.versionSizeList.Add(vas);
        }

        return fileVersionJsonObject;
    }

    public static string GetPlatformString()
    {
        string pfStr = string.Empty;
        string rootAssetName = string.Empty;
#if UNITY_EDITOR
        pfStr = "Editor";
#elif !UNITY_EDITOR && UNITY_ANDROID
        pfStr = "Android";
#endif

        return pfStr;
    }

    public static byte[] stringToByteArray(string str)
    {
        return System.Text.Encoding.Default.GetBytes(str);
    }

    public static string byteArrayToString(byte[] byteArray)
    {
        return System.Text.Encoding.Default.GetString(byteArray);
    }

    ///<summary> 
    /// 序列化 
    /// </summary> 
    /// <param name="data">要序列化的对象</param> 
    /// <returns>返回存放序列化后的数据缓冲区</returns> 
    public static byte[] Serialize(object data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream rems = new MemoryStream();
        formatter.Serialize(rems, data);
        return rems.GetBuffer();
    }

    /// <summary> 
    /// 反序列化 
    /// </summary> 
    /// <param name="data">数据缓冲区</param> 
    /// <returns>对象</returns> 
    public static object Deserialize(byte[] data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream rems = new MemoryStream(data);
        data = null;
        return formatter.Deserialize(rems);
    }

    public static GameObject GetManagerGroup()
    {
        GameObject managerGroup = GameObject.Find("ManagerGroup");
        if (managerGroup == null)
        {
            managerGroup = new GameObject();
            managerGroup.name = "ManagerGroup";
            UnityEngine.Object.DontDestroyOnLoad(managerGroup);
        }

        return managerGroup;
    }
}

