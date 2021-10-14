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
            InternalBaseGraphView tempGraphView = graphView as InternalBaseGraphView;

            BaseNode from = (edge.output.node as InternalBaseNodeView).Model;
            BaseSlot fromSlot = (edge.output as InternalBasePortView).Model;
            BaseNode to = (edge.input.node as InternalBaseNodeView).Model;
            BaseSlot toSlot = (edge.input as InternalBasePortView).Model;
            tempGraphView.CommandDispacter.Do(new ConnectCommand(tempGraphView.Model, from, fromSlot.name, to, toSlot.name));
        }

        /// <summary> 拖到空白松开时触发 </summary>
        public void OnDropOutsidePort(Edge edge, Vector2 position) { }
    }
}