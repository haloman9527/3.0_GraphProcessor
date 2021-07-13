using System;

namespace CZToolKit.GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomNodeViewAttribute : Attribute
    {
        public Type nodeType;

        public CustomNodeViewAttribute(Type _nodeType)
        {
            nodeType = _nodeType;
        }
    }
}