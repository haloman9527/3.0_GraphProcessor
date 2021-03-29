using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
