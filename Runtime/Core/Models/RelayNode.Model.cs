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

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Utils", "Relay")]
    public partial class RelayNode : BaseNode
    {
        [Port(PortDirection.Input, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [PortColor(0, 0.7f, 0.3f)]
        public object input;

        [Port(PortDirection.Output, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [PortColor(0, 0.7f, 0.3f)]
        public object output;
    }
}