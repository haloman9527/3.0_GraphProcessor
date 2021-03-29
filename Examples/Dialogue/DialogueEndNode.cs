using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor.Dialogue
{
    [NodeMenuItem("Dialogue", "End")]
    public class DialogueEndNode : BaseDialogueNode
    {
        public override void Execute(NodePort _port, params object[] _params)
        {
            if (_port.FieldName == nameof(enter))
            {

            }
        }
    }
}
