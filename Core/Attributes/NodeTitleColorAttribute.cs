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

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeTitleColorAttribute : Attribute
    {
        public readonly InternalColor color;

        public NodeTitleColorAttribute(float r, float g, float b)
        {
            color = new InternalColor(r, g, b, 1);
        }
    }
}
