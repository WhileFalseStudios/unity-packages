using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using WhileFalse.Core;

namespace WhileFalse.Cvar.Editor
{
    public class ConsoleExplorer : EditorWindow
    {
        #region Strings and Data

        private const string k_WindowTitle = "CVar Explorer";
        private const string k_WindowIconPath = "";
    
        #endregion

        [SerializeField] private string m_searchString;
        [NonSerialized] private List<ConBase> m_searchResults;
        [SerializeField] private Vector2 m_scrollPos;
    
        [MenuItem("Tools/While False/CVar Explorer")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<ConsoleExplorer>(k_WindowTitle);
            wnd.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(k_WindowTitle, EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image);
            RefreshSearch();
        }

        private void RefreshSearch()
        {
            m_searchResults = ConManager.Search(m_searchString);
            m_searchResults.Sort();
        }

        private void OnGUI()
        {
            DrawSearchBar();
            DrawList();
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            //GUILayout.FlexibleSpace();
            string newSearchString = GUILayout.TextField(m_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                // Remove focus if cleared
                newSearchString = string.Empty;
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();

            if (newSearchString != m_searchString)
            {
                m_searchString = newSearchString;
                RefreshSearch();                
            }
        }

        private void DrawList()
        {
            EditorGUILayout.LabelField("Results:");
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPos))
            {
                foreach (var cvar in m_searchResults)
                {
                    EditorGUILayout.LabelField($"{cvar.name} ({cvar.GetTypeString()})");
                }
                
                m_scrollPos = scroll.scrollPosition;
            }
        }
    }
}
