using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace CZToolKit.GraphProcessor.Editors
{
    public sealed class NodePortView : Port, IBindableView<NodePort>
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
        public NodePort Model { get; private set; }

        NodePortView(Orientation _orientation, Direction _direction, NodePort _nodePort, Type _displayType)
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

        NodePortView(Orientation _orientation, Direction _direction, NodePort _nodePort)
            : this(_orientation, _direction, _nodePort, _nodePort.DisplayType) { }

        public void SetUp(NodePort _port, CommandDispatcher _commandDispatcher, BaseGraphView _graphView)
        {
            Model = _port;
            GraphView = _graphView;

            m_EdgeConnector = new EdgeConnector<BaseEdgeView>(new EdgeConnectorListener(GraphView));
            this.AddManipulator(m_EdgeConnector);

            AddToClassList(Model.FieldName);
            visualClass = "Port_" + portType.Name;

            // 初始化
            portName = Model.PortName;
            tooltip = Model.Tooltip;
            if (orientation == Orientation.Vertical && string.IsNullOrEmpty(Model.Tooltip))
                Model.Tooltip = GraphProcessorEditorUtility.GetDisplayName(Model.FieldName);

            // 绑定
            BindingPropertiesBeforeUpdate();
        }
        #region 数据监听回调
        void OnPortNameChanged(string _name)
        {
            portName = _name;
        }
        void OnToolTipChanged(string _tooltip)
        {
            tooltip = _tooltip;
        }
        void OnColorChanged(Color _color)
        {
            portColor = _color;
        }
        void BindingPropertiesBeforeUpdate()
        {
            Model.RegisterValueChangedEvent<string>(nameof(Model.PortName), OnPortNameChanged);
            Model.RegisterValueChangedEvent<string>(nameof(Model.Tooltip), OnToolTipChanged);
            Model.RegisterValueChangedEvent<Color>(nameof(Model.PortColor), OnColorChanged);
        }

        public void UnBindingProperties()
        {
            Model.UnregisterValueChangedEvent<string>(nameof(Model.PortName), OnPortNameChanged);
            Model.UnregisterValueChangedEvent<string>(nameof(Model.Tooltip), OnToolTipChanged);
            Model.UnregisterValueChangedEvent<Color>(nameof(Model.PortColor), OnColorChanged);
        }
        #endregion

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