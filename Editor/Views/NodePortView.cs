using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace CZToolKit.GraphProcessor.Editors
{
    public class NodePortView : Port
    {
        public static NodePortView CreatePV(Orientation _orientation, Direction _direction, NodePort _viewModel)
        {
            return new NodePortView(_orientation, _direction, _viewModel);
        }

        public static NodePortView CreatePV(Orientation _orientation, Direction _direction, NodePort _viewModel, Type _displayType)
        {
            return new NodePortView(_orientation, _direction, _viewModel, _displayType);
        }

        public Action onConnected;
        public Action onDisconnected;

        public Image Icon { get; }
        public BaseGraphView GraphView { get; private set; }
        public NodePort ViewModel { get; private set; }

        protected NodePortView(Orientation _orientation, Direction _direction, NodePort _nodePort, Type _displayType)
            : base(_orientation, _direction, _nodePort.Multiple ? Capacity.Multi : Capacity.Single, _displayType)
        {
            styleSheets.Add(GraphProcessorStyles.PortViewStyle);
            Icon = new Image();
            Icon.AddToClassList("port-icon");
            Insert(1, Icon);
            portName = _nodePort.FieldName;

            var portLabel = this.Q("type");
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;
                portLabel.style.flexGrow = 1;
            }
            bool vertical = _orientation == Orientation.Vertical;

            if (vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            if (vertical)
                this.Q("connector").pickingMode = PickingMode.Position;

            if (_orientation == Orientation.Vertical)
                AddToClassList("vertical");
        }

        protected NodePortView(Orientation _orientation, Direction _direction, NodePort _nodePort)
            : this(_orientation, _direction, _nodePort, _nodePort.DisplayType) { }

        public void SetUp(NodePort _port, CommandDispatcher _commandDispatcher, BaseGraphView _graphView)
        {
            GraphView = _graphView;

            ViewModel = _port;
            BindingProperties();
            ViewModel.UpdateProperties();

            ViewModel.RegisterValueChangedEvent<Color>(nameof(ViewModel.PortColor), v =>
            {
                portColor = v;
            });
            if (orientation == Orientation.Vertical && string.IsNullOrEmpty(ViewModel.Tooltip))
                ViewModel.Tooltip = GraphProcessorEditorUtility.GetDisplayName(ViewModel.FieldName);


            m_EdgeConnector = new EdgeConnector<BaseEdgeView>(new EdgeConnectorListener(GraphView));
            this.AddManipulator(m_EdgeConnector);

            AddToClassList(ViewModel.FieldName);
            visualClass = "Port_" + portType.Name;
        }

        public virtual void BindingProperties()
        {
            ViewModel.RegisterValueChangedEvent<string>(nameof(ViewModel.PortName), v =>
            {
                portName = v;
            });
            ViewModel.RegisterValueChangedEvent<string>(nameof(ViewModel.Tooltip), v =>
            {
                tooltip = v;
            });
        }

        #region API
        public override void Connect(Edge _edge)
        {
            base.Connect(_edge);
            onConnected?.Invoke();
        }

        public override void Disconnect(Edge _edge)
        {
            base.Disconnect(_edge);
            onDisconnected?.Invoke();
        }
        #endregion
    }
}