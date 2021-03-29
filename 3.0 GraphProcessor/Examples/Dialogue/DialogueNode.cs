using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor.Dialogue
{
    [NodeMenuItem("Dialogue", "Dialogue")]
    public class DialogueNode : BaseDialogueNode
    {
        [PortColor(1,0.6f,0)]
        [Port(PortDirection.Output, IsMulti = true, TypeConstraint = PortTypeConstraint.Inherited)]
        public BaseDialogueNode next;

        [TextArea()]
        public string text;

        public override void Execute(NodePort _port, params object[] _params)
        {
            if (_port.FieldName == nameof(enter))
            {

            }
        }
    }
}