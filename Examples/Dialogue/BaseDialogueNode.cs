using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor.Dialogue
{
    public abstract class BaseDialogueNode : BaseNode
    {
        [PortColor(1, 0.9f, 0)]
        [Port(PortDirection.Input, IsMulti = true, TypeConstraint = PortTypeConstraint.Inherited)]
        public BaseDialogueNode enter;
    }
}