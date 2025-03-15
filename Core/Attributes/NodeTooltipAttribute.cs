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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */
#endregion
using System;

namespace Atom.GraphProcessor
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
