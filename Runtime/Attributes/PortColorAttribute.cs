using System;
using UnityEngine;

namespace GraphProcessor
{
    public class PortColorAttribute : Attribute
    {
        public Color Color;
        public PortColorAttribute(float r, float g, float b)
        {
            Color = new Color(r, g, b);
        }
    }
}
