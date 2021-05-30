using CZToolKit.Core;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor.Editors
{
    public class PortView : Port
    {
        const int DefaultPortSize = 8;

        const string PortViewStyleFile = "GraphProcessor/Styles/PortView";

        static StyleSheet portViewStyle;

        public static StyleSheet PortViewStyle
        {
            get
            {
                if (portViewStyle == null)
                    portViewStyle = Resources.Load<StyleSheet>(PortViewStyleFile);
                return portViewStyle;
            }
        }

        public static PortView CreatePV(Orientation _orientation, Direction _direction, NodePort _portData, BaseEdgeConnectorListener _edgeConnectorListener)
        {
            var portView = new PortView(_orientation, _direction, _portData, _edgeConnectorListener);

            portView.m_EdgeConnector = new BaseEdgeConnector(_edgeConnectorListener);
            portView.AddManipulator(portView.m_EdgeConnector);
            var portLabel = portView.Q("type");
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;
                portLabel.style.flexGrow = 1;
            }
            bool vertical = _orientation == Orientation.Vertical;

            if (vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            if (vertical)
                portView.Q("connector").pickingMode = PickingMode.Position;

            return portView;
        }

        public static PortView CreatePV(Orientation _orientation, Direction _direction, NodePort _portData, Type _displayType, BaseEdgeConnectorListener _edgeConnectorListener)
        {
            var portView = new PortView(_orientation, _direction, _portData, _displayType, _edgeConnectorListener);

            portView.m_EdgeConnector = new BaseEdgeConnector(_edgeConnectorListener);
            portView.AddManipulator(portView.m_EdgeConnector);

            var portLabel = portView.Q("type");
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;
                portLabel.style.flexGrow = 1;
            }
            bool vertical = _orientation == Orientation.Vertical;

            if (vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            if (vertical)
                portView.Q("connector").pickingMode = PickingMode.Position;

            return portView;
        }

        public int size;
        public BaseNodeView Owner { get; private set; }
        public NodePort PortData { get; private set; }
        public string FieldName { get { return PortData.FieldName; } }
        protected BaseEdgeConnectorListener Listener { get; set; }
        public PortView Connection
        {
            get
            {
                foreach (var edgeView in connections)
                {
                    return (direction == Direction.Input ? edgeView.output : edgeView.input) as PortView;
                }
                return null;
            }
        }

        public Action onConnected, onDisconnected;

        PortView(Orientation _orientation, Direction _direction, NodePort _portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(_orientation, _direction, _portData.IsMulti ? Capacity.Multi : Capacity.Single, _portData.DisplayType)
        {
            PortData = _portData;
            Listener = edgeConnectorListener;
            portName = PortData.FieldName;

            if (_orientation == Orientation.Vertical)
                AddToClassList("Vertical");

            UpdatePortSize();
        }

        PortView(Orientation _orientation, Direction _direction, NodePort _portData, Type _displayType, BaseEdgeConnectorListener edgeConnectorListener)
            : base(_orientation, _direction, _portData.IsMulti ? Capacity.Multi : Capacity.Single, _displayType)
        {
            PortData = _portData;
            Listener = edgeConnectorListener;
            portName = PortData.FieldName;

            if (_orientation == Orientation.Vertical)
                AddToClassList("Vertical");

            UpdatePortSize();
        }

        public void Initialize(BaseNodeView _nodeView)
        {
            styleSheets.Add(PortViewStyle);

            Owner = _nodeView;

            if (Utility.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out PortColorAttribute colorAttrib))
                portColor = colorAttrib.Color;

            if (Utility.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out TooltipAttribute toolTipAttrib))
                tooltip = toolTipAttrib.tooltip;
            else if (orientation == Orientation.Vertical)
                tooltip = NodeEditorUtility.GetDisplayName(FieldName);

            if (Utility.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out DisplayNameAttribute attrib))
                portName = attrib.DisplayName;
            else
                portName = NodeEditorUtility.GetDisplayName(FieldName);

            AddToClassList(FieldName);
            visualClass = "Port_" + portType.Name;
            if (Owner.Owner.Initialized)
                OnInitialized();
            else
                Owner.Owner.OnInitializeCompleted += OnInitialized;
        }

        protected virtual void OnInitialized() { }

        #region API
        public IEnumerable<PortView> GetConnections()
        {
            if (direction == Direction.Input)
            {
                foreach (Edge edgeView in connections)
                {
                    yield return edgeView.output as PortView;
                }
            }
            else
            {
                foreach (Edge edgeView in connections)
                {
                    yield return edgeView.input as PortView;
                }
            }
        }

        public bool TryGetValue<T>(ref T _value)
        {
            return Owner.GetValue(this, ref _value);
        }

        public void Execute(params object[] _params)
        {
            Owner.Execute(this, _params);
        }

        public bool TryGetConnectValue<T>(ref T _value)
        {
            PortView portView = Connection;
            if (portView == null) return false;
            return portView.TryGetValue(ref _value);
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            onConnected?.Invoke();
            switch (direction)
            {
                case Direction.Input:
                    var outputNode = (edge.output as PortView).Owner;
                    outputNode.OnPortConnected(edge.output as PortView, edge.input as PortView);
                    break;
                case Direction.Output:
                    var inputNode = (edge.input as PortView).Owner;
                    inputNode.OnPortConnected(edge.input as PortView, edge.output as PortView);
                    break;
            }
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            if (!(edge as EdgeView).isConnected)
                return;
            onDisconnected?.Invoke();
            switch (direction)
            {
                case Direction.Input:
                    var outputNode = (edge.output as PortView).Owner;
                    outputNode.OnPortDisconnected(edge.output as PortView, edge.input as PortView);
                    break;
                case Direction.Output:
                    var inputNode = (edge.input as PortView).Owner;
                    inputNode.OnPortDisconnected(edge.input as PortView, edge.output as PortView);
                    break;
            }
        }

        public void UpdatePortView()
        {
            if (PortData == null) return;

            if (PortData.DisplayType != null)
            {
                portType = PortData.DisplayType;
                visualClass = "Port_" + portType.Name;
            }

            if (Utility.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out DisplayNameAttribute attrib))
                portName = attrib.DisplayName;
            else
                portName = NodeEditorUtility.GetDisplayName(FieldName);

            // Update the edge in case the port color have changed
            schedule.Execute(() =>
            {
                foreach (var edge in connections)
                {
                    edge.UpdateEdgeControl();
                    edge.MarkDirtyRepaint();
                }
            }).ExecuteLater(50); // Hummm

            UpdatePortSize();
        }
        #endregion

        public void UpdatePortSize()
        {
            if (Utility.TryGetFieldAttribute(PortData.Owner.GetType(), PortData.FieldName, out PortSizeAttribute portSizeAttribute))
                size = portSizeAttribute.size;
            else
                size = DefaultPortSize;

            var connector = this.Q("connector");
            var cap = connector.Q("cap");
            connector.style.width = size;
            connector.style.height = size;
            cap.style.width = size - 4;
            cap.style.height = size - 4;

            // Update connected edge sizes:
            foreach (EdgeView edgeView in connections)
            {
                edgeView.UpdateEdgeSize();
            }
        }
    }
}