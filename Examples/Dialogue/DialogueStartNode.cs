using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor.Dialogue
{
    [NodeMenuItem("Dialogue", "Start", ShowInList = false)]
    public class DialogueStartNode : BaseNode
    {
        [PortColor(0,1,0)]
        [Port(PortDirection.Output, IsMulti = true, TypeConstraint = PortTypeConstraint.Inherited, DisplayName = "")]
        private BaseDialogueNode start;

        public void Start()
        {
            ExecuteConnections("start");
        }
    }
}