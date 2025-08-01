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
using System.Linq;
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
            this.schedule.Execute(() =>
            {
                foreach (var pair in this.NodeViews)
                {
                    if (!this.worldBound.Overlaps(pair.Value.worldBound))
                    {
                        pair.Value.controls.visible = false;
                    }
                    else
                    {
                        pair.Value.controls.visible = true;
                    }
                }
            }).Every(50);
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
                if (!NodeViews.TryGetValue(connection.FromNodeID, out var fromNodeView)) throw new NullReferenceException($"找不到From节点{connection.FromNodeID}");
                if (!NodeViews.TryGetValue(connection.ToNodeID, out var toNodeView)) throw new NullReferenceException($"找不到To节点{connection.ToNodeID}");
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
            this.AddElement(groupView);
            this.GroupViews[group.ID] = groupView;
            groupView.SetUp(group, this);
            groupView.Init();
            return groupView;
        }

        public void RemoveGroupView(GroupView groupView)
        {
            groupView.UnInit();
            groupView.RemoveElementsWithoutNotification(groupView.containedElements.ToArray());
            this.RemoveElement(groupView);
            this.GroupViews.Remove(groupView.ViewModel.ID);
        }

        public BaseConnectionView ConnectView(BaseNodeView from, BaseNodeView to, BaseConnectionProcessor connection)
        {
            var connectionView = NewConnectionView(connection);
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
            groupView.OnNodesAdded(args.Nodes);
        }

        private void OnGroupRemoveNodes(RemoveNodesFromGroupEventArgs args)
        {
            var groupView = GetGroupView(args.Group.ID);
            groupView.OnNodesRemoved(args.Nodes);
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
            edges.ForEach(edge =>
            {
                if (edge.userData != args.Connection) return;
                DisconnectView(edge as BaseConnectionView);
            });
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
            nodeMenu.entries.QuickSort(0, multiLayereEntryCount - 1, (a, b) => String.Compare(a.Path, b.Path, StringComparison.Ordinal));
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
                        // case StickyNoteView stickyNoteView:
                        // {
                        //     newPos[stickyNoteView.ViewModel] = stickyNoteView.GetPosition();
                        //     return true;
                        // }
                        case BaseNodeView nodeView:
                        {
                            if (nodeView.Movable)
                            {
                                newPos[nodeView.ViewModel] = nodeView.GetPosition();
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
                            newPos[groupView.ViewModel] = groupView.GetPosition();
                            foreach (var nodeId in groupView.ViewModel.Nodes)
                            {
                                var node = ViewModel.Nodes[nodeId];
                                var nodeView = NodeViews[nodeId];
                                if (nodeView.Movable)
                                {
                                    newPos[node] = nodeView.GetPosition();
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
            }

            if (changes.elementsToRemove != null)
            {
                var graphElements = changes.elementsToRemove
                    .Where(item => item.selected && item is IGraphElementView)
                    .Select(item => ((IGraphElementView)item).V).ToArray();
                changes.elementsToRemove.RemoveAll(item => item is IGraphElementView);
                this.Context.Do(new RemoveElementsCommand(ViewModel, graphElements));
            }

            UpdateInspector();
            return changes;
        }

        /// <summary> 转换发生改变时调用 </summary>
        private void OnViewTransformChanged(GraphView view)
        {
            ViewModel.Zoom = viewTransform.scale.x;
            ViewModel.Pan = viewTransform.position.ToInternalVector3Int();
        }

        #endregion
    }
}
#endif