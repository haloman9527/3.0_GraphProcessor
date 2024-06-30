
#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BasePort))]
    public class DefaultPortView : BasePortView
    {
        protected DefaultPortView(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener connectorListener) : base(orientation, direction, capacity, type, connectorListener)
        {
        }

        public DefaultPortView(BasePortProcessor port, IEdgeConnectorListener connectorListener) : base(port, connectorListener)
        {
        }
    }
}
#endif