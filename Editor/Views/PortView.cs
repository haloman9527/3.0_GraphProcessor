using CZToolKit.Core;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor.Editors
{
    public class PortView : Port, IPortView
    {
        public static PortView CreatePV(Orientation _orientation, Direction _direction, NodePort _portData)
        {
            var portView = new PortView(_orientation, _direction, _portData);

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

        public static PortView CreatePV(Orientation _orientation, Direction _direction, NodePort _portData, Type _displayType)
        {
            var portView = new PortView(_orientation, _direction, _portData, _displayType);

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
        public Port Self { get { return this; } }
        public BaseGraphView GraphView { get; private set; }
        public NodePort PortData { get; private set; }
        public string FieldName { get { return PortData.FieldName; } }

        public PortTypeConstraint TypeConstraint { get { return PortData.TypeConstraint; } }

        public Type DisplayType { get { return PortData.DisplayType; } }

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

        PortView(Orientation _orientation, Direction _direction, NodePort _portData)
            : base(_orientation, _direction, _portData.IsMulti ? Capacity.Multi : Capacity.Single, _portData.DisplayType)
        {
            styleSheets.Add(GraphProcessorStyles.PortViewStyle);

            PortData = _portData;
            portName = PortData.FieldName;

            if (_orientation == Orientation.Vertical)
                AddToClassList("Vertical");

            UpdatePortSize();
        }

        PortView(Orientation _orientation, Direction _direction, NodePort _portData, Type _displayType)
            : base(_orientation, _direction, _portData.IsMulti ? Capacity.Multi : Capacity.Single, _displayType)
        {
            PortData = _portData;
            portName = PortData.FieldName;

            if (_orientation == Orientation.Vertical)
                AddToClassList("Vertical");

            UpdatePortSize();
        }

        public void SetUp(IGraphElement _graphElement, CommandDispatcher _commandDispatcher, IGraphView _graphView)
        {
            GraphView = _graphView as BaseGraphView;

            m_EdgeConnector = new BaseEdgeConnector(GraphView, new BaseEdgeConnectorListener(GraphView));
            this.AddManipulator(m_EdgeConnector);

            if (Utility_Attribute.TryGetFieldAttribute(PortData.Owner.GetType(), FieldName, out PortColorAttribute colorAttrib))
                portColor = colorAttrib.Color;

            if (Utility_Attribute.TryGetFieldAttribute(PortData.Owner.GetType(), FieldName, out TooltipAttribute toolTipAttrib))
                tooltip = toolTipAttrib.tooltip;
            else if (orientation == Orientation.Vertical)
                tooltip = NodeEditorUtility.GetDisplayName(FieldName);

            if (Utility_Attribute.TryGetFieldAttribute(PortData.Owner.GetType(), FieldName, out InspectorNameAttribute attrib))
                portName = attrib.displayName;
            else
                portName = NodeEditorUtility.GetDisplayName(FieldName);

            AddToClassList(FieldName);
            visualClass = "Port_" + portType.Name;
            if (GraphView.Initialized)
                OnInitialized();
            else
                GraphView.onInitializeCompleted += OnInitialized;
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

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            onConnected?.Invoke();
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            if (!(edge as EdgeView).isConnected)
                return;
            onDisconnected?.Invoke();
        }

        public void UpdatePortView()
        {
            if (PortData == null) return;

            if (PortData.DisplayType != null)
            {
                portType = PortData.DisplayType;
                visualClass = "Port_" + portType.Name;
            }

            if (Utility_Attribute.TryGetFieldAttribute(PortData.Owner.GetType(), FieldName, out InspectorNameAttribute attrib))
                portName = attrib.displayName;
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
            if (Utility_Attribute.TryGetFieldAttribute(PortData.Owner.GetType(), PortData.FieldName, out PortSizeAttribute portSizeAttribute))
                size = portSizeAttribute.size;
            else
                size = GraphProcessorStyles.DefaultPortSize;

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