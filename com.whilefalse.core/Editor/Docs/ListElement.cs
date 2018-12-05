using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace WhileFalse.Core
{
    [System.Serializable]
    [MarkdownElement("ol", "ul")]
    internal class OrderedListElement : DocsElement
    {
        private enum ListType
        {
            Ordered,
            Unordered,
        }

        [SerializeField] List<string> m_items = new List<string>();
        [SerializeField] ListType m_listType;

        public override void Parse(XElement rootElement)
        {
            var tag = rootElement.Name.LocalName;
            switch (tag)
            {
                case "ul":
                    m_listType = ListType.Unordered;
                    break;
                case "ol":
                    m_listType = ListType.Ordered;
                    break;
            }
            foreach (var child in rootElement.Descendants())
            {
                m_items.Add(child.Value);
            }
        }

        public override void Render(GUISkin skin)
        {
            using (var scope = new EditorGUILayout.VerticalScope())
            {
                for (int i = 0; i < m_items.Count; i++)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    switch (m_listType)
                    {
                        case ListType.Ordered:
                            EditorGUILayout.LabelField((i + 1).ToString(), skin.GetStyle("listNumber"));
                            break;
                        case ListType.Unordered:
                            EditorGUILayout.LabelField("\x2022", skin.GetStyle("listNumber"));
                            break;
                    }
                    EditorGUILayout.LabelField(m_items[i], skin.label);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}
