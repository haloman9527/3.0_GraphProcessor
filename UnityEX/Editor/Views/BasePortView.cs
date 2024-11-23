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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;

namespace Jiange.GraphProcessor.Editors
{
    public abstract partial class BasePortView
    {
        public BasePortView(BasePortProcessor port, IEdgeConnectorListener connectorListener) : this(
            orientation: port.Orientation == BasePort.Orientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical,
            direction: port.Direction == BasePort.Direction.Left ? Direction.Input : Direction.Output,
            capacity: port.Capacity == BasePort.Capacity.Single ? Capacity.Single : Capacity.Multi,
            port.Type, connectorListener)
        {
        }
    }
}
#endif