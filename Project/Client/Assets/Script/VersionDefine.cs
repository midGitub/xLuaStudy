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