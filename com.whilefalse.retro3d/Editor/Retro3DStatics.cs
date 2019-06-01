using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WhileFalse.Core;

namespace WhileFalse.Retro3D.Editor
{
    public static class Retro3DStatics
    {
        [MenuItem("Tools/While False/Retro3D/Show Documentation")]
        static void OpenDocs()
        {
            Documentation.Open("Packages/com.whilefalse.retro3d/README.md");
        }
    }
}
