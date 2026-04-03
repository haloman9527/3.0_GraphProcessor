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
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Atom.GraphProcessor.Editors
{
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        private static BaseGraphView s_ActivePreviewGraphView;

        public void OnStartEdgeDragging(Port draggedPort)
        {
            if (draggedPort is BasePortView portView)
            {
                s_ActivePreviewGraphView = portView.GraphView;
                s_ActivePreviewGraphView?.ShowPortCompatibilityPreview(portView);
            }
        }

        public void OnStopEdgeDragging()
        {
            s_ActivePreviewGraphView?.ClearPortCompatibilityPreview();
            s_ActivePreviewGraphView = null;
        }

        /// <summary> 拖拽到符合条件的接口上松开时触发 </summary>
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
            BaseGraphView tempGraphView = graphView as BaseGraphView;
            tempGraphView?.ClearPortCompatibilityPreview();

            PortProcessor from = (edge.output as BasePortView).ViewModel;
            PortProcessor to = (edge.input as BasePortView).ViewModel;
            // 已有连线拖拽重定向
            if (edge.userData is BaseConnectionProcessor oldConnection)
            {
                tempGraphView.Context.Do(new ReconnectCommand(tempGraphView.ViewModel, oldConnection, from, to));
            }
            else
            {
                tempGraphView.Context.Do(new ConnectCommand(tempGraphView.ViewModel, from, to));
            }
        }

        /// <summary> 拖到空白松开时触发 </summary>
        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            (edge.output as BasePortView)?.GraphView?.ClearPortCompatibilityPreview();
            (edge.input as BasePortView)?.GraphView?.ClearPortCompatibilityPreview();

            var output = edge.output as BasePortView;
            var input = edge.input as BasePortView;
            var sourcePort = output ?? input;
            if (sourcePort == null)
                return;

            if (sourcePort.GraphView == null)
                return;

            sourcePort.GraphView.OpenNodeMenuForPort(sourcePort, GUIUtility.GUIToScreenPoint(position));
        }
    }
}
#endif
