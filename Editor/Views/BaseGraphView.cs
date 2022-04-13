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
using System.Linq;
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
            evt.menu.AppendAction("Create Group", delegate
            {
                var group = new Group("New Group");
                Model.AddGroup(group);
                group.AddNodes(selection.Where(select => select is BaseNodeView).Select(select => (select as BaseNodeView).Model));
            }, (DropdownMenuAction a) => canDeleteSelection && selection.Find(s => s is BaseNodeView) != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);

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
            var type = GraphProcessorEditorUtil.GetNodeViewType(node.GetType());
            if (type == null)
                type = typeof(BaseNodeView);
            return type;
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
                        ObjectEditor.DrawObjectInInspector("Node", nodeView, GraphAsset);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    case BaseConnectionView edgeView:
                        ObjectEditor.DrawObjectInInspector("Connection", edgeView, GraphAsset);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    default:
                        break;
                }
            }
            ObjectEditor.DrawObjectInInspector("Graph", this, GraphAsset);
            Selection.activeObject = ObjectInspector.Instance;
        }

        protected virtual bool IsCompatible(BasePortView portView, BasePortView toPortView, NodeAdapter nodeAdapter)
        {
            if (toPortView.direction == portView.direction)
                return false;
            // 类型兼容查询
            if (!toPortView.Model.Type.IsAssignableFrom(portView.Model.Type) && !portView.Model.Type.IsAssignableFrom(toPortView.Model.Type))
                return false;
            return true;
        }
    }
}
#endif