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
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Atom.UnityEditors;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    public abstract partial class BaseGraphView
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
            var mousePosition = evt.mousePosition;
            var localMousePosition = contentViewContainer.WorldToLocal(mousePosition);
            evt.menu.AppendAction("Create Group", delegate
            {
                var group = ViewModel.NewGroup("New Group");
                group.Model.nodes.AddRange(selection.Where(select => select is BaseNodeView).Select(select => (select as BaseNodeView).ViewModel.ID));
                this.Context.Do(new AddGroupCommand(ViewModel, group));
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
                        
                    case "Light Theme":
                    case "Dark Theme":
                    case "Small Text Size":
                    case "Medium Text Size":
                    case "Large Text Size":
                    case "Huge Text Size":
                        return true;
                    default:
                        return false;
                }
            });

            evt.menu.AppendAction("Create Note", delegate
            {
                var data = new StickyNote();
                data.id = GraphProcessorUtil.GenerateId();
                data.position = localMousePosition.ToInternalVector2Int();
                data.size = new InternalVector2Int(300, 200);
                data.title = "title";
                data.content = "contents";
                var note = ViewModelFactory.ProduceViewModel(data) as StickyNoteProcessor;
                this.Context.Do(() => { ViewModel.AddNote(note); }, () => { ViewModel.RemoveNote(note.ID); });
            });

            switch (evt.target)
            {
                case GraphView:
                case UnityEditor.Experimental.GraphView.Node:
                case UnityEditor.Experimental.GraphView.Group:
                case Edge:
                case UnityEditor.Experimental.GraphView.StickyNote:
                {
                    evt.menu.AppendAction("Delete", delegate { DeleteSelectionCallback(AskUser.DontAskUser); }, (DropdownMenuAction a) => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);
                    break;
                }
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
            foreach (var nodeInfo in GraphProcessorUtil.GetNodeStaticInfos())
            {
                if (nodeInfo.Hidden)
                    continue;

                var path = nodeInfo.Path;
                var menu = nodeInfo.Menu;
                nodeMenu.entries.Add(new NodeMenuWindow.NodeEntry(path, menu, nodeInfo.NodeType));
            }
        }

        protected virtual BaseNodeView NewNodeView(BaseNodeProcessor nodeVM)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(nodeVM.ModelType)) as BaseNodeView;
        }

        protected virtual GroupView NewGroupView(GroupProcessor groupVM)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(groupVM.ModelType)) as GroupView;
        }

        protected virtual BaseConnectionView NewConnectionView(BaseConnectionProcessor connectionVM)
        {
            return Activator.CreateInstance(GraphProcessorEditorUtil.GetViewType(connectionVM.ModelType)) as BaseConnectionView;
        }

        protected virtual void UpdateInspector()
        {
            foreach (var element in selection)
            {
                if (!ObjectEditor.HasEditor(element.GetType()))
                    continue;

                ObjectInspector.Show(element);
                return;
            }

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<IGraphAssetOwner>() != null)
                return;
            
            ObjectInspector.Show(this);
        }

        protected virtual bool IsCompatible(BasePortView fromPortView, BasePortView toPortView, NodeAdapter nodeAdapter)
        {
            if (toPortView.direction == fromPortView.direction)
                return false;
            // 类型兼容查询
            if (!toPortView.ViewModel.PortType.IsAssignableFrom(fromPortView.ViewModel.PortType) && !fromPortView.ViewModel.PortType.IsAssignableFrom(toPortView.ViewModel.PortType))
                return false;
            return true;
        }
    }
}
#endif