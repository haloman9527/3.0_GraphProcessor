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
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BasePortView : Port, IBindableView<BasePort>
    {
        public Image Icon { get; }
        public BaseGraphView GraphView { get; private set; }
        public BasePort Model { get; private set; }
        public Dictionary<BaseConnection, BaseConnectionView> ConnectionViews { get; private set; }

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
            ConnectionViews = new Dictionary<BaseConnection, BaseConnectionView>();
            this.AddManipulator(m_EdgeConnector);
        }

        public void SetUp(BasePort port, BaseGraphView graphView)
        {
            Model = port;
            GraphView = graphView;

            portName = Model.name;
            tooltip = Model.name;

            Model[nameof(Model.Type)].RegisterValueChangedEvent<Type>(OnPortTypeChanged);
        }

        public void UnBindingProperties()
        {
            Model[nameof(Model.Type)].UnregisterValueChangedEvent<Type>(OnPortTypeChanged);
        }

        private void OnPortTypeChanged(Type newPortType)
        {
            this.portType = newPortType;
        }

        public virtual void Connect(BaseConnectionView connection)
        {
            base.Connect(connection);
            if (connection is BaseConnectionView connectionView)
            {
                ConnectionViews[connectionView.Model] = connectionView;
            }
        }

        public virtual void Disconnect(BaseConnectionView connection)
        {
            base.Disconnect(connection);
            if (connection is BaseConnectionView connectionView)
            {
                ConnectionViews.Remove(connectionView.Model);
            }
        }

        #region 不建议使用
        /// <summary>
        /// 不建议使用
        /// </summary>
        /// <param name="edge"></param>
        public sealed override void Connect(Edge edge)
        {
            base.Connect(edge);
        }

        /// <summary>
        /// 不建议使用
        /// </summary>
        /// <param name="edge"></param>
        public sealed override void Disconnect(Edge edge)
        {
            base.Connect(edge);
        }
        #endregion
    }
}
#endif