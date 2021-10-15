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
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BasePortView
    {
        public BasePortView(BaseSlot slot, Type portType, IEdgeConnectorListener connectorListener) : this(
            orientation: slot.orientation == BaseSlot.Orientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical,
            direction: slot.direction == BaseSlot.Direction.Input ? Direction.Input : Direction.Output,
            capacity: slot.capacity == BaseSlot.Capacity.Single ? Capacity.Single : Capacity.Multi,
            portType, connectorListener)
        {

        }
    }
}