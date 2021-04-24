using System;

namespace GraphProcessor
{
    /// <summary> 节点菜单，和自定义节点名 </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeMenuItemAttribute : Attribute
    {
        /// <summary> 节点菜单路径 </summary>
        public string[] Titles;
        /// <summary> 是否要显示在节点菜单中 </summary>
        public bool ShowInList = true;

        public NodeMenuItemAttribute(params string[] _titles)
        {
            Titles = _titles;
        }
    }
}