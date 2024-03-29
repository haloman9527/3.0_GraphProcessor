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
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGraphView : GraphView
    {
        #region Properties

        private MiniMap miniMap;
        private BaseGraphProcessor viewModel;
        private BaseGraphWindow graphWindow;
        private CommandDispatcher commandDispatcher;
        private Dictionary<int, BaseNodeView> nodeViews = new Dictionary<int, BaseNodeView>();
        private Dictionary<int, BaseGroupView> groupViews = new Dictionary<int, BaseGroupView>();
        private Dictionary<BaseConnectionProcessor, BaseConnectionView> connectionViews = new Dictionary<BaseConnectionProcessor, BaseConnectionView>();
        private Dictionary<int, StickNoteView> noteViews = new Dictionary<int, StickNoteView>();

        public BaseGraphWindow GraphWindow
        {
            get { return graphWindow; }
        }

        public CommandDispatcher CommandDispatcher
        {
            get { return commandDispatcher; }
        }

        public IGraphAsset GraphAsset
        {
            get { return GraphWindow.GraphAsset; }
        }

        public BaseGraphProcessor ViewModel
        {
            get { return viewModel; }
        }

        public Dictionary<int, BaseNodeView> NodeViews
        {
            get { return nodeViews; }
        }

        public Dictionary<int, StickNoteView> NoteViews
        {
            get { return noteViews; }
        }

        public Dictionary<int, BaseGroupView> GroupViews
        {
            get { return groupViews; }
        }

        public Dictionary<BaseConnectionProcessor, BaseConnectionView> ConnectionViews
        {
            get { return connectionViews; }
        }

        public bool MiniMapActive
        {
            get { return miniMap != null; }
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

        public BaseGraphView(BaseGraphProcessor graph, BaseGraphWindow window, CommandDispatcher commandDispatcher)
        {
            styleSheets.Add(GraphProcessorStyles.BaseGraphViewStyle);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.05f, maxScale = 4f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();

            this.viewModel = graph;
            this.graphWindow = window;
            this.commandDispatcher = commandDispatcher;
        }

        #region Initialize

        public void Init()
        {
            var coroutine = graphWindow.StartCoroutine(InitCoroutine());
            this.RegisterCallback<DetachFromPanelEvent>(evt => { graphWindow.StopCoroutine(coroutine); });
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
                yield return GraphWindow.StartCoroutine(LinkNodeViews());
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
            int step = 0;
            foreach (var node in ViewModel.Nodes.Values)
            {
                if (node == null) continue;
                AddNodeView(node);
                step++;
                if (step % 10 == 0)
                    yield return null;
            }
        }

        /// <summary> 连接节点 </summary>
        private IEnumerator LinkNodeViews()
        {
            int step = 0;
            foreach (var connection in ViewModel.Connections)
            {
                if (connection == null) continue;
                if (!NodeViews.TryGetValue(connection.FromNodeID, out var fromNodeView)) throw new NullReferenceException($"找不到From节点{connection.FromNodeID}");
                if (!NodeViews.TryGetValue(connection.ToNodeID, out var toNodeView)) throw new NullReferenceException($"找不到To节点{connection.ToNodeID}");
                ConnectView(fromNodeView, toNodeView, connection);
                step++;
                if (step % 10 == 0)
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
            int step = 0;
            foreach (var pair in ViewModel.Notes)
            {
                if (pair.Value == null) continue;
                AddNoteView(pair.Value);
                step++;
                if (step % 10 == 0)
                    yield return null;
            }
        }

        private void BindEvents()
        {
            RegisterCallback<KeyDownEvent>(KeyDownCallback);

            ViewModel.BindProperty<InternalVector2Int>(nameof(BaseGraph.pan), OnPositionChanged);
            ViewModel.BindProperty<float>(nameof(BaseGraph.zoom), OnZoomChanged);

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
            UnregisterCallback<KeyDownEvent>(KeyDownCallback);

            ViewModel.UnBindProperty<InternalVector2Int>(nameof(BaseGraph.pan), OnPositionChanged);
            ViewModel.UnBindProperty<float>(nameof(BaseGraph.zoom), OnZoomChanged);

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

        public BaseNodeView AddNodeView(BaseNodeProcessor node)
        {
            var nodeView = NewNodeView(node);
            nodeView.AddToClassList(node.ModelType.Name);
            nodeView.SetUp(node, this);
            nodeView.OnCreate();
            NodeViews[node.ID] = nodeView;
            AddElement(nodeView);
            return nodeView;
        }

        public void RemoveNodeView(BaseNodeView nodeView)
        {
            nodeView.OnDestroy();
            RemoveElement(nodeView);
            NodeViews.Remove(nodeView.ViewModel.ID);
        }

        public BaseGroupView AddGroupView(BaseGroupProcessor group)
        {
            var groupView = NewGroupView(group);
            groupView.SetUp(group, this);
            groupView.OnCreate();
            GroupViews[group.ID] = groupView;
            AddElement(groupView);
            return groupView;
        }

        public void RemoveGroupView(BaseGroupView groupView)
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


        private void AddNoteView(StickNoteProcessor note)
        {
            var noteView = new StickNoteView();
            noteView.SetUp(note, this);
            noteView.OnCreate();
            NoteViews[note.ID] = noteView;
            AddElement(noteView);
        }

        private void RemoveNoteView(StickNoteProcessor note)
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
            graphWindow?.SetGraphDirty();
        }

        public void SetUnDirty()
        {
            graphWindow?.SetGraphUndirty();
        }

        #endregion

        #region Callbacks

        private void OnPositionChanged(InternalVector2Int oldPosition, InternalVector2Int position)
        {
            viewTransform.position = position.ToVector2();
            SetDirty();
        }

        private void OnZoomChanged(float oldZoom, float zoom)
        {
            viewTransform.scale = new Vector3(zoom, zoom, 1);
            SetDirty();
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

        private void OnNoteAdded(StickNoteProcessor note)
        {
            AddNoteView(note);
            SetDirty();
        }

        private void OnNoteRemoved(StickNoteProcessor note)
        {
            RemoveNoteView(note);
            SetDirty();
        }

        private void OnGroupAdded(BaseGroupProcessor group)
        {
            AddGroupView(group);
            SetDirty();
        }

        private void OnGroupRemoved(BaseGroupProcessor group)
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

        private void KeyDownCallback(KeyDownEvent evt)
        {
            if (evt.commandKey || evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.Z:
                        CommandDispatcher.Undo();
                        evt.StopPropagation();
                        break;
                    case KeyCode.Y:
                        CommandDispatcher.Redo();
                        evt.StopPropagation();
                        break;
                    default:
                        break;
                }
            }
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
                var newPos = new Dictionary<IGraphScopeViewModel, Rect>();
                // Dictionary<BaseGroupProcessor, InternalVector2Int> groupNewPos = new Dictionary<BaseGroupProcessor, InternalVector2Int>();
                HashSet<BasePortProcessor> portsHashset = new HashSet<BasePortProcessor>();

                changes.movedElements.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case BaseNodeView nodeView:
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

                            return true;
                        }
                        case BaseGroupView groupView:
                        {
                            newPos[groupView.ViewModel] = groupView.GetPosition();
                            foreach (var nodeGUID in groupView.ViewModel.Nodes)
                            {
                                var node = ViewModel.Nodes[nodeGUID];
                                var nodeView = NodeViews[nodeGUID];
                                newPos[node] = nodeView.GetPosition();
                            }

                            return true;
                        }
                    }

                    return false;
                });

                if (newPos.Count > 0)
                {
                    CommandDispatcher.BeginGroup();

                    // 排序
                    foreach (var port in portsHashset)
                    {
                        port.Resort();
                    }

                    CommandDispatcher.Do(new MoveElementsCommand(newPos));

                    CommandDispatcher.EndGroup();
                }
            }

            if (changes.elementsToRemove == null)
                return changes;

            CommandDispatcher.BeginGroup();

            var groups = changes.elementsToRemove
                .Where(item => item.selected && item is BaseGroupView)
                .Select(item => (item as BaseGroupView).ViewModel).ToArray();
            changes.elementsToRemove.RemoveAll(item => item is BaseGroupView);
            CommandDispatcher.Do(new RemoveGroupsCommand(ViewModel, groups));

            var edges = changes.elementsToRemove
                .Where(item => item.selected && item is BaseConnectionView)
                .Select(item => (item as BaseConnectionView).ViewModel).ToArray();
            changes.elementsToRemove.RemoveAll(item => item is BaseConnectionView);
            CommandDispatcher.Do(new DisconnectsCommand(ViewModel, edges));

            var nodes = changes.elementsToRemove
                .Where(item => item.selected && item is BaseNodeView)
                .Select(item => (item as BaseNodeView).ViewModel).ToArray();
            changes.elementsToRemove.RemoveAll(item => item is BaseNodeView);
            CommandDispatcher.Do(new RemoveNodesCommand(ViewModel, nodes));

            CommandDispatcher.EndGroup();

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