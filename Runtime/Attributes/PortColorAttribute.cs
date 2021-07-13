using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PortColorAttribute : Attribute
    {
        public Color Color;

        public PortColorAttribute(float r, float g, float b)
        {
            Color = new Color(r, g, b);
        }
    }
}
