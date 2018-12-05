using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WhileFalse.Core
{
    public static class Documentation
    {
        public static void Open(string docPath)
        {
            DocsWindow.OpenDocs(docPath);
        }

        [MenuItem("Tools/While False/Show Documentation")]
        private static void OpenWhileFalseDocs()
        {
            Open("Packages/com.whilefalse.core/README.md");
        }

        [MenuItem("Assets/Open In Docs")]
        static void ShowPath()
        {
            var obj = Selection.GetFiltered<TextAsset>(SelectionMode.Assets).FirstOrDefault();
            if (!obj)
                return;

            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path))
            {
                Open(path);
            }
        }
    }
}
