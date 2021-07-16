#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
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
