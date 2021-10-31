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
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        /// <summary> 拖拽到符合条件的接口上松开时触发 </summary>
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
            BaseGraphView tempGraphView = graphView as BaseGraphView;

            BaseNode from = (edge.output.node as BaseNodeView).Model;
            BasePort fromPort = (edge.output as BasePortView).Model;
            BaseNode to = (edge.input.node as BaseNodeView).Model;
            BasePort toPort = (edge.input as BasePortView).Model;
            tempGraphView.CommandDispacter.Do(new ConnectCommand(tempGraphView.Model, from, fromPort.name, to, toPort.name));
        }


        /// <summary> 拖到空白松开时触发 </summary>
        public void OnDropOutsidePort(Edge edge, Vector2 position) { }
    }
}
#endif