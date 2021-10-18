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
using CZToolKit.Core;
using CZToolKit.Core.Editors;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGraphView
    {
        protected virtual void OnInitialized() { }

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

        protected virtual IEnumerable<Type> GetNodeTypes()
        {
            foreach (var type in Util_Reflection.GetChildTypes<BaseNode>())
            {
                if (type.IsAbstract) continue;
                yield return type;
            }
        }

        protected virtual Type GetNodeViewType(BaseNode node)
        {
            return GraphProcessorEditorUtility.GetNodeViewType(node.GetType());
        }

        protected virtual Type GetConnectionViewType(BaseConnection connection)
        {
            return typeof(BaseConnectionView);
        }

        protected virtual void UpdateInspector()
        {
            foreach (var element in selection)
            {
                switch (element)
                {
                    case BaseNodeView nodeView:
                        EditorGUILayoutExtension.DrawFieldsInInspector("Node", nodeView, GraphAsset);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    case BaseConnectionView edgeView:
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
}
#endif