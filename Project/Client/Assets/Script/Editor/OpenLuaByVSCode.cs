using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

public static class OpenLuaByVSCode
{
    [MenuItem("Assets/Open Lua Project", false, 1000)]
    public static void OpenLuaProject()
    {
        Helper.OpenBat(@"E:\UnityProject\Personal\xLuaStudy\Project\Client\Assets\Bat\OpenLuaProject.bat");
    }
}
