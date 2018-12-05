using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace WhileFalse.Core
{
    [System.Serializable]
    [MarkdownElement("p", "h1", "h2", "h3", "h4", "h5", "h6")]
    internal class ParagraphElement : DocsElement
    {
        private enum ParagraphStyle
        {
            Normal,
            Image,
            Link,
        }

        [SerializeField] private string m_text;
        [SerializeField] private string m_tag;
        [SerializeField] private string m_url;        
        [SerializeField] private ParagraphStyle m_style = ParagraphStyle.Normal;

        private UnityWebRequest m_imgDownload;
        private UnityWebRequestAsyncOperation m_imgOp;

        public override void Parse(XElement rootElement)
        {
            m_text = rootElement.Value;
            m_tag = rootElement.Name.LocalName;
            if (m_tag == "p") m_tag = string.Empty; //Default styling

            foreach (var tag in rootElement.Descendants())
            {
                if (tag.Name.LocalName == "img")
                {
                    m_style = ParagraphStyle.Image;
                    m_url = tag.Attribute("src").Value;
                    if (m_url.StartsWith("http://") || m_url.StartsWith("https://"))
                    {
                        m_imgDownload = UnityWebRequestTexture.GetTexture(m_url);
                        m_imgOp = m_imgDownload.SendWebRequest();
                    }
                }
                else if (tag.Name.LocalName == "a")
                {
                    m_style = ParagraphStyle.Link;
                    m_url = tag.Attribute("href").Value;
                }
            }
        }

        public override void Render(GUISkin skin)
        {
            var style = skin.label;
            if (!string.IsNullOrEmpty(m_tag))
            {
                style = skin.GetStyle(m_tag);
            }

            switch (m_style)
            {
                case ParagraphStyle.Normal:
                    EditorGUILayout.LabelField(m_text, style);
                    break;
                case ParagraphStyle.Image:
                    DrawImage(skin);
                    break;
                case ParagraphStyle.Link:
                    if (GUILayout.Button(m_text, skin.GetStyle("hyperlink"), GUILayout.ExpandWidth(false)))
                    {

                    }
                    break;
            }
        }

        private void DrawImage(GUISkin skin)
        {
            var imgPath = Path.Combine(pagePath, m_url);
            Texture img = null;
            if (m_imgDownload == null)
            {
                img = AssetDatabase.LoadAssetAtPath<Texture>(imgPath);                
            }
            else
            {
                var handler = m_imgDownload.downloadHandler as DownloadHandlerTexture;
                if (m_imgDownload.isHttpError || m_imgDownload.isNetworkError)
                {
                    EditorGUILayout.LabelField($"Error loading image: {m_imgDownload.error} @ {m_imgDownload.url}");
                }
                else if (!m_imgDownload.isDone)
                {
                    EditorGUILayout.LabelField($"Loading: {m_imgOp.progress * 100}%");
                }
                else
                {
                    img = handler.texture;
                }
            }

            if (img != null)
            {
                float aspect = (float)img.width / (float)img.height;
                var pos = GUILayoutUtility.GetAspectRect(aspect, skin.GetStyle("image"), GUILayout.ExpandWidth(true));
                GUI.DrawTexture(pos, img, ScaleMode.ScaleAndCrop);
            }
        }
    }
}