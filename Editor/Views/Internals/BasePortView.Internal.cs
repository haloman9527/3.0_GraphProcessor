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
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BasePortView : Port
    {
        public Image Icon { get; }
        public BaseGraphView GraphView { get; private set; }
        public BasePort Model { get; private set; }

        protected BasePortView(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener connectorListener) : base(orientation, direction, capacity, type)
        {
            styleSheets.Add(GraphProcessorStyles.PortViewStyle);

            Icon = new Image();
            Icon.AddToClassList("port-icon");
            Insert(1, Icon);

            visualClass = "Port_" + portType.Name;

            var portLabel = this.Q("type");
            portLabel.pickingMode = PickingMode.Position;
            portLabel.style.flexGrow = 1;
            bool vertical = orientation == Orientation.Vertical;
            if (vertical)
            {
                portLabel.style.display = DisplayStyle.None;
                this.Q("connector").pickingMode = PickingMode.Position;
                AddToClassList("vertical");
            }

            m_EdgeConnector = new EdgeConnector<BaseConnectionView>(connectorListener);
            this.AddManipulator(m_EdgeConnector);
        }

        public void SetUp(BasePort port, BaseGraphView graphView)
        {
            Model = port;
            GraphView = graphView;

            portName = port.name;
            tooltip = port.name;
        }
    }
}
#endif