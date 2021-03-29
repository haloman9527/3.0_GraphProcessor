using System;

namespace GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomGraphWindowAttribute : Attribute
    {
        public Type GraphType;

        public CustomGraphWindowAttribute(Type _graphType)
        {
            GraphType = _graphType;
        }
    }
}