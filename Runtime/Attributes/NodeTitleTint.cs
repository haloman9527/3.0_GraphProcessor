using System;
using UnityEngine;

namespace GraphProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NodeTitleTintAttribute : Attribute
    {
        public Color BackgroundColor;

        public NodeTitleTintAttribute(float _r, float _g, float _b)
        {
            BackgroundColor = new Color(_r, _g, _b, 1);
        }
    }
}
