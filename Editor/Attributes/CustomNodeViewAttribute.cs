using System;

namespace CZToolKit.GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomNodeViewAttribute : Attribute
    {
        public Type NodeType;

        public CustomNodeViewAttribute(Type _nodeType)
        {
            NodeType = _nodeType;
        }
    }
}