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
    public partial class BasePortView : Port, IGraphElementView<BasePortVM>
    {
        public Image Icon { get; }
        public VisualElement CapIconBG { get; }
        public VisualElement CapIcon { get; }
        public Label PortLabel { get; }
        public VisualElement Connector { get; }
        public VisualElement ConnectorCap { get; }
        public BaseGraphView GraphView { get; private set; }
        public BasePortVM ViewModel { get; private set; }
        public Dictionary<BaseConnectionVM, BaseConnectionView> ConnectionViews { get; private set; }

        protected BasePortView(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener connectorListener) : base(orientation, direction, capacity, type)
        {
            styleSheets.Add(GraphProcessorStyles.PortViewStyle);
            styleSheets.Add(GraphProcessorStyles.PortViewTypesStyle);
            
            visualClass = "port_" + portType.Name;
            this.AddToClassList("capacity_" + capacity.ToString());
            
            PortLabel = this.Q("type") as Label;
            Connector = this.Q("connector");
            ConnectorCap = Connector.Q("connectorCap");

            Icon = new Image();
            Icon.AddToClassList("port-icon");
            Insert(1, Icon);

            CapIconBG = new VisualElement();
            CapIconBG.name = "cap-icon-bg";
            Connector.Add(CapIconBG);
            
            CapIcon = new VisualElement();
            CapIcon.name = "cap-icon";
            CapIcon.pickingMode = PickingMode.Ignore;
            Connector.Add(CapIcon);


            PortLabel.pickingMode = PickingMode.Position;
            Connector.pickingMode = PickingMode.Position;
            bool vertical = orientation == Orientation.Vertical;
            if (vertical)
            {
                PortLabel.style.display = DisplayStyle.None;
                AddToClassList("vertical");
            }

            m_EdgeConnector = new EdgeConnector<BaseConnectionView>(connectorListener);
            ConnectionViews = new Dictionary<BaseConnectionVM, BaseConnectionView>();
            this.AddManipulator(m_EdgeConnector);
        }

        public void SetUp(BasePortVM port, BaseGraphView graphView)
        {
            ViewModel = port;
            GraphView = graphView;

            portName = ViewModel.Name;
            tooltip = ViewModel.Name;

            if (ViewModel.HideLabel)
                PortLabel.AddToClassList("hidden");

            OnInitialized();
        }

        public void OnCreate()
        {
            ViewModel[nameof(BasePort.type)].AsBindableProperty<Type>().RegisterValueChangedEvent(OnPortTypeChanged);

            OnBindingProperties();
        }

        public void OnDestroy()
        {
            ViewModel[nameof(BasePort.type)].AsBindableProperty<Type>().UnregisterValueChangedEvent(OnPortTypeChanged);

            OnUnBindingProperties();
        }

        #region Callback
        private void OnPortTypeChanged(Type newPortType)
        {
            this.portType = newPortType;
        }
        #endregion

        public void Connect(BaseConnectionView connection)
        {
            base.Connect(connection);
            if (connection is BaseConnectionView connectionView)
            {
                ConnectionViews[connectionView.ViewModel] = connectionView;
            }
        }

        public void Disconnect(BaseConnectionView connection)
        {
            base.Disconnect(connection);
            if (connection is BaseConnectionView connectionView)
            {
                ConnectionViews.Remove(connectionView.ViewModel);
            }
        }

        protected virtual void OnInitialized() { }

        protected virtual void OnBindingProperties() { }

        protected virtual void OnUnBindingProperties() { }

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