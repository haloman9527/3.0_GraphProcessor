using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using CZToolKit.Core;

namespace GraphProcessor.Editors
{
    public class PortView : Port
    {
        public const int DefaultPortSize = 8;

        const string PortViewStyleFile = "GraphProcessorStyles/PortView";
        const string UserPortStyleFile = "PortViewTypes";

        public static StyleSheet portViewStyle;
        public static StyleSheet userPortStyle;

        public static StyleSheet PortViewStyle
        {
            get
            {
                if (portViewStyle == null)
                    portViewStyle = Resources.Load<StyleSheet>(PortViewStyleFile);
                return portViewStyle;
            }
        }
        public static StyleSheet UserPortStyle
        {
            get
            {
                if (userPortStyle == null)
                    userPortStyle = Resources.Load<StyleSheet>(UserPortStyleFile);
                return userPortStyle;
            }
        }

        public static PortView CreatePV(Orientation orientation, Direction direction, NodePort portData, BaseEdgeConnectorListener edgeConnectorListener)
        {
            var pv = new PortView(orientation, direction, portData, edgeConnectorListener);

            pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            pv.AddManipulator(pv.m_EdgeConnector);

            // Force picking in the port label to enlarge the edge creation zone
            var portLabel = pv.Q("type");
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;
                portLabel.style.flexGrow = 1;
            }
            bool vertical = orientation == Orientation.Vertical;
            // hide label when the port is vertical
            if (vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            // Fixup picking mode for vertical top ports
            if (vertical)
                pv.Q("connector").pickingMode = PickingMode.Position;

            return pv;
        }


        public int size;
        public NodePort portData;
        public FieldInfo fieldInfo;

        public PortAttribute portAttribute;
        protected BaseEdgeConnectorListener listener;

        List<EdgeView> edges = new List<EdgeView>();

        public BaseNodeView Owner { get; private set; }
        public string FieldName { get { return portData.FieldName; } }
        public List<EdgeView> Edges { get { return edges; } }
        public int ConnectionCount { get { return edges.Count; } }
        bool vertical;
        PortView(Orientation _orientation, Direction _direction, NodePort _portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(_orientation, _direction, Capacity.Multi, _portData.DisplayType)
        {
            styleSheets.Add(Resources.Load<StyleSheet>(PortViewStyleFile));
            StyleSheet userPortStyle = Resources.Load<StyleSheet>(UserPortStyleFile);
            if (userPortStyle != null)
                styleSheets.Add(userPortStyle);

            listener = edgeConnectorListener;
            portType = _portData.DisplayType;
            portData = _portData;


            vertical = _orientation == Orientation.Vertical;
            if (vertical)
                AddToClassList("Vertical");

            UpdatePortSize();
        }

        public virtual void Initialize(BaseNodeView _nodeView)
        {
            Owner = _nodeView;

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out PortColorAttribute colorAttrib))
                portColor = colorAttrib.Color;

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out TooltipAttribute toolTipAttrib))
                tooltip = toolTipAttrib.tooltip;
            else if (vertical)
            {
                tooltip = NodeEditorUtility.GetPortDisplayName(FieldName);
            }

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out PortAttribute attrib) && !string.IsNullOrEmpty(attrib.DisplayName))
                portName = attrib.DisplayName;
            else
                portName = NodeEditorUtility.GetPortDisplayName(FieldName);
            AddToClassList(FieldName);
            visualClass = "Port_" + portType.Name;
        }

        /// <summary> Update the size of the port view (using the portData.sizeInPixel property) </summary>
        public void UpdatePortSize()
        {
            if (AttributeCache.TryGetFieldAttribute(portData.Owner.GetType(), portData.FieldName, out PortSizeAttribute portSizeAttribute))
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
            edges.ForEach(e => e.UpdateEdgeSize());
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);

            var inputNode = (edge.input as PortView).Owner;
            var outputNode = (edge.output as PortView).Owner;

            edges.Add(edge as EdgeView);

            inputNode.OnPortConnected(edge.input as PortView);
            outputNode.OnPortConnected(edge.output as PortView);
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);

            if (!(edge as EdgeView).isConnected)
                return;

            var inputNode = (edge.input as PortView).Owner;
            var outputNode = (edge.output as PortView).Owner;

            inputNode.OnPortDisconnected(edge.input as PortView);
            outputNode.OnPortDisconnected(edge.output as PortView);

            edges.Remove(edge as EdgeView);
        }

        public void UpdatePortView(NodePort _portData)
        {
            portData = _portData;

            if (_portData.DisplayType != null)
            {
                base.portType = _portData.DisplayType;
                portType = _portData.DisplayType;
                visualClass = "Port_" + portType.Name;
            }

            if (AttributeCache.TryGetFieldAttribute(Owner.NodeDataType, FieldName, out PortAttribute attrib) && !string.IsNullOrEmpty(attrib.DisplayName))
                portName = attrib.DisplayName;
            else
                portName = NodeEditorUtility.GetPortDisplayName(FieldName);

            // Update the edge in case the port color have changed
            schedule.Execute(() =>
            {
                foreach (var edge in edges)
                {
                    edge.UpdateEdgeControl();
                    edge.MarkDirtyRepaint();
                }
            }).ExecuteLater(50); // Hummm

            UpdatePortSize();
        }
    }
}