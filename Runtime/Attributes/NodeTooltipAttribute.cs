using System;

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeTooltipAttribute : Attribute
    {
        public string Tooltip;
        public NodeTooltipAttribute(string _tooltip)
        {
            Tooltip = _tooltip;
        }
    }
}
