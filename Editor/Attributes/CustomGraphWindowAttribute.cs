using System;

namespace CZToolKit.GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomGraphWindowAttribute : Attribute
    {
        public Type graphType;

        public CustomGraphWindowAttribute(Type _graphType)
        {
            graphType = _graphType;
        }
    }
}