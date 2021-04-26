using System;

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NodeIconAttribute : Attribute
    {
        public string IconFolder;

        public NodeIconAttribute(string _iconPath) { IconFolder = _iconPath; }
    }
}
