#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public class DeleteGraphElementsCommand : Command
    {
        public override string UndoString => "Delete GraphElements";

        //public DeleteGraphElementsCommand(List<IGraphElement> _graphElements)
    }
}