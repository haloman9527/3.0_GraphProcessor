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
 *  Blog: https://www.mindgear.net/
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

            BasePortVM from = (edge.output as BasePortView).ViewModel;
            BasePortVM to = (edge.input as BasePortView).ViewModel;
            // 如果连线不是一个新建的连线就重定向
            if (edge.userData is BaseConnectionVM)
                tempGraphView.CommandDispatcher.Do(new ConnectCommand(tempGraphView.ViewModel, from, to));
            else
                tempGraphView.CommandDispatcher.Do(new ConnectCommand(tempGraphView.ViewModel, from, to));
        }

        /// <summary> 拖到空白松开时触发 </summary>
        public void OnDropOutsidePort(Edge edge, Vector2 position) { }
    }
}
#endif