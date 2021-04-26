using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("Util", "Debug")]
    public class DebugNode : BaseNode
    {
        [Port(PortDirection.Input, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [TextArea]
        public string input;

        [Setting()]
        public LayerMask layer;
    }
}