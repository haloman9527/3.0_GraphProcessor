#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public class CreateEdgeCommand : Command
    {
        public override string UndoString => "Create Edge";

        public CreateEdgeCommand(NodePort _input, NodePort _output)
        {

        }
    }
}
