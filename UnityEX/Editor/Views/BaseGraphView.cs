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
using Atom.UnityEditors;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    public abstract partial class BaseGraphView
    {
        private sealed class GraphClipboardData
        {
            public readonly List<BaseNode> Nodes = new List<BaseNode>();
            public readonly List<BaseConnection> Connections = new List<BaseConnection>();
            public readonly List<Group> Groups = new List<Group>();
            public readonly List<StickyNote> Notes = new List<StickyNote>();
            public readonly List<PlacematData> Placemats = new List<PlacematData>();
            public InternalVector2Int Anchor;

            public bool HasData => Nodes.Count > 0 || Notes.Count > 0 || Groups.Count > 0 || Placemats.Count > 0;
        }

        private enum SelectionAlignMode
        {
            Left,
            Right,
            Top,
            Bottom,
        }

        private static GraphClipboardData s_Clipboard;
        private static int s_PasteSerial;
        private readonly List<BasePortView> m_CompatibilityPreviewPorts = new List<BasePortView>(32);
        private BasePortView m_CompatibilityPreviewSource;

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
                // 收集选中的节点
                var selectedNodes = new List<BaseNodeProcessor>();
                foreach (var select in selection)
                {
                    if (select is BaseNodeView nodeView)
                        selectedNodes.Add(nodeView.ViewModel);
                }
                var capturedGraph = ViewModel;
                var capturedNodes = selectedNodes.ToArray();
                // 将 AddGroup + AddToGroup 封装为单个 Undo 步骤，避免跨帧分割
                this.Context.Do(
                    () =>
                    {
                        capturedGraph.AddGroup(group);
                        foreach (var node in capturedNodes)
                            capturedGraph.Groups.AddNodeToGroup(group, node);
                    },
                    () =>
                    {
                        foreach (var node in capturedNodes)
                            capturedGraph.Groups.RemoveNodeFromGroup(node);
                        capturedGraph.RemoveGroup(group);
                    });
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

            evt.menu.AppendAction("Create Placemat", delegate
            {
                var placemat = ViewModel.NewPlacemat(localMousePosition.ToInternalVector2Int());
                this.Context.Do(() => { ViewModel.AddPlacemat(placemat); }, () => { ViewModel.RemovePlacemat(placemat.ID); });
            });

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Selection/Frame", _ => FrameSelection(), _ => selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Copy", _ => CopySelectionToClipboard(), _ => selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Cut", _ => CutSelectionToClipboard(), _ => selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Paste", _ => PasteClipboard(), _ => HasClipboardData() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Duplicate", _ => DuplicateSelection(), _ => selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Align Left", _ => AlignSelection(SelectionAlignMode.Left), _ => HasAtLeastTwoSelectedScopes() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Align Right", _ => AlignSelection(SelectionAlignMode.Right), _ => HasAtLeastTwoSelectedScopes() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Align Top", _ => AlignSelection(SelectionAlignMode.Top), _ => HasAtLeastTwoSelectedScopes() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Align Bottom", _ => AlignSelection(SelectionAlignMode.Bottom), _ => HasAtLeastTwoSelectedScopes() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Distribute Horizontal", _ => DistributeSelectionHorizontal(), _ => HasAtLeastThreeSelectedScopes() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Selection/Distribute Vertical", _ => DistributeSelectionVertical(), _ => HasAtLeastThreeSelectedScopes() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

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

            // 每次返回新列表，避免共享引用导致调用方迭代时列表被清空的竞态问题
            var result = new List<Port>();
            foreach (var _nodeView in NodeViews.Values)
            {
                if (_nodeView.PortViews.Count == 0)
                {
                    continue;
                }

                foreach (var _portView in _nodeView.PortViews.Values)
                {
                    if (IsCompatible(_portView, portView, nodeAdapter))
                        result.Add(_portView);
                }
            }

            return result;
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

        internal List<NodeMenuWindow.INodeEntry> CollectNodeMenuEntries()
        {
            var nodeMenu = ScriptableObject.CreateInstance<NodeMenuWindow>();
            nodeMenu.Initialize("Nodes", this);
            BuildNodeMenu(nodeMenu);
            nodeMenu.entries.QuickSort((a, b) => string.Compare(a.Path, b.Path, StringComparison.Ordinal));
            return new List<NodeMenuWindow.INodeEntry>(nodeMenu.entries);
        }

        internal void CreateNodeFromLibraryEntry(NodeMenuWindow.INodeEntry entry)
        {
            if (entry == null)
                return;

            var position = GetMousePosition().ToInternalVector2Int();
            var node = entry.CreateNode(ViewModel, position);
            if (node == null)
                return;

            Context.Do(new AddNodeCommand(ViewModel, node));
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

        private bool IsNodeEntryCompatibleWithSourcePort(NodeMenuWindow.INodeEntry entry, PortProcessor sourcePort)
        {
            if (entry?.NodeType == null || sourcePort == null)
                return false;

            var node = CreateTemporaryNode(entry.NodeType);
            if (node == null)
                return false;

            return FindBestCompatiblePort(node, sourcePort) != null;
        }

        private BaseNodeProcessor CreateTemporaryNode(Type nodeType)
        {
            try
            {
                if (nodeType == null)
                    return null;

                var model = Activator.CreateInstance(nodeType) as BaseNode;
                if (model == null)
                    return null;

                model.id = GraphProcessorUtil.GenerateId();
                model.position = InternalVector2Int.zero;
                return ViewModelFactory.ProduceViewModel(model) as BaseNodeProcessor;
            }
            catch
            {
                return null;
            }
        }

        internal void OpenNodeMenuForPort(BasePortView sourcePortView, Vector2 screenPosition)
        {
            if (sourcePortView == null)
                return;

            var sourcePort = sourcePortView.ViewModel;
            if (sourcePort == null)
                return;

            var nodeMenu = ScriptableObject.CreateInstance<NodeMenuWindow>();
            nodeMenu.Initialize($"Create & Connect ({sourcePort.Name} | {GetPortDirectionLabel(sourcePort.Direction)} | {GetPortTypeLabel(sourcePort.PortType)})", this);
            BuildNodeMenu(nodeMenu);

            var compatibleNodeTypes = new HashSet<Type>();
            for (var i = 0; i < nodeMenu.entries.Count; i++)
            {
                var entry = nodeMenu.entries[i];
                if (entry?.NodeType == null)
                    continue;
                if (IsNodeEntryCompatibleWithSourcePort(entry, sourcePort))
                    compatibleNodeTypes.Add(entry.NodeType);
            }

            if (compatibleNodeTypes.Count == 0)
            {
                Context?.graphWindow?.ShowNotification(new GUIContent($"No compatible nodes for '{sourcePort.Name}'"));
                return;
            }

            nodeMenu.SetFilter(entry =>
            {
                return entry?.NodeType != null && compatibleNodeTypes.Contains(entry.NodeType);
            });

            nodeMenu.SetNodeCreatedHandler(newNode =>
            {
                if (newNode == null)
                    return true;

                var targetPort = FindBestCompatiblePort(newNode, sourcePort);
                ConnectCommand connectCommand = null;
                if (targetPort != null)
                {
                    if (sourcePort.Direction == BasePort.Direction.Left || sourcePort.Direction == BasePort.Direction.Top)
                        connectCommand = new ConnectCommand(ViewModel, targetPort, sourcePort);
                    else
                        connectCommand = new ConnectCommand(ViewModel, sourcePort, targetPort);
                }

                Context.Do(() =>
                {
                    ViewModel.AddNode(newNode);
                    connectCommand?.Do();

                    if (connectCommand != null && connectCommand.Connection == null)
                    {
                        ViewModel.RemoveNode(newNode);
                        Context?.graphWindow?.ShowNotification(new GUIContent("Create node succeeded, but connect failed"));
                    }
                }, () =>
                {
                    if (connectCommand?.Connection != null)
                        connectCommand.Undo();
                    if (newNode.Owner == ViewModel)
                        ViewModel.RemoveNode(newNode);
                });

                return true;
            });

            SearchWindow.Open(new SearchWindowContext(screenPosition), nodeMenu);
        }

        internal void ShowPortCompatibilityPreview(BasePortView sourcePortView)
        {
            if (sourcePortView == null)
                return;

            ClearPortCompatibilityPreview();
            m_CompatibilityPreviewSource = sourcePortView;
            sourcePortView.AddToClassList("port-source");

            foreach (var nodeView in NodeViews.Values)
            {
                foreach (var targetPort in nodeView.PortViews.Values)
                {
                    if (targetPort == sourcePortView)
                        continue;
                    if (!IsCompatible(sourcePortView, targetPort, null))
                        continue;

                    targetPort.AddToClassList("port-compatible");
                    m_CompatibilityPreviewPorts.Add(targetPort);
                }
            }
        }

        internal void ClearPortCompatibilityPreview()
        {
            if (m_CompatibilityPreviewSource != null)
            {
                m_CompatibilityPreviewSource.RemoveFromClassList("port-source");
                m_CompatibilityPreviewSource = null;
            }

            for (var i = 0; i < m_CompatibilityPreviewPorts.Count; i++)
                m_CompatibilityPreviewPorts[i].RemoveFromClassList("port-compatible");
            m_CompatibilityPreviewPorts.Clear();
        }

        private static PortProcessor FindBestCompatiblePort(BaseNodeProcessor node, PortProcessor sourcePort)
        {
            if (node == null || sourcePort == null)
                return null;

            var sourceIsInput = sourcePort.Direction == BasePort.Direction.Left || sourcePort.Direction == BasePort.Direction.Top;
            var needInput = !sourceIsInput;
            var targetDirection = needInput ? BasePort.Direction.Left : BasePort.Direction.Right;
            PortProcessor fallback = null;

            foreach (var port in node.Ports.Values)
            {
                var isInput = port.Direction == BasePort.Direction.Left || port.Direction == BasePort.Direction.Top;
                if (needInput != isInput)
                    continue;

                if (port.Direction == targetDirection && IsPortTypeCompatible(sourcePort.PortType, port.PortType))
                    return port;

                if (fallback == null && IsPortTypeCompatible(sourcePort.PortType, port.PortType))
                    fallback = port;
            }

            return fallback;
        }

        private static bool IsPortTypeCompatible(Type a, Type b)
        {
            if (a == null || b == null)
                return true;
            return a.IsAssignableFrom(b) || b.IsAssignableFrom(a);
        }

        private static string GetPortDirectionLabel(BasePort.Direction direction)
        {
            switch (direction)
            {
                case BasePort.Direction.Left:
                    return "Input";
                case BasePort.Direction.Top:
                    return "Input";
                case BasePort.Direction.Right:
                    return "Output";
                case BasePort.Direction.Bottom:
                    return "Output";
                default:
                    return direction.ToString();
            }
        }

        private static string GetPortTypeLabel(Type portType)
        {
            return portType == null ? "Any" : portType.Name;
        }

        public void CopySelectionToClipboard()
        {
            if (selection.Count == 0)
            {
                s_Clipboard = null;
                s_PasteSerial = 0;
                return;
            }

            s_Clipboard = BuildClipboardFromSelection();
            s_PasteSerial = 0;
        }

        public void CutSelectionToClipboard()
        {
            if (selection.Count == 0)
                return;

            CopySelectionToClipboard();
            DeleteSelectionCallback(AskUser.DontAskUser);
        }

        public void DuplicateSelection()
        {
            if (selection.Count == 0)
                return;

            CopySelectionToClipboard();
            PasteClipboard();
        }

        public void PasteClipboard()
        {
            if (!HasClipboardData())
                return;

            var clip = s_Clipboard;
            var mouse = GetMousePosition().ToInternalVector2Int();
            var step = new InternalVector2Int(24 * s_PasteSerial, 24 * s_PasteSerial);
            var delta = (mouse - clip.Anchor) + step;

            var nodeIdMap = new Dictionary<long, long>(clip.Nodes.Count);
            var nodeVMs = new List<BaseNodeProcessor>(clip.Nodes.Count);
            for (var i = 0; i < clip.Nodes.Count; i++)
            {
                var nodeModel = (BaseNode)CloneModel(clip.Nodes[i]);
                var oldId = nodeModel.id;
                nodeModel.id = GraphProcessorUtil.GenerateId();
                nodeModel.position = nodeModel.position + delta;
                nodeIdMap[oldId] = nodeModel.id;
                nodeVMs.Add(ViewModelFactory.ProduceViewModel(nodeModel) as BaseNodeProcessor);
            }

            var noteVMs = new List<StickyNoteProcessor>(clip.Notes.Count);
            for (var i = 0; i < clip.Notes.Count; i++)
            {
                var noteModel = (StickyNote)CloneModel(clip.Notes[i]);
                noteModel.id = GraphProcessorUtil.GenerateId();
                noteModel.position = noteModel.position + delta;
                noteVMs.Add(ViewModelFactory.ProduceViewModel(noteModel) as StickyNoteProcessor);
            }

            var placematVMs = new List<PlacematProcessor>(clip.Placemats.Count);
            for (var i = 0; i < clip.Placemats.Count; i++)
            {
                var placematModel = (PlacematData)CloneModel(clip.Placemats[i]);
                placematModel.id = GraphProcessorUtil.GenerateId();
                placematModel.position = placematModel.position + delta;
                placematVMs.Add(ViewModelFactory.ProduceViewModel(placematModel) as PlacematProcessor);
            }

            var groupVMs = new List<GroupProcessor>(clip.Groups.Count);
            var groupNodeMaps = new List<(GroupProcessor group, BaseNodeProcessor node)>();
            for (var i = 0; i < clip.Groups.Count; i++)
            {
                var groupModel = (Group)CloneModel(clip.Groups[i]);
                groupModel.id = GraphProcessorUtil.GenerateId();
                groupModel.position = groupModel.position + delta;
                groupModel.nodes.Clear();
                var groupVM = ViewModelFactory.ProduceViewModel(groupModel) as GroupProcessor;
                groupVMs.Add(groupVM);

                for (var n = 0; n < clip.Groups[i].nodes.Count; n++)
                {
                    var oldNodeId = clip.Groups[i].nodes[n];
                    if (!nodeIdMap.TryGetValue(oldNodeId, out var newNodeId))
                        continue;
                    for (var p = 0; p < nodeVMs.Count; p++)
                    {
                        if (nodeVMs[p].ID == newNodeId)
                        {
                            groupNodeMaps.Add((groupVM, nodeVMs[p]));
                            break;
                        }
                    }
                }
            }

            var connectionVMs = new List<BaseConnectionProcessor>(clip.Connections.Count);
            for (var i = 0; i < clip.Connections.Count; i++)
            {
                var connectionModel = (BaseConnection)CloneModel(clip.Connections[i]);
                if (!nodeIdMap.TryGetValue(connectionModel.fromNode, out var newFromId))
                    continue;
                if (!nodeIdMap.TryGetValue(connectionModel.toNode, out var newToId))
                    continue;
                connectionModel.fromNode = newFromId;
                connectionModel.toNode = newToId;
                connectionVMs.Add(ViewModelFactory.ProduceViewModel(connectionModel) as BaseConnectionProcessor);
            }

            var nodeMap = new Dictionary<long, BaseNodeProcessor>(nodeVMs.Count);
            for (var i = 0; i < nodeVMs.Count; i++)
                nodeMap[nodeVMs[i].ID] = nodeVMs[i];

            var connectCommands = new List<ConnectCommand>(connectionVMs.Count);
            for (var i = 0; i < connectionVMs.Count; i++)
            {
                var c = connectionVMs[i];
                if (!nodeMap.TryGetValue(c.FromNodeID, out var fromNode))
                    continue;
                if (!nodeMap.TryGetValue(c.ToNodeID, out var toNode))
                    continue;
                if (!fromNode.Ports.TryGetValue(c.FromPortName, out var fromPort))
                    continue;
                if (!toNode.Ports.TryGetValue(c.ToPortName, out var toPort))
                    continue;
                connectCommands.Add(new ConnectCommand(ViewModel, fromPort, toPort));
            }

            Context.Do(() =>
            {
                for (var i = 0; i < nodeVMs.Count; i++)
                    ViewModel.AddNode(nodeVMs[i]);
                for (var i = 0; i < noteVMs.Count; i++)
                    ViewModel.AddNote(noteVMs[i]);
                for (var i = 0; i < placematVMs.Count; i++)
                    ViewModel.AddPlacemat(placematVMs[i]);
                for (var i = 0; i < groupVMs.Count; i++)
                    ViewModel.AddGroup(groupVMs[i]);
                for (var i = 0; i < groupNodeMaps.Count; i++)
                    ViewModel.Groups.AddNodeToGroup(groupNodeMaps[i].group, groupNodeMaps[i].node);
                for (var i = 0; i < connectCommands.Count; i++)
                    connectCommands[i].Do();

                for (var i = connectCommands.Count - 1; i >= 0; i--)
                {
                    if (connectCommands[i].Connection == null)
                        connectCommands.RemoveAt(i);
                }

                ClearSelection();
                for (var i = 0; i < nodeVMs.Count; i++)
                {
                    if (NodeViews.TryGetValue(nodeVMs[i].ID, out var nodeView))
                        AddToSelection(nodeView);
                }
                for (var i = 0; i < noteVMs.Count; i++)
                {
                    if (NoteViews.TryGetValue(noteVMs[i].ID, out var noteView))
                        AddToSelection(noteView);
                }
                for (var i = 0; i < placematVMs.Count; i++)
                {
                    if (PlacematViews.TryGetValue(placematVMs[i].ID, out var placematView))
                        AddToSelection(placematView);
                }
                for (var i = 0; i < groupVMs.Count; i++)
                {
                    if (GroupViews.TryGetValue(groupVMs[i].ID, out var groupView))
                        AddToSelection(groupView);
                }
            },
            () =>
            {
                for (var i = connectCommands.Count - 1; i >= 0; i--)
                    connectCommands[i].Undo();
                for (var i = 0; i < groupVMs.Count; i++)
                    ViewModel.RemoveGroup(groupVMs[i]);
                for (var i = 0; i < placematVMs.Count; i++)
                    ViewModel.RemovePlacemat(placematVMs[i].ID);
                for (var i = 0; i < noteVMs.Count; i++)
                    ViewModel.RemoveNote(noteVMs[i].ID);
                for (var i = 0; i < nodeVMs.Count; i++)
                    ViewModel.RemoveNode(nodeVMs[i]);
                ClearSelection();
            });

            s_PasteSerial++;
        }

        private bool HasClipboardData()
        {
            return s_Clipboard != null && s_Clipboard.HasData;
        }

        private GraphClipboardData BuildClipboardFromSelection()
        {
            var data = new GraphClipboardData();
            var nodeIds = new HashSet<long>();
            var selectedGroups = new List<GroupProcessor>();
            var connectionKeys = new HashSet<string>();

            foreach (var item in selection)
            {
                switch (item)
                {
                    case BaseNodeView nodeView:
                        nodeIds.Add(nodeView.ViewModel.ID);
                        break;
                    case StickyNoteView noteView:
                        data.Notes.Add((StickyNote)CloneModel(noteView.ViewModel.Model));
                        break;
                    case PlacematView placematView:
                        data.Placemats.Add((PlacematData)CloneModel(placematView.ViewModel.Model));
                        break;
                    case GroupView groupView:
                        selectedGroups.Add(groupView.ViewModel);
                        foreach (var nodeId in groupView.ViewModel.Nodes)
                            nodeIds.Add(nodeId);
                        break;
                }
            }

            foreach (var nodeId in nodeIds)
            {
                if (ViewModel.Nodes.TryGetValue(nodeId, out var node))
                    data.Nodes.Add((BaseNode)CloneModel(node.Model));
            }

            for (var i = 0; i < selectedGroups.Count; i++)
            {
                var group = (Group)CloneModel(selectedGroups[i].Model);
                group.nodes.RemoveAll(id => !nodeIds.Contains(id));
                data.Groups.Add(group);
            }

            for (var i = 0; i < ViewModel.Connections.Count; i++)
            {
                var c = ViewModel.Connections[i];
                if (nodeIds.Contains(c.FromNodeID) && nodeIds.Contains(c.ToNodeID))
                {
                    var key = $"{c.FromNodeID}:{c.FromPortName}->{c.ToNodeID}:{c.ToPortName}";
                    if (connectionKeys.Add(key))
                        data.Connections.Add((BaseConnection)CloneModel(c.Model));
                }
            }

            data.Anchor = CalcClipboardAnchor(data);
            return data;
        }

        private static InternalVector2Int CalcClipboardAnchor(GraphClipboardData data)
        {
            var has = false;
            var minX = 0;
            var minY = 0;
            for (var i = 0; i < data.Nodes.Count; i++)
            {
                var p = data.Nodes[i].position;
                if (!has)
                {
                    has = true;
                    minX = p.x;
                    minY = p.y;
                }
                else
                {
                    if (p.x < minX) minX = p.x;
                    if (p.y < minY) minY = p.y;
                }
            }
            for (var i = 0; i < data.Notes.Count; i++)
            {
                var p = data.Notes[i].position;
                if (!has)
                {
                    has = true;
                    minX = p.x;
                    minY = p.y;
                }
                else
                {
                    if (p.x < minX) minX = p.x;
                    if (p.y < minY) minY = p.y;
                }
            }
            for (var i = 0; i < data.Groups.Count; i++)
            {
                var p = data.Groups[i].position;
                if (!has)
                {
                    has = true;
                    minX = p.x;
                    minY = p.y;
                }
                else
                {
                    if (p.x < minX) minX = p.x;
                    if (p.y < minY) minY = p.y;
                }
            }
            for (var i = 0; i < data.Placemats.Count; i++)
            {
                var p = data.Placemats[i].position;
                if (!has)
                {
                    has = true;
                    minX = p.x;
                    minY = p.y;
                }
                else
                {
                    if (p.x < minX) minX = p.x;
                    if (p.y < minY) minY = p.y;
                }
            }

            return has ? new InternalVector2Int(minX, minY) : InternalVector2Int.zero;
        }

        private static object CloneModel(object source)
        {
            if (source == null)
                return null;
            var clone = Activator.CreateInstance(source.GetType());
            EditorUtility.CopySerializedManagedFieldsOnly(source, clone);
            return clone;
        }

        public void AlignSelectionLeft()
        {
            AlignSelection(SelectionAlignMode.Left);
        }

        public void AlignSelectionRight()
        {
            AlignSelection(SelectionAlignMode.Right);
        }

        public void AlignSelectionTop()
        {
            AlignSelection(SelectionAlignMode.Top);
        }

        public void AlignSelectionBottom()
        {
            AlignSelection(SelectionAlignMode.Bottom);
        }

        public void DistributeSelectionHorizontal()
        {
            if (!TryGetSelectedScopes(out var scopes) || scopes.Count < 3)
                return;

            scopes.Sort((a, b) => a.Position.x.CompareTo(b.Position.x));
            var first = scopes[0].Position.x;
            var last = scopes[scopes.Count - 1].Position.x;
            if (first == last)
                return;

            var step = (last - first) / (float)(scopes.Count - 1);
            var newPos = new Dictionary<IGraphElementProcessor_Scope, Rect>(scopes.Count);
            for (var i = 1; i < scopes.Count - 1; i++)
            {
                var scope = scopes[i];
                var p = scope.Position;
                p.x = Mathf.RoundToInt(first + step * i);
                AddMoveIfChanged(newPos, scope, p);
            }

            if (newPos.Count > 0)
                Context.Do(new MoveElementsCommand(newPos));
        }

        public void DistributeSelectionVertical()
        {
            if (!TryGetSelectedScopes(out var scopes) || scopes.Count < 3)
                return;

            scopes.Sort((a, b) => a.Position.y.CompareTo(b.Position.y));
            var first = scopes[0].Position.y;
            var last = scopes[scopes.Count - 1].Position.y;
            if (first == last)
                return;

            var step = (last - first) / (float)(scopes.Count - 1);
            var newPos = new Dictionary<IGraphElementProcessor_Scope, Rect>(scopes.Count);
            for (var i = 1; i < scopes.Count - 1; i++)
            {
                var scope = scopes[i];
                var p = scope.Position;
                p.y = Mathf.RoundToInt(first + step * i);
                AddMoveIfChanged(newPos, scope, p);
            }

            if (newPos.Count > 0)
                Context.Do(new MoveElementsCommand(newPos));
        }

        private bool HasAtLeastTwoSelectedScopes()
        {
            return TryGetSelectedScopes(out var scopes) && scopes.Count >= 2;
        }

        private bool HasAtLeastThreeSelectedScopes()
        {
            return TryGetSelectedScopes(out var scopes) && scopes.Count >= 3;
        }

        private void AlignSelection(SelectionAlignMode mode)
        {
            if (!TryGetSelectedScopes(out var scopes) || scopes.Count < 2)
                return;

            var target = GetAlignTarget(scopes, mode);
            var newPos = new Dictionary<IGraphElementProcessor_Scope, Rect>(scopes.Count);
            foreach (var scope in scopes)
            {
                var p = scope.Position;
                switch (mode)
                {
                    case SelectionAlignMode.Left:
                    case SelectionAlignMode.Right:
                        p.x = target;
                        break;
                    case SelectionAlignMode.Top:
                    case SelectionAlignMode.Bottom:
                        p.y = target;
                        break;
                }

                AddMoveIfChanged(newPos, scope, p);
            }

            if (newPos.Count > 0)
                Context.Do(new MoveElementsCommand(newPos));
        }

        private static int GetAlignTarget(List<IGraphElementProcessor_Scope> scopes, SelectionAlignMode mode)
        {
            var target = mode == SelectionAlignMode.Right || mode == SelectionAlignMode.Bottom ? int.MinValue : int.MaxValue;
            for (var i = 0; i < scopes.Count; i++)
            {
                var p = scopes[i].Position;
                switch (mode)
                {
                    case SelectionAlignMode.Left:
                        if (p.x < target) target = p.x;
                        break;
                    case SelectionAlignMode.Right:
                        if (p.x > target) target = p.x;
                        break;
                    case SelectionAlignMode.Top:
                        if (p.y < target) target = p.y;
                        break;
                    case SelectionAlignMode.Bottom:
                        if (p.y > target) target = p.y;
                        break;
                }
            }

            return target;
        }

        private bool TryGetSelectedScopes(out List<IGraphElementProcessor_Scope> scopes)
        {
            scopes = new List<IGraphElementProcessor_Scope>(selection.Count);
            foreach (var item in selection)
            {
                if (item is IGraphElementView ge && ge.V is IGraphElementProcessor_Scope scope)
                    scopes.Add(scope);
            }

            return scopes.Count > 0;
        }

        private static void AddMoveIfChanged(Dictionary<IGraphElementProcessor_Scope, Rect> dict, IGraphElementProcessor_Scope scope, InternalVector2Int newPosition)
        {
            newPosition = SnapPositionForLayout(newPosition);
            if (scope.Position == newPosition)
                return;

            if (scope is StickyNoteProcessor note)
            {
                dict[scope] = new Rect(newPosition.ToVector2(), note.Size.ToVector2());
                return;
            }

            dict[scope] = new Rect(newPosition.ToVector2(), Vector2.zero);
        }

        private static InternalVector2Int SnapPositionForLayout(InternalVector2Int position)
        {
            if (!GraphProcessorEditorSettings.GridSnapActive.Value)
                return position;

            var snap = GraphProcessorEditorSettings.GridSnapSize.Value;
            if (snap <= 1)
                return position;

            var x = Mathf.RoundToInt(position.x / (float)snap) * snap;
            var y = Mathf.RoundToInt(position.y / (float)snap) * snap;
            return new InternalVector2Int(x, y);
        }
    }
}
#endif
