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
    public partial class BasePortView : Port, IBindableView<BasePortVM>
    {
        public Image Icon { get; }
        public BaseGraphView GraphView { get; private set; }
        public BasePortVM ViewModel { get; private set; }
        public Dictionary<BaseConnectionVM, BaseConnectionView> ConnectionViews { get; private set; }

        protected BasePortView(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener connectorListener) : base(orientation, direction, capacity, type)
        {
            styleSheets.Add(GraphProcessorStyles.PortViewStyle);
            styleSheets.Add(GraphProcessorStyles.PortViewTypesStyle);

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
            ConnectionViews = new Dictionary<BaseConnectionVM, BaseConnectionView>();
            this.AddManipulator(m_EdgeConnector);
        }

        public void SetUp(BasePortVM port, BaseGraphView graphView)
        {
            ViewModel = port;
            GraphView = graphView;

            portName = ViewModel.Name;
            tooltip = ViewModel.Name;

            OnInitialized();
        }

        public void BindingProperties()
        {
            ViewModel[nameof(BasePort.type)].RegisterValueChangedEvent<Type>(OnPortTypeChanged);

            OnBindingProperties();
        }

        public void UnBindingProperties()
        {
            ViewModel[nameof(BasePort.type)].UnregisterValueChangedEvent<Type>(OnPortTypeChanged);

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