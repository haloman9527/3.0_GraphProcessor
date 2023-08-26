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
using System;
using System.Collections.Generic;
using System.Linq;
using CZToolKitEditor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BaseGraph))]
    public partial class BaseGraphView
    {
        List<Port> compatiblePorts = new List<Port>();

        protected virtual void OnInitialized()
        {
            
        }

        protected virtual void OnDestroyed()
        {
            
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Group", delegate
            {
                var group = ViewModel.NewGroup("New Group");
                group.Model.nodes.AddRange(selection.Where(select => select is BaseNodeView).Select(select => (select as BaseNodeView).ViewModel.ID));
                CommandDispatcher.Do(new AddGroupCommand(ViewModel, group));
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
                evt.menu.AppendAction("Delete", delegate { DeleteSelectionCallback(AskUser.DontAskUser); }, (DropdownMenuAction a) => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPortView, NodeAdapter nodeAdapter)
        {
            BasePortView portView = startPortView as BasePortView;

            compatiblePorts.Clear();
            foreach (var _nodeView in NodeViews.Values)
            {
                if (_nodeView.PortViews.Count == 0)
                {
                    continue;
                }
                
                foreach (var _portView in _nodeView.PortViews.Values)
                {
                    if (IsCompatible(_portView, portView, nodeAdapter))
                        compatiblePorts.Add(_portView);
                }
            }

            return compatiblePorts;
        }

        protected virtual void BuildNodeMenu(NodeMenuWindow nodeMenu)
        {
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
            {
                if (nodeType.IsAbstract) 
                    continue;
                var nodeStaticInfo = GraphProcessorUtil.NodeStaticInfos[nodeType];
                if (nodeStaticInfo.hidden)
                    continue;
                
                var path = nodeStaticInfo.path;
                var menu = nodeStaticInfo.menu;
                nodeMenu.entries.Add(new NodeMenuWindow.NodeEntry(path, menu, nodeType));
            }
        }

        protected virtual BaseNodeView NewNodeView(BaseNodeVM nodeVM)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(nodeVM.ModelType)) as BaseNodeView;
        }

        protected virtual BaseGroupView NewGroupView(BaseGroupVM groupVM)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(groupVM.ModelType)) as BaseGroupView;
        }

        protected virtual BaseConnectionView NewConnectionView(BaseConnectionVM connectionVM)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(connectionVM.ModelType)) as BaseConnectionView;
        }

        protected virtual void UpdateInspector()
        {
            foreach (var element in selection)
            {
                if (!ObjectEditor.HasEditor(element))
                    continue;
                ObjectEditor.DrawObjectInInspector((element as GraphElement)?.title, element, GraphAsset);
                return;
            }

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<IGraphAssetOwner>() != null)
                return;
            ObjectEditor.DrawObjectInInspector("Graph", this, GraphAsset);
        }

        protected virtual bool IsCompatible(BasePortView fromPortView, BasePortView toPortView, NodeAdapter nodeAdapter)
        {
            if (toPortView.direction == fromPortView.direction)
                return false;
            // 类型兼容查询
            if (!toPortView.ViewModel.Type.IsAssignableFrom(fromPortView.ViewModel.Type) && !fromPortView.ViewModel.Type.IsAssignableFrom(toPortView.ViewModel.Type))
                return false;
            return true;
        }
    }
}
#endif