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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeTitleColorAttribute : Attribute
    {
        public Color color;

        public NodeTitleColorAttribute(float _r, float _g, float _b)
        {
            color = new Color(_r, _g, _b, 1);
        }
    }
}
