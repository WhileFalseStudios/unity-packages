using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace WhileFalse.Core
{
    public static class WhileFalseUtility
    {
        public static GUIContent GetIconWithText(string text, string iconPath)
        {
            var gui = GetIcon(iconPath);
            gui.text = text;
            return gui;
        }

        public static GUIContent GetIcon(string iconPath)
        {
            if (EditorGUIUtility.isProSkin)
            {
                var ext = Path.GetExtension(iconPath);
                var path = Path.GetFileNameWithoutExtension(iconPath);
                path += "_dark";
                return new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(path + ext));
            }
            else
            {
                return new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>(iconPath));
            }
        }

        public static GUISkin GetSkin(string skinPath)
        {
            return AssetDatabase.LoadAssetAtPath<GUISkin>(skinPath);
        }
    }
}
