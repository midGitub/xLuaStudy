using System.Collections.Generic;
#region fileversion.json 解析类

public class VersionJsonObject
{
    public uint version;
    public List<ABNameHash> ABHashList;

    public VersionJsonObject()
    {
        ABHashList = new List<ABNameHash>();
    }
}

public class ABNameHash
{
    public string abName;
    public int hashCode;
}

#endregion

#region AllPackageVersion.json 解析类

public class VersionIsNewPackage
{
    public uint version;
    public bool isNewPackage;
}

#endregion

#region FileVersion.json 解析类
public class FileVersionJsonObject
{
    public List<VersionAndSize> versionSizeList;

    public FileVersionJsonObject()
    {
        versionSizeList = new List<VersionAndSize>();
    }
}

public class VersionAndSize
{
    public string name;
    public uint version;
    public uint size;
}
#endregion