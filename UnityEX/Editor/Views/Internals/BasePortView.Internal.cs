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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CZToolKit;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract partial class BasePortView : Port, IGraphElementView<BasePortProcessor>
    {
        public Image Icon { get; }
        public VisualElement CapIconBG { get; }
        public VisualElement CapIcon { get; }
        public Label PortLabel { get; }
        public VisualElement Connector { get; }
        public VisualElement ConnectorCap { get; }
        public BaseGraphView GraphView { get; private set; }
        public BasePortProcessor ViewModel { get; private set; }
        public Dictionary<BaseConnectionProcessor, BaseConnectionView> ConnectionViews { get; private set; }

        protected BasePortView(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener connectorListener) : base(orientation, direction, capacity, type)
        {
            styleSheets.Add(GraphProcessorStyles.BasePortViewStyle);

            visualClass = "port-" + portType.Name;
            this.AddToClassList("capacity-" + capacity.ToString());

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
            ConnectionViews = new Dictionary<BaseConnectionProcessor, BaseConnectionView>();
            this.AddManipulator(m_EdgeConnector);
        }

        public void SetUp(BasePortProcessor port, BaseGraphView graphView)
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
            ViewModel.PropertyChanged += OnViewModelChanged;

            OnBindingProperties();
        }

        public void OnDestroy()
        {
            ViewModel.PropertyChanged -= OnViewModelChanged;

            OnUnBindingProperties();
        }

        #region Callback

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            var port = sender as BasePortProcessor;
            switch (e.PropertyName)
            {
                case nameof(BasePort.type):
                {
                    this.portType = port.Type;
                    break;
                }
                case "hideLabel":
                {
                    if (port.HideLabel)
                        PortLabel.AddToClassList("hidden");
                    else
                        PortLabel.RemoveFromClassList("hidden");
                    break;
                }
            }
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

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnBindingProperties()
        {
        }

        protected virtual void OnUnBindingProperties()
        {
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