using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace WhileFalse.Core
{
    internal static class ShowAssetPath
    {
        [MenuItem("Assets/Show Path")]
        static void ShowPath()
        {
            var obj = Selection.GetFiltered<Object>(SelectionMode.Assets).FirstOrDefault();
            if (!obj)
                return;

            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Asset Path", path, "OK");
            }
        }
    }
}
