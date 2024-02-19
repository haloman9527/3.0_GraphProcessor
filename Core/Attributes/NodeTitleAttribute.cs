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

namespace CZToolKit.GraphProcessor
{
    /// <summary> 节点标题 </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NodeTitleAttribute : Attribute
    {
        /// <summary> 节点标题名称 </summary>
        public string title;

        public NodeTitleAttribute(string title)
        {
            this.title = title;
        }
    }
}