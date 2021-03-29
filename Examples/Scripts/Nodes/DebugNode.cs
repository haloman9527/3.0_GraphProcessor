using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
    [NodeMenuItem("Util", "Debug")]
    public class DebugNode : BaseNode
    {
        [Port(PortDirection.Input, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        public object input;
    }
}