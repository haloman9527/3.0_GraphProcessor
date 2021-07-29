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
using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Util", "Debug.Log")]
    public partial class DebugNode : BaseNode
    {
        [Input(IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [TextArea]
        [SerializeField] string input;
    }
}