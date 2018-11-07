using UnityEngine;
using System.Collections;
using System.IO;

public class AssetBundleLoader : MonoBehaviour
{
    private bool b = false;

    public static AssetBundleLoader Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        LoadAssetBundleByPath(@"file://" + "E:/UnityProject/Personal/xLuaStudy/Project/Client/AssetBundles/view/uirootview.prefab.unity3d");
    }

    private string pathurl = "";
    // Use this for initialization
    public void LoadAssetBundleByPath(string path)
    {
        StartCoroutine(LoadALLGameObject(path));
    }

    //读取全部资源 
    private IEnumerator LoadALLGameObject(string path)
    {

        WWW bundle = new WWW(path);

        yield return bundle;

        Debug.LogError(bundle.assetBundle.name);
        string[] s = bundle.assetBundle.GetAllAssetNames();
        string[] ss = null;
        for (int i = 0; i < s.Length; i++)
        {
            s[i] = s[i].Replace(".prefab", "");
            ss = s[i].Split('/');
        }

        CreateModelFile(Application.persistentDataPath, ss[ss.Length - 1], bundle.bytes, bundle.bytes.Length);
        ////通过Prefab的名称把他们都读取出来 
        //Object obj0 = bundle.assetBundle.LoadAsset("UIRootView.prefab");

        ////加载到游戏中    
        //yield return Instantiate(obj0);
        b = true;
        bundle.assetBundle.Unload(false);
    }

    void CreateModelFile(string path, string name, byte[] info, int length)
    {
        //文件流信息
        //StreamWriter sw;
        Stream sw;
        FileInfo t = new FileInfo(path + "//" + name);
        if (!t.Exists)
        {
            //如果此文件不存在则创建
            sw = t.Create();
        }
        else
        {
            //如果此文件存在则打开
            //sw = t.Append();
            return;
        }
        //以行的形式写入信息
        //sw.WriteLine(info);
        sw.Write(info, 0, length);
        //关闭流
        sw.Close();
        //销毁流
        sw.Dispose();
    }

    public void Update()
    {
        if (b)
        {

        }
    }

}
