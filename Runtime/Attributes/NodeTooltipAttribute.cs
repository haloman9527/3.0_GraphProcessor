using System;

namespace CZToolKit.GraphProcessor
{
    public class NodeTooltipAttribute : Attribute
    {
        public string Tooltip;
        public NodeTooltipAttribute(string _tooltip)
        {
            Tooltip = _tooltip;
        }
    }
}
