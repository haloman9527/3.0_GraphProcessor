using System;

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PortIconAttribute : Attribute
    {
        public string iconPath;

        public PortIconAttribute(string _iconPath)
        {
            iconPath = _iconPath;
        }
    }
}
