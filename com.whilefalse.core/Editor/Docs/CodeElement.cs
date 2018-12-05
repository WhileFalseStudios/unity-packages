using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace WhileFalse.Core
{
    [System.Serializable]
    [MarkdownElement("pre")]
    internal class CodeElement : DocsElement
    {
        [SerializeField] private string m_code;

        public override void Parse(XElement rootElement)
        {
            foreach (var code in rootElement.Descendants())
            {
                if (code.Name.LocalName == "code")
                {
                    m_code = code.Value.TrimEnd();
                }
            }
        }

        public override void Render(GUISkin skin)
        {
            using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.textArea))
            {
                EditorGUILayout.LabelField(m_code, skin.FindStyle("code")); //, GUILayout.Height(m_code.Split('\n').Length * EditorGUIUtility.singleLineHeight)
            }
        }
    }

}