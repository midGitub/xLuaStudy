using System.Collections.Generic;

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