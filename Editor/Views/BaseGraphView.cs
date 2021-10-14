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
using CZToolKit.Core;
using CZToolKit.Core.Editors;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseGraphView<M> : InternalBaseGraphView where M : BaseGraph
    {
        public M T_Model { get { return Model as M; } }

        public BaseGraphView(BaseGraph graph, BaseGraphWindow window, CommandDispatcher commandDispacter) : base(graph, window, commandDispacter) { }

        protected override void OnInitialized() { }

        public override List<Port> GetCompatiblePorts(Port startPortView, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(_portView =>
            {
                if (_portView.direction != startPortView.direction)
                    compatiblePorts.Add(_portView);
            });
            return compatiblePorts;
        }

        protected override IEnumerable<Type> GetNodeTypes()
        {
            foreach (var type in Utility_Reflection.GetChildTypes<BaseNode>())
            {
                if (type.IsAbstract) continue;
                yield return type;
            }
        }

        protected override Type GetNodeViewType(BaseNode node)
        {
            return GraphProcessorEditorUtility.GetNodeViewType(node.GetType());
        }

        protected override Type GetConnectionViewType(BaseConnection connection)
        {
            return typeof(InternalBaseConnectionView);
        }

        protected override void UpdateInspector()
        {
            foreach (var element in selection)
            {
                switch (element)
                {
                    case InternalBaseNodeView nodeView:
                        EditorGUILayoutExtension.DrawFieldsInInspector("Node", nodeView, GraphAsset);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    case InternalBaseConnectionView edgeView:
                        EditorGUILayoutExtension.DrawFieldsInInspector("Edge", edgeView, GraphAsset);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    default:
                        break;
                }
            }

            Selection.activeObject = null;
        }
    }

    /// <summary> 默认 </summary>
    public sealed class BaseGraphView : BaseGraphView<BaseGraph>
    {
        public BaseGraphView(BaseGraph graph, BaseGraphWindow window, CommandDispatcher commandDispacter) : base(graph, window, commandDispacter) { }
    }
}
