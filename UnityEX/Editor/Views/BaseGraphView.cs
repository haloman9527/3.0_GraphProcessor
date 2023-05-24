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
using CZToolKit.Common.Collection;
using CZToolKit.Common.ViewModel;
using CZToolKit.Common.IMGUI;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected virtual void OnCreated()
        {
            
        }

        protected virtual void OnDestroyed()
        {
            
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Group", delegate
            {
                var group = new BaseGroup() { groupName = "New Group" };
                group.nodes.AddRange(selection.Where(select => select is BaseNodeView).Select(select => (select as BaseNodeView).ViewModel.ID));
                CommandDispatcher.Do(new AddGroupCommand(ViewModel, ViewModelFactory.CreateViewModel(group) as BaseGroupVM));
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
            switch (portView.ViewModel.Direction)
            {
                case BasePort.Direction.Input:
                {
                    ports.ForEach(_portView =>
                    {
                        var fromPortView = _portView as BasePortView;
                        if (IsCompatible(fromPortView, portView, nodeAdapter))
                            compatiblePorts.Add(_portView);
                    });
                }
                    break;
                case BasePort.Direction.Output:
                {
                    ports.ForEach(_portView =>
                    {
                        var toPortView = _portView as BasePortView;
                        if (IsCompatible(portView, toPortView, nodeAdapter))
                            compatiblePorts.Add(_portView);
                    });
                }
                    break;
            }

            return compatiblePorts;
        }

        protected virtual void NodeCreationRequest(NodeCreationContext c)
        {
            var multiLayereEntryCount = 0;
            var entries = new List<NodeEntry>(16);
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
            {
                if (nodeType.IsAbstract) 
                    continue;
                var nodeStaticInfo = GraphProcessorUtil.NodeStaticInfos[nodeType];
                var path = nodeStaticInfo.path;
                var menu = nodeStaticInfo.menu;
                var hidden = nodeStaticInfo.hidden;

                if (menu.Length > 1)
                    multiLayereEntryCount++;
                entries.Add(new NodeEntry(nodeType, path, menu, hidden));
            }

            entries.QuickSort((a, b) => -(a.menu.Length.CompareTo(b.menu.Length)));
            entries.QuickSort(0, multiLayereEntryCount - 1, (a, b) => String.Compare(a.path, b.path, StringComparison.Ordinal));
            entries.QuickSort(multiLayereEntryCount, entries.Count - 1, (a, b) => String.Compare(a.path, b.path, StringComparison.Ordinal));

            var nodeMenu = ScriptableObject.CreateInstance<NodeMenuWindow>();
            nodeMenu.Initialize("Nodes", this, entries);
            SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), nodeMenu);
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