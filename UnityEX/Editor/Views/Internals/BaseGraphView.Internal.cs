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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Atom.GraphProcessor.UnityEX.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    public partial class BaseGraphView : GraphView
    {
        #region Fields

        private BaseGraphProcessor viewModel;
        private GraphViewContext context;
        private MiniMap miniMap;

        #endregion

        #region Properties

        public BaseGraphProcessor ViewModel => viewModel;

        public GraphViewContext Context => context;

        public IGraphAsset GraphAsset => Context.graphWindow.GraphAsset;

        public Dictionary<long, BaseNodeView> NodeViews { get; } = new Dictionary<long, BaseNodeView>();

        public Dictionary<long, StickyNoteView> NoteViews { get; } = new Dictionary<long, StickyNoteView>();

        public Dictionary<long, GroupView> GroupViews { get; } = new Dictionary<long, GroupView>();

        public Dictionary<long, PlacematView> PlacematViews { get; } = new Dictionary<long, PlacematView>();

        public Dictionary<BaseConnectionProcessor, BaseConnectionView> ConnectionViews { get; } = new Dictionary<BaseConnectionProcessor, BaseConnectionView>();

        public bool MiniMapActive
        {
            get => miniMap != null;
            set
            {
                if (value == false && miniMap != null)
                {
                    Remove(miniMap);
                    miniMap = null;
                }

                if (value == true && miniMap == null)
                {
                    miniMap = new MiniMap()
                    {
                        anchored = true,
                    };
                    miniMap.SetPosition(new Rect(15, 15, 200, 200));
                    Add(miniMap);
                }
            }
        }

        #region 不建议使用自带复制粘贴功能，建议自己实现

        protected sealed override bool canCopySelection => false;
        protected sealed override bool canCutSelection => false;
        protected sealed override bool canDuplicateSelection => false;
        protected sealed override bool canPaste => false;

        #endregion

        #endregion

        public BaseGraphView()
        {
            styleSheets.Add(GraphProcessorEditorStyles.DefaultStyles.BaseGraphViewStyle);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.05f, maxScale = 4f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();

            // 拖拽过程中实时网格吸附，避免“松手后跳一下”
            this.RegisterCallback<MouseMoveEvent>(OnMouseMoveRealtimeSnap);
            this.RegisterCallback<MouseUpEvent>(OnMouseUpRealtimeSnap);
            this.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutRealtimeSnap);
        }

        /// <summary>
        /// 根据可见区域更新节点 controls 显示状态（在视图变换后调用，避免定时轮询）。
        /// 使用 style.display 而非 visible，确保隐藏节点不占布局空间。
        /// </summary>
        private void UpdateNodeControlsVisibility()
        {
            var viewBound = this.worldBound;
            foreach (var pair in this.NodeViews)
            {
                // Bug 修复：原为 visible = bool，隐藏时元素仍占布局空间；
                // 改用 style.display 使隐藏元素完全脱离布局流
                pair.Value.controls.style.display = viewBound.Overlaps(pair.Value.worldBound)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }

        private InternalVector2Int SnapPosition(InternalVector2Int position)
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

        private Rect SnapRectPosition(Rect rect)
        {
            var pos = rect.position.ToInternalVector2Int();
            var snapped = SnapPosition(pos);
            if (snapped == pos)
                return rect;

            rect.position = snapped.ToVector2();
            return rect;
        }

        private void ApplyRealtimeSnapForSelection()
        {
            if (!GraphProcessorEditorSettings.GridSnapActive.Value)
                return;

            foreach (var selectable in selection)
            {
                switch (selectable)
                {
                    case BaseNodeView nodeView when nodeView.Movable:
                    {
                        var rect = nodeView.GetPosition();
                        var snapped = SnapRectPosition(rect);
                        if (snapped.position != rect.position)
                            nodeView.SetPosition(snapped);
                        break;
                    }
                    case GroupView groupView:
                    {
                        var rect = groupView.GetPosition();
                        var snapped = SnapRectPosition(rect);
                        if (snapped.position != rect.position)
                            groupView.SetPosition(snapped);
                        break;
                    }
                    case StickyNoteView stickyNoteView:
                    {
                        var rect = stickyNoteView.GetPosition();
                        var snapped = SnapRectPosition(rect);
                        if (snapped.position != rect.position)
                            stickyNoteView.SetPosition(snapped);
                        break;
                    }
                    case PlacematView placematView:
                    {
                        var rect = placematView.GetPosition();
                        var snapped = SnapRectPosition(rect);
                        if (snapped.position != rect.position)
                            placematView.SetPosition(snapped);
                        break;
                    }
                }
            }
        }

        #region Initialize

        public void SetUp(BaseGraphProcessor graph, GraphViewContext context)
        {
            this.viewModel = graph;
            this.context = context;
        }

        public void Init()
        {
            var coroutine = Context.graphWindow.StartCoroutine(InitCoroutine());
            this.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                Context.graphWindow.StopCoroutine(coroutine);
                Uninit();
            });

            IEnumerator InitCoroutine()
            {
                UpdateInspector();

                viewTransform.position = ViewModel.Pan.ToVector2();
                viewTransform.scale = new Vector3(ViewModel.Zoom, ViewModel.Zoom, 1);

                nodeCreationRequest = NodeCreationRequest;
                graphViewChanged = OnGraphViewChangedCallback;
                viewTransformChanged = OnViewTransformChanged;

                yield return Context.graphWindow.StartCoroutine(GenerateNodeViews());
                yield return Context.graphWindow.StartCoroutine(GenerateConnectionViews());
                yield return Context.graphWindow.StartCoroutine(GenerateGroupViews());
                yield return Context.graphWindow.StartCoroutine(GenerateNoteViews());
                yield return Context.graphWindow.StartCoroutine(GeneratePlacematViews());

                ViewModel.PropertyChanged += OnViewModelChanged;

                ViewModel.GraphEvents.Subscribe<AddNodeEventArgs>(OnNodeAdded);
                ViewModel.GraphEvents.Subscribe<RemoveNodeEventArgs>(OnNodeRemoved);

                ViewModel.GraphEvents.Subscribe<AddConnectionEventArgs>(OnConnected);
                ViewModel.GraphEvents.Subscribe<RemoveConnectionEventArgs>(OnDisconnected);

                ViewModel.GraphEvents.Subscribe<AddGroupEventArgs>(OnGroupAdded);
                ViewModel.GraphEvents.Subscribe<RemoveGroupEventArgs>(OnGroupRemoved);
                ViewModel.GraphEvents.Subscribe<AddNodesToGroupEventArgs>(OnGroupAddNodes);
                ViewModel.GraphEvents.Subscribe<RemoveNodesFromGroupEventArgs>(OnGroupRemoveNodes);

                ViewModel.GraphEvents.Subscribe<AddNoteEventArgs>(OnNoteAdded);
                ViewModel.GraphEvents.Subscribe<RemoveNoteEventArgs>(OnNoteRemoved);
                ViewModel.GraphEvents.Subscribe<AddPlacematEventArgs>(OnPlacematAdded);
                ViewModel.GraphEvents.Subscribe<RemovePlacematEventArgs>(OnPlacematRemoved);
                
                // Bug 修复：协程结束时节点 worldBound 尚未经过一帧布局，延一帧后再刷新可见性
                // 否则所有节点 controls 因 worldBound == Rect.zero 而被错误隐藏
                schedule.Execute(() => UpdateNodeControlsVisibility()).ExecuteLater(0);
                OnCreated();
            }
        }

        private void Uninit()
        {
            this.Query<GraphElement>().ForEach(element =>
            {
                if (element is IGraphElementView bindableView)
                {
                    bindableView.UnInit();
                }
            });
            
            ViewModel.PropertyChanged -= OnViewModelChanged;

            ViewModel.GraphEvents.Unsubscribe<AddNodeEventArgs>(OnNodeAdded);
            ViewModel.GraphEvents.Unsubscribe<RemoveNodeEventArgs>(OnNodeRemoved);

            ViewModel.GraphEvents.Unsubscribe<AddConnectionEventArgs>(OnConnected);
            ViewModel.GraphEvents.Unsubscribe<RemoveConnectionEventArgs>(OnDisconnected);

            ViewModel.GraphEvents.Unsubscribe<AddGroupEventArgs>(OnGroupAdded);
            ViewModel.GraphEvents.Unsubscribe<RemoveGroupEventArgs>(OnGroupRemoved);
            ViewModel.GraphEvents.Unsubscribe<AddNodesToGroupEventArgs>(OnGroupAddNodes);
            ViewModel.GraphEvents.Unsubscribe<RemoveNodesFromGroupEventArgs>(OnGroupRemoveNodes);

            ViewModel.GraphEvents.Unsubscribe<AddNoteEventArgs>(OnNoteAdded);
            ViewModel.GraphEvents.Unsubscribe<RemoveNoteEventArgs>(OnNoteRemoved);
            ViewModel.GraphEvents.Unsubscribe<AddPlacematEventArgs>(OnPlacematAdded);
            ViewModel.GraphEvents.Unsubscribe<RemovePlacematEventArgs>(OnPlacematRemoved);
            
            OnDestroyed();
        }

        /// <summary> 生成所有NodeView </summary>
        private IEnumerator GenerateNodeViews()
        {
            for (int index = 0; index < ViewModel.Model.nodes.Count; index++)
            {
                var node = ViewModel.Model.nodes[index];
                if (node == null) continue;
                var nodeProcessor = ViewModel.Nodes[node.id];
                AddNodeView(nodeProcessor);
                if (index > 0 && index % 10 == 0)
                    yield return null;
            }
        }

        /// <summary> 连接节点 </summary>
        private IEnumerator GenerateConnectionViews()
        {
            for (var index = 0; index < ViewModel.Connections.Count; index++)
            {
                var connection = ViewModel.Connections[index];
                if (connection == null) continue;
                if (!NodeViews.TryGetValue(connection.FromNodeID, out var fromNodeView)) throw new InvalidOperationException($"找不到From节点View: {connection.FromNodeID}");
                if (!NodeViews.TryGetValue(connection.ToNodeID, out var toNodeView)) throw new InvalidOperationException($"找不到To节点View: {connection.ToNodeID}");
                ConnectView(fromNodeView, toNodeView, connection);
                if (index > 0 && index % 10 == 0)
                    yield return null;
            }
        }

        /// <summary> 生成所有GroupView </summary>
        private IEnumerator GenerateGroupViews()
        {
            int step = 0;

            foreach (var group in ViewModel.Groups.GroupMap.Values)
            {
                if (group == null) continue;
                AddGroupView(group);
                step++;
                if (step % 10 == 0)
                    yield return null;
            }
        }

        private IEnumerator GenerateNoteViews()
        {
            for (int index = 0; index < ViewModel.Model.notes.Count; index++)
            {
                var note = ViewModel.Model.notes[index];
                if (note == null) continue;
                var noteProcessor = ViewModel.Notes[note.id];
                if (noteProcessor == null) continue;
                AddNoteView(noteProcessor);
                if (index > 0 && index % 10 == 0)
                    yield return null;
            }
        }

        private IEnumerator GeneratePlacematViews()
        {
            var index = 0;
            foreach (var placemat in ViewModel.Placemats.Values)
            {
                AddPlacematView(placemat);
                if (index > 0 && index % 10 == 0)
                    yield return null;
                index++;
            }
        }

        #endregion

        #region API

        public BaseNodeView GetNodeView(long id)
        {
            return NodeViews.GetValueOrDefault(id);
        }
        
        public GroupView GetGroupView(long id)
        {
            return GroupViews.GetValueOrDefault(id);
        }

        public BaseNodeView AddNodeView(BaseNodeProcessor nodeProcessor)
        {
            var nodeView = NewNodeView(nodeProcessor);
            nodeView.layer = 4;
            this.AddElement(nodeView);
            this.NodeViews[nodeProcessor.ID] = nodeView;
            nodeView.AddToClassList(nodeProcessor.ModelType.Name);
            nodeView.SetUp(nodeProcessor, this);
            nodeView.Init();
            return nodeView;
        }

        public void RemoveNodeView(BaseNodeView nodeView)
        {
            nodeView.UnInit();
            this.RemoveElement(nodeView);
            this.NodeViews.Remove(nodeView.ViewModel.ID);
        }

        public GroupView AddGroupView(GroupProcessor group)
        {
            var groupView = NewGroupView(group);
            groupView.layer = 1;
            this.AddElement(groupView);
            this.GroupViews[group.ID] = groupView;
            groupView.SetUp(group, this);
            groupView.Init();
            return groupView;
        }

        public void RemoveGroupView(GroupView groupView)
        {
            groupView.UnInit();
            // 避免 LINQ ToArray() GC 分配，直接转换为列表
            var contained = new List<GraphElement>(groupView.containedElements);
            groupView.RemoveElementsWithoutNotification(contained);
            this.RemoveElement(groupView);
            this.GroupViews.Remove(groupView.ViewModel.ID);
        }

        public BaseConnectionView ConnectView(BaseNodeView from, BaseNodeView to, BaseConnectionProcessor connection)
        {
            var connectionView = NewConnectionView(connection);
            connectionView.layer = 3;
            connectionView.SetUp(connection, this);
            connectionView.Init();
            connectionView.userData = connection;
            connectionView.output = from.PortViews[connection.FromPortName];
            connectionView.input = to.PortViews[connection.ToPortName];
            from.PortViews[connection.FromPortName].Connect(connectionView);
            to.PortViews[connection.ToPortName].Connect(connectionView);
            AddElement(connectionView);
            ConnectionViews[connection] = connectionView;
            return connectionView;
        }

        public void DisconnectView(BaseConnectionView connectionView)
        {
            BasePortView inputPortView = connectionView.input as BasePortView;
            BaseNodeView inputNodeView = inputPortView.node as BaseNodeView;
            inputPortView.Disconnect(connectionView);

            BasePortView outputPortView = connectionView.output as BasePortView;
            BaseNodeView outputNodeView = outputPortView.node as BaseNodeView;
            outputPortView.Disconnect(connectionView);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            connectionView.UnInit();
            RemoveElement(connectionView);
            ConnectionViews.Remove(connectionView.ViewModel);
        }


        private void AddNoteView(StickyNoteProcessor note)
        {
            var noteView = new StickyNoteView();
            noteView.layer = 2;
            noteView.SetUp(note, this);
            noteView.Init();
            NoteViews[note.ID] = noteView;
            AddElement(noteView);
        }

        private void RemoveNoteView(StickyNoteProcessor note)
        {
            var noteView = NoteViews[note.ID];
            noteView.UnInit();
            RemoveElement(noteView);
            NoteViews.Remove(noteView.ViewModel.ID);
        }

        private void AddPlacematView(PlacematProcessor placemat)
        {
            var placematView = this.placematContainer.CreatePlacemat<PlacematView>(new Rect(placemat.Position.ToVector2(),  placemat.Size.ToVector2()), 0, placemat.Title);
            placematView.SetUp(placemat, this);
            placematView.Init();
            PlacematViews.Add(placemat.ID, placematView);
            placematView.SendToBack();
        }

        private void RemovePlacematView(PlacematProcessor placemat)
        {
            if (!PlacematViews.TryGetValue(placemat.ID, out var placematView))
                return;

            placematView.UnInit();
            PlacematViews.Remove(placemat.ID);
            this.placematContainer.Remove(placematView);
        }

        /// <summary> 获取鼠标在GraphView中的坐标，如果鼠标不在GraphView内，则返回当前GraphView显示的中心点 </summary>
        public Vector2 GetMousePosition()
        {
            if (Event.current != null && worldBound.Contains(Event.current.mousePosition))
                return contentViewContainer.WorldToLocal(Event.current.mousePosition);
            return contentViewContainer.WorldToLocal(worldBound.center);
        }

        public sealed override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            UpdateInspector();
        }

        public void AddToSelection(IEnumerable<ISelectable> selectables)
        {
            foreach (var selectable in selectables)
            {
                base.AddToSelection(selectable);
            }

            UpdateInspector();
        }

        public sealed override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            UpdateInspector();
        }

        public void RemoveFromSelection(IEnumerable<ISelectable> selectables)
        {
            foreach (var selectable in selectables)
            {
                base.RemoveFromSelection(selectable);
            }

            UpdateInspector();
        }

        public sealed override void ClearSelection()
        {
            base.ClearSelection();
            UpdateInspector();
        }

        // 标记Dirty
        public void SetDirty()
        {
            this.Context.graphWindow?.SetHasUnsavedChanges(true);
        }

        #endregion

        #region Callbacks

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            var graph = sender as BaseGraphProcessor;
            switch (e.PropertyName)
            {
                case nameof(BaseGraph.pan):
                {
                    viewTransform.position = graph.Pan.ToVector2();
                    SetDirty();
                    break;
                }
                case nameof(BaseGraph.zoom):
                {
                    viewTransform.scale = new Vector3(graph.Zoom, graph.Zoom, 1);
                    SetDirty();
                    break;
                }
            }
        }

        private void OnNodeAdded(AddNodeEventArgs args)
        {
            AddNodeView(args.Node);
            SetDirty();
        }

        private void OnNodeRemoved(RemoveNodeEventArgs args)
        {
            RemoveNodeView(NodeViews[args.Node.ID]);
            SetDirty();
        }

        private void OnNoteAdded(AddNoteEventArgs args)
        {
            AddNoteView(args.Note);
            SetDirty();
        }

        private void OnNoteRemoved(RemoveNoteEventArgs args)
        {
            RemoveNoteView(args.Note);
            SetDirty();
        }

        private void OnPlacematAdded(AddPlacematEventArgs args)
        {
            AddPlacematView(args.Placemat);
            SetDirty();
        }

        private void OnPlacematRemoved(RemovePlacematEventArgs args)
        {
            RemovePlacematView(args.Placemat);
            SetDirty();
        }

        private void OnGroupAdded(AddGroupEventArgs args)
        {
            AddGroupView(args.Group);
            SetDirty();
        }

        private void OnGroupRemoved(RemoveGroupEventArgs args)
        {
            RemoveGroupView(GroupViews[args.Group.ID]);
            SetDirty();
        }

        private void OnGroupAddNodes(AddNodesToGroupEventArgs args)
        {
            var groupView = GetGroupView(args.Group.ID);
            if (groupView == null) return;
            groupView.OnNodesAdded(args.Node);
        }

        private void OnGroupRemoveNodes(RemoveNodesFromGroupEventArgs args)
        {
            var groupView = GetGroupView(args.Group.ID);
            if (groupView == null) return;
            groupView.OnNodesRemoved(args.Node);
        }

        private void OnConnected(AddConnectionEventArgs args)
        {
            var from = NodeViews[args.Connection.FromNodeID];
            var to = NodeViews[args.Connection.ToNodeID];
            ConnectView(from, to, args.Connection);
            SetDirty();
        }

        private void OnDisconnected(RemoveConnectionEventArgs args)
        {
            // 直接通过字典 O(1) 查找，避免全量遍历 edges
            if (ConnectionViews.TryGetValue(args.Connection, out var connectionView))
            {
                DisconnectView(connectionView);
            }
            SetDirty();
        }

        private void NodeCreationRequest(NodeCreationContext c)
        {
            var nodeMenu = ScriptableObject.CreateInstance<NodeMenuWindow>();
            nodeMenu.Initialize("Nodes", this);

            BuildNodeMenu(nodeMenu);

            var multiLayereEntryCount = 0;
            for (int i = 0; i < nodeMenu.entries.Count; i++)
            {
                if (nodeMenu.entries[i].Menu.Length > 1)
                    multiLayereEntryCount++;
            }

            nodeMenu.entries.QuickSort((a, b) => -(a.Menu.Length.CompareTo(b.Menu.Length)));
            // 添加边界检查，防止 multiLayereEntryCount 为 0 或等于 entries.Count 时越界
            if (multiLayereEntryCount > 0)
                nodeMenu.entries.QuickSort(0, multiLayereEntryCount - 1, (a, b) => String.Compare(a.Path, b.Path, StringComparison.Ordinal));
            if (multiLayereEntryCount < nodeMenu.entries.Count)
                nodeMenu.entries.QuickSort(multiLayereEntryCount, nodeMenu.entries.Count - 1, (a, b) => String.Compare(a.Path, b.Path, StringComparison.Ordinal));

            SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), nodeMenu);
        }

        /// <summary> GraphView发生改变时调用 </summary>
        private GraphViewChange OnGraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.movedElements != null)
            {
                // 当节点移动之后，与之连接的接口重新排序
                var newPos = new Dictionary<IGraphElementProcessor_Scope, Rect>();
                var portsHashset = new HashSet<PortProcessor>();

                changes.movedElements.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case StickyNoteView stickyNoteView:
                        {
                            newPos[stickyNoteView.ViewModel] = SnapRectPosition(stickyNoteView.GetPosition());
                            return true;
                        }
                        case PlacematView placematView:
                        {
                            newPos[placematView.ViewModel] = SnapRectPosition(placematView.GetPosition());
                            return true;
                        }
                        case BaseNodeView nodeView:
                        {
                            if (nodeView.Movable)
                            {
                                newPos[nodeView.ViewModel] = SnapRectPosition(nodeView.GetPosition());
                                // 记录需要重新排序的接口
                                foreach (var port in nodeView.ViewModel.Ports.Values)
                                {
                                    foreach (var connection in port.Connections)
                                    {
                                        if (port.Direction == BasePort.Direction.Left)
                                            portsHashset.Add(connection.FromNode.Ports[connection.FromPortName]);
                                        else
                                            portsHashset.Add(connection.ToNode.Ports[connection.ToPortName]);
                                    }
                                }
                            }

                            return true;
                        }
                        case GroupView groupView:
                        {
                            newPos[groupView.ViewModel] = SnapRectPosition(groupView.GetPosition());
                            foreach (var nodeId in groupView.ViewModel.Nodes)
                            {
                                var node = ViewModel.Nodes[nodeId];
                                var nodeView = NodeViews[nodeId];
                                if (nodeView.Movable)
                                {
                                    newPos[node] = SnapRectPosition(nodeView.GetPosition());
                                }
                            }

                            return true;
                        }
                    }

                    return false;
                });

                if (newPos.Count > 0)
                {
                    // 排序
                    foreach (var port in portsHashset)
                    {
                        port.Trim();
                    }

                    this.Context.Do(new MoveElementsCommand(newPos));
                }

                // 兜底：每次移动结算后确保 placemat 保持在最底层
                foreach (var placematView in PlacematViews.Values)
                    placematView.SendToBack();
            }

            if (changes.elementsToRemove != null)
            {
                // 避免 LINQ Where/Select/ToArray 产生 GC，改用预分配 List
                var graphElementsList = new List<IGraphElementProcessor>(changes.elementsToRemove.Count);
                foreach (var item in changes.elementsToRemove)
                {
                    if (item is IGraphElementView gev)
                        graphElementsList.Add(gev.V);
                }
                changes.elementsToRemove.RemoveAll(item => item is IGraphElementView);
                if (graphElementsList.Count > 0)
                    this.Context.Do(new RemoveElementsCommand(ViewModel, graphElementsList.ToArray()));
            }

            UpdateInspector();
            return changes;
        }

        /// <summary> 转换发生改变时调用 </summary>
        private void OnViewTransformChanged(GraphView view)
        {
            ViewModel.Zoom = viewTransform.scale.x;
            ViewModel.Pan = viewTransform.position.ToInternalVector3Int();
            // 视图变换后更新节点 controls 可见性（事件驱动，替代原定时轮询）
            UpdateNodeControlsVisibility();
        }

        private void OnMouseMoveRealtimeSnap(MouseMoveEvent evt)
        {
            if ((evt.pressedButtons & 1) == 0)
                return;

            ApplyRealtimeSnapForSelection();
        }

        private void OnMouseUpRealtimeSnap(MouseUpEvent evt)
        {
            if (evt.button != 0)
                return;

            ApplyRealtimeSnapForSelection();
        }

        private void OnMouseCaptureOutRealtimeSnap(MouseCaptureOutEvent evt)
        {
            ApplyRealtimeSnapForSelection();
        }

        #endregion
    }
}
#endif
