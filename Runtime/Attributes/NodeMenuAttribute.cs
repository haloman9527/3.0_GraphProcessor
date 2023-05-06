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
    /// <summary> 节点菜单，和自定义节点名 </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeMenuAttribute : Attribute
    {
        /// <summary> 节点路径 </summary>
        public string path;
        /// <summary> 是否要显示在节点菜单中 </summary>
        public bool hidden;

        public NodeMenuAttribute(string path)
        {
            this.path = path;
        }
    }
}