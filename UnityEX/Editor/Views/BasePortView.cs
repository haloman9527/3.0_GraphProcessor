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
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BasePort))]
    public partial class BasePortView
    {
        public BasePortView(BasePortVM port, IEdgeConnectorListener connectorListener) : this(
            orientation: port.Orientation == BasePort.Orientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical,
            direction: port.Direction == BasePort.Direction.Input ? Direction.Input : Direction.Output,
            capacity: port.Capacity == BasePort.Capacity.Single ? Capacity.Single : Capacity.Multi,
            port.Type, connectorListener)
        {

        }
    }
}
#endif