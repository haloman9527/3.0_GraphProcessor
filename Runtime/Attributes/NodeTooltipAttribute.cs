using System;

namespace GraphProcessor
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
