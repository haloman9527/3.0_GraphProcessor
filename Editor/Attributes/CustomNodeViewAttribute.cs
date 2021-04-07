using System;

namespace GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomNodeViewAttribute : Attribute
    {
        public Type NodeType;

        public CustomNodeViewAttribute(Type _nodeType)
        {
            NodeType = _nodeType;
        }
    }
}