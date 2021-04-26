using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using CZToolKit.Core;

namespace CZToolKit.GraphProcessor.Editors
{
    public class PortView : Port
    {
        const int DefaultPortSize = 8;

        const string PortViewStyleFile = "GraphProcessor/Styles/PortView";
        const string PortViewTypesFile = "GraphProcessor/Styles/PortViewTypes";

        static StyleSheet portViewStyle;
        static StyleSheet portViewTypesStyle;

        public static StyleSheet PortViewStyle
        {
            get
            {
                if (portViewStyle == null)
                    portViewStyle = Resources.Load<StyleSheet>(PortViewStyleFile);
                return portViewStyle;
            }
        }
        public static StyleSheet PortViewTypesStyle
        {
            get
            {
                if (portViewTypesStyle == null)
                    portViewTypesStyle = Resources.Load<StyleSheet>(PortViewTypesFile);
                return portViewTypesStyle;
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
        NodePort portData;
        protected BaseEdgeConnectorListener listener;

        public BaseNodeView Owner { get; private set; }
        public NodePort PortData { get { return portData; } }
        public string FieldName { get { return portData.FieldName; } }
        public List<EdgeView> Edges { get;  } = new List<EdgeView>();

        PortView(Orientation _orientation, Direction _direction, NodePort _portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(_orientation, _direction, _portData.IsMulti ? Capacity.Multi : Capacity.Single, _portData.DisplayType)
        {
            portData = _portData;
            listener = edgeConnectorListener;

            if (_orientation == Orientation.Vertical)
                AddToClassList("Vertical");

            UpdatePortSize();
        }

        PortView(Orientation _orientation, Direction _direction, NodePort _portData, Type _displayType, BaseEdgeConnectorListener edgeConnectorListener)
            : base(_orientation, _direction, _portData.IsMulti ? Capacity.Multi : Capacity.Single, _displayType)
        {
            portData = _portData;
            listener = edgeConnectorListener;

            if (_orientation == Orientation.Vertical)
                AddToClassList("Vertical");

            UpdatePortSize();
        }

        public virtual void Initialize(BaseNodeView _nodeView)
        {
            styleSheets.Add(PortViewStyle);
            styleSheets.Add(PortViewTypesStyle);

            Owner = _nodeView;

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out PortColorAttribute colorAttrib))
                portColor = colorAttrib.Color;

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out TooltipAttribute toolTipAttrib))
                tooltip = toolTipAttrib.tooltip;
            else if (orientation == Orientation.Vertical)
                tooltip = NodeEditorUtility.GetDisplayName(FieldName);

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out DisplayNameAttribute attrib))
                portName = attrib.DisplayName;
            else
                portName = NodeEditorUtility.GetDisplayName(FieldName);

            AddToClassList(FieldName);
            visualClass = "Port_" + portType.Name;
        }

        /// <summary> Update the size of the port view (using the portData.sizeInPixel property) </summary>
        public void UpdatePortSize()
        {
            if (AttributeCache.TryGetFieldAttribute(PortData.Owner.GetType(), PortData.FieldName, out PortSizeAttribute portSizeAttribute))
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
            Edges.ForEach(e => e.UpdateEdgeSize());
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            Edges.Add(edge as EdgeView);
            var inputNode = (edge.input as PortView).Owner;
            var outputNode = (edge.output as PortView).Owner;
            switch (direction)
            {
                case Direction.Input:
                    outputNode.OnPortConnected(edge.output as PortView, edge.input as PortView);
                    break;
                case Direction.Output:
                    inputNode.OnPortConnected(edge.input as PortView, edge.output as PortView);
                    break;
            }
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            if (!(edge as EdgeView).isConnected)
                return;

            var inputNode = (edge.input as PortView).Owner;
            var outputNode = (edge.output as PortView).Owner;

            inputNode.OnPortDisconnected(edge.input as PortView, edge.output as PortView);
            outputNode.OnPortDisconnected(edge.output as PortView, edge.input as PortView);

            Edges.Remove(edge as EdgeView);
        }

        public void UpdatePortView()
        {
            if (PortData == null) return;

            if (PortData.DisplayType != null)
            {
                portType = PortData.DisplayType;
                visualClass = "Port_" + portType.Name;
            }

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out DisplayNameAttribute attrib))
                portName = attrib.DisplayName;
            else
                portName = NodeEditorUtility.GetDisplayName(FieldName);

            // Update the edge in case the port color have changed
            schedule.Execute(() =>
            {
                foreach (var edge in Edges)
                {
                    edge.UpdateEdgeControl();
                    edge.MarkDirtyRepaint();
                }
            }).ExecuteLater(50); // Hummm

            UpdatePortSize();
        }
    }
}