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
using CZToolKit.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CZToolKit.Common.Collection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGraphView : GraphView, IGraphElementView<BaseGraphVM>
    {
        public class NodeEntry
        {
            public readonly string path;
            public readonly string[] menu;
            public readonly bool hidden;
            public readonly Type nodeType;
            public string port;

            public NodeEntry(Type nodeType, string path, string[] menu, bool hidden)
            {
                this.nodeType = nodeType;
                this.path = path;
                this.menu = menu;
                this.hidden = hidden;
            }
        }

        #region Properties

        public event Action onDirty;
        public event Action onUndirty;

        public BaseGraphWindow GraphWindow { get; private set; }
        public CommandDispatcher CommandDispatcher { get; private set; }

        public UnityObject GraphAsset
        {
            get { return GraphWindow.GraphAsset; }
        }

        public Dictionary<int, BaseNodeView> NodeViews { get; private set; } = new Dictionary<int, BaseNodeView>();
        public Dictionary<BaseGroupVM, BaseGroupView> GroupViews { get; private set; } = new Dictionary<BaseGroupVM, BaseGroupView>();
        public Dictionary<BaseConnectionVM, BaseConnectionView> ConnectionViews { get; private set; } = new Dictionary<BaseConnectionVM, BaseConnectionView>();

        public BaseGraphVM ViewModel { get; set; }

        #region 不建议使用自带复制粘贴功能，建议自己实现

        protected sealed override bool canCopySelection => false;
        protected sealed override bool canCutSelection => false;
        protected sealed override bool canDuplicateSelection => false;
        protected sealed override bool canPaste => false;

        #endregion

        #endregion

        public BaseGraphView(BaseGraphVM graph, BaseGraphWindow window, CommandDispatcher commandDispatcher)
        {
            styleSheets.Add(GraphProcessorStyles.GraphViewStyle);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.05f, maxScale = 4f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();

            ViewModel = graph;
            GraphWindow = window;
            CommandDispatcher = commandDispatcher;
        }

        #region Initialize

        public IEnumerator Initialize()
        {
            UpdateInspector();

            viewTransform.position = ViewModel.Pan.ToVector2();
            viewTransform.scale = new Vector3(ViewModel.Zoom, ViewModel.Zoom, 1);
            yield return GraphWindow.StartCoroutine(GenerateNodeViews());
            yield return GraphWindow.StartCoroutine(LinkNodeViews());
            yield return GraphWindow.StartCoroutine(GenerateGroupViews());

            nodeCreationRequest = NodeCreationRequest;
            graphViewChanged = OnGraphViewChangedCallback;
            viewTransformChanged = OnViewTransformChanged;

            RegisterCallback<KeyDownEvent>(KeyDownCallback);

            OnCreate();
        }

        /// <summary> 生成所有NodeView </summary>
        IEnumerator GenerateNodeViews()
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
        IEnumerator LinkNodeViews()
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
        IEnumerator GenerateGroupViews()
        {
            int step = 0;
            foreach (var group in ViewModel.Groups)
            {
                if (group == null) continue;
                AddGroupView(group);
                step++;
                if (step % 10 == 0)
                    yield return null;
            }
        }

        #endregion

        #region API

        public void OnCreate()
        {
            RegisterCallback<DetachFromPanelEvent>(evt => { OnDestroy(); });

            ViewModel.BindingProperty<InternalVector2Int>(nameof(BaseGraph.pan), OnPositionChanged);
            ViewModel.BindingProperty<float>(nameof(BaseGraph.zoom), OnZoomChanged);

            ViewModel.OnNodeAdded += OnNodeAdded;
            ViewModel.OnNodeRemoved += OnNodeRemoved;

            ViewModel.OnGroupAdded += OnGroupAdded;
            ViewModel.OnGroupRemoved += OnGroupRemoved;

            ViewModel.OnConnected += OnConnected;
            ViewModel.OnDisconnected += OnDisconnected;
            
            OnCreated();
        }

        public virtual void OnDestroy()
        {
            this.Query<GraphElement>().ForEach(element =>
            {
                if (element is IGraphElementView bindableView)
                {
                    bindableView.OnDestroy();
                }
            });

            ViewModel.UnBindingProperty<InternalVector2Int>(nameof(BaseGraph.pan), OnPositionChanged);
            ViewModel.UnBindingProperty<float>(nameof(BaseGraph.zoom), OnZoomChanged);

            ViewModel.OnNodeAdded -= OnNodeAdded;
            ViewModel.OnNodeRemoved -= OnNodeRemoved;

            ViewModel.OnConnected -= OnConnected;
            ViewModel.OnDisconnected -= OnDisconnected;

            OnDestroyed();
        }

        public BaseNodeView AddNodeView(BaseNodeVM node)
        {
            BaseNodeView nodeView = NewNodeView(node);
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

        public BaseGroupView AddGroupView(BaseGroupVM group)
        {
            BaseGroupView groupView = NewGroupView(group);
            groupView.SetUp(group, this);
            groupView.OnCreate();
            GroupViews[group] = groupView;
            AddElement(groupView);
            return groupView;
        }

        public void RemoveGroupView(BaseGroupView groupView)
        {
            groupView.OnDestroy();
            groupView.RemoveElementsWithoutNotification(groupView.containedElements.ToArray());
            RemoveElement(groupView);
            GroupViews.Remove(groupView.ViewModel);
        }

        public BaseConnectionView ConnectView(BaseNodeView from, BaseNodeView to, BaseConnectionVM connection)
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

        /// <summary> 获取鼠标在GraphView中的坐标，如果鼠标不在GraphView内，则返回当前GraphView显示的中心点 </summary>
        public Vector2 GetMousePosition()
        {
            if (worldBound.Contains(Event.current.mousePosition))
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
            onDirty?.Invoke();
        }

        public void SetUnDirty()
        {
            onUndirty?.Invoke();
        }

        #endregion

        #region Callbacks

        void OnPositionChanged(InternalVector2Int position)
        {
            viewTransform.position = position.ToVector2();
            SetDirty();
        }

        void OnZoomChanged(float zoom)
        {
            viewTransform.scale = new Vector3(zoom, zoom, 1);
            SetDirty();
        }

        void OnNodeAdded(BaseNodeVM node)
        {
            AddNodeView(node);
            SetDirty();
        }

        void OnNodeRemoved(BaseNodeVM node)
        {
            RemoveNodeView(NodeViews[node.ID]);
            SetDirty();
        }

        void OnGroupAdded(BaseGroupVM group)
        {
            AddGroupView(group);
            SetDirty();
        }

        void OnGroupRemoved(BaseGroupVM group)
        {
            RemoveGroupView(GroupViews[group]);
            SetDirty();
        }

        void OnConnected(BaseConnectionVM connection)
        {
            var from = NodeViews[connection.FromNodeID];
            var to = NodeViews[connection.ToNodeID];
            ConnectView(from, to, connection);
            SetDirty();
        }

        void OnDisconnected(BaseConnectionVM connection)
        {
            edges.ForEach(edge =>
            {
                if (edge.userData != connection) return;
                DisconnectView(edge as BaseConnectionView);
            });
            SetDirty();
        }

        void KeyDownCallback(KeyDownEvent evt)
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

        void NodeCreationRequest(NodeCreationContext c)
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
        GraphViewChange OnGraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.movedElements != null)
            {
                CommandDispatcher.BeginGroup();
                // 当节点移动之后，与之连接的接口重新排序
                Dictionary<BaseNodeVM, InternalVector2Int> newPos = new Dictionary<BaseNodeVM, InternalVector2Int>();
                Dictionary<BaseGroupVM, InternalVector2Int> groupNewPos = new Dictionary<BaseGroupVM, InternalVector2Int>();
                HashSet<BasePortVM> portsHashset = new HashSet<BasePortVM>();

                changes.movedElements.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case BaseNodeView nodeView:
                            newPos[nodeView.ViewModel] = nodeView.GetPosition().position.ToInternalVector2Int();
                            // 记录需要重新排序的接口
                            foreach (var port in nodeView.ViewModel.Ports.Values)
                            {
                                foreach (var connection in port.Connections)
                                {
                                    if (port.Direction == BasePort.Direction.Input)
                                        portsHashset.Add(connection.FromNode.Ports[connection.FromPortName]);
                                    else
                                        portsHashset.Add(connection.ToNode.Ports[connection.ToPortName]);
                                }
                            }

                            return true;
                        case BaseGroupView groupView:
                            groupNewPos[groupView.ViewModel] = groupView.GetPosition().position.ToInternalVector2Int();
                            return true;
                        default:
                            break;
                    }

                    return false;
                });
                foreach (var pair in groupNewPos)
                {
                    foreach (var nodeGUID in pair.Key.Nodes)
                    {
                        var node = ViewModel.Nodes[nodeGUID];
                        var nodeView = NodeViews[nodeGUID];
                        newPos[node] = nodeView.GetPosition().position.ToInternalVector2Int();
                    }
                }

                CommandDispatcher.Do(new MoveNodesCommand(newPos));
                CommandDispatcher.Do(new MoveGroupsCommand(groupNewPos));
                // 排序
                foreach (var port in portsHashset)
                {
                    port.Resort();
                }

                CommandDispatcher.EndGroup();
            }

            if (changes.elementsToRemove == null)
                return changes;

            changes.elementsToRemove.Sort((element1, element2) =>
            {
                int GetPriority(GraphElement element)
                {
                    switch (element)
                    {
                        case Edge edgeView:
                            return 0;
                        case Node nodeView:
                            return 1;
                    }

                    return 4;
                }

                return GetPriority(element1).CompareTo(GetPriority(element2));
            });

            CommandDispatcher.BeginGroup();
            changes.elementsToRemove.RemoveAll(element =>
            {
                switch (element)
                {
                    case BaseConnectionView edgeView:
                        if (edgeView.selected)
                            CommandDispatcher.Do(new DisconnectCommand(ViewModel, edgeView.ViewModel));
                        return true;
                    case BaseNodeView nodeView:
                        if (nodeView.selected)
                            CommandDispatcher.Do(new RemoveNodeCommand(ViewModel, nodeView.ViewModel));
                        return true;
                    case BaseGroupView groupView:
                        if (groupView.selected)
                            CommandDispatcher.Do(new RemoveGroupCommand(ViewModel, groupView.ViewModel));
                        return true;
                }

                return false;
            });
            CommandDispatcher.EndGroup();

            UpdateInspector();
            return changes;
        }

        /// <summary> 转换发生改变时调用 </summary>
        void OnViewTransformChanged(GraphView view)
        {
            ViewModel.Zoom = viewTransform.scale.x;
            ViewModel.Pan = viewTransform.position.ToInternalVector3Int();
        }

        #endregion
    }
}
#endif