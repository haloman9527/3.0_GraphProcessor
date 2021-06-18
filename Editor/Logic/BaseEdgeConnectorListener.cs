using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseEdgeConnectorListener : IEdgeConnectorListener
    {
        protected BaseGraphView GraphView { get; private set; }

        public BaseEdgeConnectorListener(BaseGraphView _graphView)
        {
            GraphView = _graphView;
        }

        /// <summary> 当连线被拖到新接口松开 </summary>
        /// <param name="_graphView"></param>
        /// <param name="_edge"></param>
        public virtual void OnDrop(GraphView _graphView, Edge _edge)
        {
            if (_edge.input == null || _edge.output == null)
                return;
            if (_edge.input.node == null || _edge.output.node == null)
                return;
            BaseGraphView graphView = _graphView as BaseGraphView;
            graphView.RegisterCompleteObjectUndo("Connected " + _edge.input.node.name + " and " + _edge.output.node.name);
            graphView.Connect(_edge as EdgeView);
        }

        /// <summary> 当连线被拖到空白区松开 </summary>
        /// <param name="_edge"></param>
        /// <param name="_position"></param>
        public virtual void OnDropOutsidePort(Edge _edge, Vector2 _position)
        {
            if (!_edge.isGhostEdge)
            {
                GraphView.RegisterCompleteObjectUndo("Disconnect edge");
                GraphView.RemoveElement(_edge as EdgeView);
            }

            //if (edge.input == null || edge.output == null)
            //    ShowNodeCreationMenuFromEdge(edge as EdgeView, position);
        }

        void ShowNodeCreationMenuFromEdge(EdgeView edgeView, Vector2 position)
        {
            //edgeNodeCreateMenuWindow.Initialize(graphView, edgeView);
            SearchWindow.Open(new SearchWindowContext(position + EditorWindow.focusedWindow.position.position), GraphView.CreateNodeMenu);
        }
    }
}