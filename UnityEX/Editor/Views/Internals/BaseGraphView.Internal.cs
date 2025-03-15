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

        private GraphViewContext Context => context;

        public BaseGraphWindow GraphWindow => Context.window;

        public CommandDispatcher CommandDispatcher => Context.commandDispatcher;

        public IGraphAsset GraphAsset => GraphWindow.GraphAsset;

        public Dictionary<int, BaseNodeView> NodeViews { get; } = new Dictionary<int, BaseNodeView>();

        public Dictionary<int, StickyNoteView> NoteViews { get; } = new Dictionary<int, StickyNoteView>();

        public Dictionary<int, GroupView> GroupViews { get; } = new Dictionary<int, GroupView>();

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
        }

        #region Initialize

        public void SetUp(BaseGraphProcessor graph, GraphViewContext context)
        {
            this.viewModel = graph;
            this.context = context;
        }

        public void Init()
        {
            var coroutine = GraphWindow.StartCoroutine(InitCoroutine());
            this.RegisterCallback<DetachFromPanelEvent>(evt => { GraphWindow.StopCoroutine(coroutine); });
            this.RegisterCallback<DetachFromPanelEvent>(evt => { Uninit(); });

            IEnumerator InitCoroutine()
            {
                UpdateInspector();

                viewTransform.position = ViewModel.Pan.ToVector2();
                viewTransform.scale = new Vector3(ViewModel.Zoom, ViewModel.Zoom, 1);

                nodeCreationRequest = NodeCreationRequest;
                graphViewChanged = OnGraphViewChangedCallback;
                viewTransformChanged = OnViewTransformChanged;

                yield return GraphWindow.StartCoroutine(GenerateNodeViews());
                yield return GraphWindow.StartCoroutine(GenerateConnectionViews());
                yield return GraphWindow.StartCoroutine(GenerateGroupViews());
                yield return GraphWindow.StartCoroutine(GenerateNoteViews());

                BindEvents();
                OnInitialized();
            }
        }

        private void Uninit()
        {
            this.Query<GraphElement>().ForEach(element =>
            {
                if (element is IGraphElementView bindableView)
                {
                    bindableView.OnDestroy();
                }
            });
            UnbindEvents();
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

        private void BindEvents()
        {
            ViewModel.PropertyChanged += OnViewModelChanged;

            ViewModel.OnNodeAdded += OnNodeAdded;
            ViewModel.OnNodeRemoved += OnNodeRemoved;

            ViewModel.OnGroupAdded += OnGroupAdded;
            ViewModel.OnGroupRemoved += OnGroupRemoved;

            ViewModel.OnConnected += OnConnected;
            ViewModel.OnDisconnected += OnDisconnected;

            ViewModel.OnNoteAdded += OnNoteAdded;
            ViewModel.OnNoteRemoved += OnNoteRemoved;
        }

        private void UnbindEvents()
        {
            ViewModel.PropertyChanged -= OnViewModelChanged;

            ViewModel.OnNodeAdded -= OnNodeAdded;
            ViewModel.OnNodeRemoved -= OnNodeRemoved;

            ViewModel.OnGroupAdded -= OnGroupAdded;
            ViewModel.OnGroupRemoved -= OnGroupRemoved;

            ViewModel.OnConnected -= OnConnected;
            ViewModel.OnDisconnected -= OnDisconnected;

            ViewModel.OnNoteAdded -= OnNoteAdded;
            ViewModel.OnNoteRemoved -= OnNoteRemoved;
        }

        #endregion

        #region API

        public BaseNodeView AddNodeView(BaseNodeProcessor nodeProcessor)
        {
            var nodeView = NewNodeView(nodeProcessor);
            nodeView.AddToClassList(nodeProcessor.ModelType.Name);
            nodeView.SetUp(nodeProcessor, this);
            nodeView.OnCreate();
            NodeViews[nodeProcessor.ID] = nodeView;
            AddElement(nodeView);
            return nodeView;
        }

        public void RemoveNodeView(BaseNodeView nodeView)
        {
            nodeView.OnDestroy();
            RemoveElement(nodeView);
            NodeViews.Remove(nodeView.ViewModel.ID);
        }

        public GroupView AddGroupView(GroupProcessor group)
        {
            var groupView = NewGroupView(group);
            groupView.SetUp(group, this);
            groupView.OnCreate();
            GroupViews[group.ID] = groupView;
            AddElement(groupView);
            return groupView;
        }

        public void RemoveGroupView(GroupView groupView)
        {
            groupView.OnDestroy();
            groupView.RemoveElementsWithoutNotification(groupView.containedElements.ToArray());
            RemoveElement(groupView);
            GroupViews.Remove(groupView.ViewModel.ID);
        }

        public BaseConnectionView ConnectView(BaseNodeView from, BaseNodeView to, BaseConnectionProcessor connection)
        {
            var connectionView = NewConnectionView(connection);
            connectionView.SetUp(connection, this);
            connectionView.OnCreate();
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

            connectionView.OnDestroy();
            RemoveElement(connectionView);
            ConnectionViews.Remove(connectionView.ViewModel);
        }


        private void AddNoteView(StickyNoteProcessor note)
        {
            var noteView = new StickyNoteView();
            noteView.SetUp(note, this);
            noteView.OnCreate();
            NoteViews[note.ID] = noteView;
            AddElement(noteView);
        }

        private void RemoveNoteView(StickyNoteProcessor note)
        {
            var noteView = NoteViews[note.ID];
            noteView.OnDestroy();
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
            GraphWindow?.SetHasUnsavedChanges(true);
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

        private void OnNodeAdded(BaseNodeProcessor node)
        {
            AddNodeView(node);
            SetDirty();
        }

        private void OnNodeRemoved(BaseNodeProcessor node)
        {
            RemoveNodeView(NodeViews[node.ID]);
            SetDirty();
        }

        private void OnNoteAdded(StickyNoteProcessor note)
        {
            AddNoteView(note);
            SetDirty();
        }

        private void OnNoteRemoved(StickyNoteProcessor note)
        {
            RemoveNoteView(note);
            SetDirty();
        }

        private void OnGroupAdded(GroupProcessor group)
        {
            AddGroupView(group);
            SetDirty();
        }

        private void OnGroupRemoved(GroupProcessor group)
        {
            RemoveGroupView(GroupViews[group.ID]);
            SetDirty();
        }

        private void OnConnected(BaseConnectionProcessor connection)
        {
            var from = NodeViews[connection.FromNodeID];
            var to = NodeViews[connection.ToNodeID];
            ConnectView(from, to, connection);
            SetDirty();
        }

        private void OnDisconnected(BaseConnectionProcessor connection)
        {
            edges.ForEach(edge =>
            {
                if (edge.userData != connection) return;
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
                var portsHashset = new HashSet<BasePortProcessor>();

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

                    CommandDispatcher.Do(new MoveElementsCommand(newPos));
                }
            }

            if (changes.elementsToRemove != null)
            {
                var graphElements = changes.elementsToRemove
                    .Where(item => item.selected && item is IGraphElementView)
                    .Select(item => ((IGraphElementView)item).V).ToArray();
                changes.elementsToRemove.RemoveAll(item => item is IGraphElementView);
                CommandDispatcher.Do(new RemoveElementsCommand(ViewModel, graphElements));
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