using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MarkdownElementAttribute : Attribute
    {
        internal string[] tags { get; }

        public MarkdownElementAttribute(params string[] htmlTags)
        {
            tags = htmlTags;
        }
    }
}
