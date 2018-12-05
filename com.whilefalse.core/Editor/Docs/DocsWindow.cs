using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text;
using System;
using Markdig;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Threading;

namespace WhileFalse.Core
{
    internal class DocsWindow : EditorWindow
    {
        #region Window Opening        
    
        public static void OpenDocs(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset != null)
            {
                OpenDocs(asset);
            }
            else
            {
                ShowNotFound(assetPath);
            }
        }

        public static void OpenDocs(TextAsset content)
        {
            var wnd = GetWindow<DocsWindow>();
            wnd.AddDocumentToHistory(content);
            wnd.Show();
            wnd.ParseDocumentation();
        }

        private static void ShowNotFound(string path)
        {
            EditorUtility.DisplayDialog("Documentation Not Found", string.Format("Documentation file {0} was not found.", path), "OK");
        }
        #endregion

        #region Assets

        private const string DocsSkin = "Packages/com.whilefalse.core/Editor/Docs/DocsGUI.guiskin";

        private const string BackIcon = "Packages/com.whilefalse.core/Editor/Docs/icon_back.png";
        private const string ForwardIcon = "Packages/com.whilefalse.core/Editor/Docs/icon_forward.png";
        private const string RefreshIcon = "Packages/com.whilefalse.core/Editor/Docs/icon_reload.png";
        private const string OpenIcon = "Packages/com.whilefalse.core/Editor/Docs/icon_open.png";

        #endregion

        [SerializeField] private List<TextAsset> m_docsHistory = new List<TextAsset>();
        [SerializeField] private int m_currentPageIndex;
        [SerializeField] private List<DocsElement> m_elements = new List<DocsElement>();
        [SerializeField] private Vector2 m_scrollPos;

        private Dictionary<string, Type> m_elementRenderers = new Dictionary<string, Type>();

        private TextAsset currentDocument => m_docsHistory[m_currentPageIndex];

        private TimeSpan m_parseTime;

        private string pagePath => AssetDatabase.GetAssetPath(currentDocument);

        private void AddDocumentToHistory(TextAsset asset)
        {
            m_docsHistory.Add(asset);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Documentation", EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image);

            // Build renderer list
            foreach (var parser in AttributeUtility.GetTypesWithAttribute<MarkdownElementAttribute>())
            {
                foreach (var tag in parser.Item2.tags)
                {
                    m_elementRenderers.Add(tag, parser.Item1);
                }
            }
        }

        private void ParseDocumentation()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            m_elements.Clear();

            string docString = currentDocument.text;
            docString = Markdown.ToHtml(docString);
            XElement doc = XElement.Parse($"<root>{docString}</root>");
            XNode element = doc.FirstNode;
            do
            {
                var e = element as XElement;
                if (e.NodeType == XmlNodeType.Text)
                {
                    Debug.LogError("Pure text detected in output of markdown parser. This is probably a bug.");
                }
                else if (e.NodeType == XmlNodeType.Element)
                {
                    if (m_elementRenderers.ContainsKey(e.Name.LocalName))
                    {
                        var renderType = m_elementRenderers[e.Name.LocalName];
                        var renderer = Activator.CreateInstance(renderType) as DocsElement;
                        renderer.pagePath = Path.GetDirectoryName(pagePath);
                        renderer.Parse(e);
                        m_elements.Add(renderer);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Unknown markdown tag {0}", e.Name.LocalName);
                    }
                }
            }
            while ((element = element.NextNode) != null);

            timer.Stop();
            m_parseTime = timer.Elapsed;
        }

        private void OnGUI()
        {
            DrawTitlebar();
            DrawBody();
            DrawFooter();
        }

        private void DrawTitlebar()
        {
            using (var scope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button(WhileFalseUtility.GetIcon(BackIcon), EditorStyles.toolbarButton))
                {

                }

                if (GUILayout.Button(WhileFalseUtility.GetIcon(ForwardIcon), EditorStyles.toolbarButton))
                {

                }

                EditorGUILayout.LabelField(currentDocument.name);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(WhileFalseUtility.GetIcon(RefreshIcon), EditorStyles.toolbarButton))
                {
                    ParseDocumentation();
                }

                if (GUILayout.Button(WhileFalseUtility.GetIcon(OpenIcon), EditorStyles.toolbarButton))
                {
                    string file = EditorUtility.OpenFilePanelWithFilters("Open documentation file", Application.dataPath, new string[] { "Markdown Files", "md" });
                }
            }            
        }

        private void DrawBody()
        {
            EditorGUILayout.Space();

            var docsSkin = WhileFalseUtility.GetSkin(DocsSkin);

            using (var scope = new EditorGUILayout.ScrollViewScope(m_scrollPos, docsSkin.scrollView))
            {
                m_scrollPos = scope.scrollPosition;

                foreach (var e in m_elements)
                {
                    e.Render(docsSkin);
                }
            }
        }

        private void DrawFooter()
        {
            using (var scope = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Button($"Parsed in {m_parseTime.Milliseconds}ms", EditorStyles.toolbarButton);
                GUILayout.Button($"{m_elements.Count} markdown elements", EditorStyles.toolbarButton);
                GUILayout.Button($"Path: {pagePath}", EditorStyles.toolbarButton);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
