using System;

namespace GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomNodeEditorAttribute : Attribute
    {
        public Type NodeType;

        public CustomNodeEditorAttribute(Type _nodeType)
        {
            NodeType = _nodeType;
        }
    }
}