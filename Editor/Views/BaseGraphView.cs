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
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGraphView
    {
        protected virtual void OnInitialized() { }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.MenuItems().RemoveAll(item =>
            {
                if (item is DropdownMenuSeparator)
                {
                    return true;
                }
                if (!(item is DropdownMenuAction actionItem))
                {
                    return false;
                }
                switch (actionItem.name)
                {
                    case "Cut":
                    case "Copy":
                    case "Paste":
                    case "Duplicate":
                        return true;
                    default:
                        return false;
                }
            });

            if (evt.target is GraphView || evt.target is Node || evt.target is Group || evt.target is Edge)
            {
                evt.menu.AppendAction("Delete", delegate
                {
                    DeleteSelectionCallback(AskUser.DontAskUser);
                }, (DropdownMenuAction a) => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);
                evt.menu.AppendSeparator();
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPortView, NodeAdapter nodeAdapter)
        {
            BasePortView portView = startPortView as BasePortView;
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(_portView =>
            {
                var toPortView = _portView as BasePortView;
                if (IsCompatible(portView, toPortView, nodeAdapter))
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
                        EditorGUILayoutExtension.DrawObjectInInspector("Node", nodeView, GraphAsset);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    case BaseConnectionView edgeView:
                        EditorGUILayoutExtension.DrawObjectInInspector("Connection", edgeView, GraphAsset);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    default:
                        break;
                }
            }
            EditorGUILayoutExtension.DrawObjectInInspector("Graph", this, GraphAsset);
            Selection.activeObject = ObjectInspector.Instance;
            //Selection.activeObject = null;
        }

        protected virtual bool IsCompatible(BasePortView portView, BasePortView toPortView, NodeAdapter nodeAdapter)
        {
            if (toPortView.direction == portView.direction)
                return false;
            // 类型兼容查询
            if (!toPortView.Model.type.IsAssignableFrom(portView.Model.type) && !portView.Model.type.IsAssignableFrom(toPortView.Model.type))
                return false;
            return true;
        }
    }
}
#endif