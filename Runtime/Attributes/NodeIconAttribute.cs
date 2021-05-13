using System;

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NodeIconAttribute : Attribute
    {
        public string iconPath;
        public float width = 30;
        public float height = 30;
        public NodeIconAttribute(string _iconPath) { iconPath = _iconPath; }
    }
}
