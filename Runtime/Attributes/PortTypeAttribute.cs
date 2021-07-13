using System;

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PortTypeAttribute : Attribute
    {
        public Type portType;

        public PortTypeAttribute(Type _portType)
        {
            portType = _portType;
        }
    }
}
