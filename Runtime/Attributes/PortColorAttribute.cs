using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
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
