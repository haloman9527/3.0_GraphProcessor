using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        protected BaseGraphView GraphView { get; private set; }

        public EdgeConnectorListener(BaseGraphView _graphView)
        {
            GraphView = _graphView;
        }

        /// <summary> 拖拽到符合条件的接口上松开时触发 </summary>
        public virtual void OnDrop(GraphView _graphView, Edge _edge)
        {
            BaseGraphView graphView = _graphView as BaseGraphView;

            BaseNode inputNode = (_edge.input.node as BaseNodeView).Model;
            NodePort inputPort = (_edge.input as NodePortView).Model;

            BaseNode outputNode = (_edge.output.node as BaseNodeView).Model;
            NodePort outputPort = (_edge.output as NodePortView).Model;

            graphView.Model.Connect(inputNode.Ports[inputPort.FieldName], outputNode.Ports[outputPort.FieldName]);
        }

        /// <summary> 拖到空白松开时触发 </summary>
        public virtual void OnDropOutsidePort(Edge _edge, Vector2 _position)
        {
            if (_edge.userData == null)
                GraphView.RemoveElement(_edge as BaseEdgeView);
            else
                GraphView.Model.DisconnectEdge((_edge as BaseEdgeView).Model.GUID);
        }
    }
}