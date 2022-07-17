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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeTooltipAttribute : Attribute
    {
        public readonly string Tooltip;
        public NodeTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}
