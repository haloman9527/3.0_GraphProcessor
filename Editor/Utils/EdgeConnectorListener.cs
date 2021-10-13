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
            Slot fromSlot = (edge.output as PortView).Model;
            BaseNode to = (edge.input.node as BaseNodeView).Model;
            Slot toSlot = (edge.input as PortView).Model;
            tempGraphView.CommandDispacter.Do(new ConnectCommand(tempGraphView.Model, from, fromSlot.name, to, toSlot.name));
        }

        /// <summary> 拖到空白松开时触发 </summary>
        public void OnDropOutsidePort(Edge edge, Vector2 position) { }
    }
}