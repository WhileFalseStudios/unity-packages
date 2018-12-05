using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace WhileFalse.Core
{
    public abstract class DocsElement
    {
        public string pagePath { get; set; }
        public abstract void Render(GUISkin skin);
        public abstract void Parse(XElement rootElement);
    }
}
