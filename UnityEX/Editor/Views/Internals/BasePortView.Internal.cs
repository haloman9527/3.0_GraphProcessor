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
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    public abstract partial class BasePortView : Port
    {
        public Image Icon { get; }
        public VisualElement CapIconBG { get; }
        public VisualElement CapIcon { get; }
        public Label PortLabel { get; }
        public VisualElement Connector { get; }
        public VisualElement ConnectorCap { get; }
        public BaseGraphView GraphView { get; private set; }
        public PortProcessor ViewModel { get; private set; }
        public Dictionary<BaseConnectionProcessor, BaseConnectionView> ConnectionViews { get; private set; }

        protected BasePortView(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener connectorListener) : base(orientation, direction, capacity, type)
        {
            styleSheets.Add(GraphProcessorEditorStyles.DefaultStyles.BasePortViewStyle);

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

        #region Initialize

        public void SetUp(PortProcessor port, BaseGraphView graphView)
        {
            ViewModel = port;
            GraphView = graphView;
        }

        public void Init()
        {
            this.portName = ViewModel.Name;
            this.tooltip = ViewModel.Name;

            if (ViewModel.HideLabel)
                this.PortLabel.AddToClassList("hidden");

            ViewModel.PropertyChanged += OnViewModelChanged;
            this.DoInit();
        }

        public void UnInit()
        {
            ViewModel.PropertyChanged -= OnViewModelChanged;
            this.DoUnInit();
        }

        #endregion

        #region Callback

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            var port = sender as PortProcessor;
            switch (e.PropertyName)
            {
                case nameof(BasePort.portType):
                {
                    this.portType = port.PortType;
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