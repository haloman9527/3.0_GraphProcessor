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
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGraphView : GraphView, IBindableView<BaseGraph>
    {
        #region 属性
        public event Action onDirty;
        public event Action onUndirty;

        public CreateNodeMenuWindow CreateNodeMenu { get; private set; }
        public BaseGraphWindow GraphWindow { get; private set; }
        public CommandDispatcher CommandDispacter { get; private set; }
        public UnityObject GraphAsset { get { return GraphWindow.GraphAsset; } }
        public Dictionary<string, BaseNodeView> NodeViews { get; private set; } = new Dictionary<string, BaseNodeView>();
        public Dictionary<Group, BaseGroupView> GroupViews { get; private set; } = new Dictionary<Group, BaseGroupView>();

        public BaseGraph Model { get; set; }

        #region 不建议使用自带复制粘贴功能，建议自己实现
        protected sealed override bool canCopySelection => false;
        protected sealed override bool canCutSelection => false;
        protected sealed override bool canDuplicateSelection => false;
        protected sealed override bool canPaste => false;
        #endregion

        #endregion

        public BaseGraphView()
        {
            styleSheets.Add(GraphProcessorStyles.GraphViewStyle);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.05f, maxScale = 2f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();
        }

        #region Initialize
        public void SetUp(IGraph graph, BaseGraphWindow window, CommandDispatcher commandDispacter)
        {
            Model = graph as BaseGraph;
            GraphWindow = window;
            CommandDispacter = commandDispacter;
            EditorCoroutine coroutine = GraphWindow.StartCoroutine(Initialize());
            RegisterCallback<DetachFromPanelEvent>(evt => { GraphWindow.StopCoroutine(coroutine); });
        }

        IEnumerator Initialize()
        {
            // 初始化
            viewTransform.position = Model.Pan == default ? Vector3.zero : Model.Pan;
            viewTransform.scale = Model.Zoom == default ? Vector3.one : Model.Zoom;

            // 绑定
            CreateNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            CreateNodeMenu.Initialize(this, GetNodeTypes());

            graphViewChanged = GraphViewChangedCallback;
            viewTransformChanged = ViewTransformChangedCallback;
            nodeCreationRequest = c => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), CreateNodeMenu);

            yield return GraphWindow.StartCoroutine(GenerateNodeViews());
            yield return GraphWindow.StartCoroutine(LinkNodeViews());
            yield return GraphWindow.StartCoroutine(GenerateGroupViews());

            UpdateInspector();
            OnInitialized();
        }

        public void BindingProperties()
        {
            RegisterCallback<DetachFromPanelEvent>(evt => { UnBindingProperties(); });

            Model.BindingProperty<Vector3>(BaseGraph.PAN_NAME, OnPositionChanged);
            Model.BindingProperty<Vector3>(BaseGraph.ZOOM_NAME, OnScaleChanged);

            Model.OnNodeAdded += OnNodeAdded;
            Model.OnNodeRemoved += OnNodeRemoved;

            Model.OnGroupAdded += OnGroupAdded;
            Model.OnGroupRemoved += OnGroupRemoved;

            Model.OnConnected += OnConnected;
            Model.OnDisconnected += OnDisconnected;

            OnBindingProperties();
        }

        public void UnBindingProperties()
        {
            this.Query<GraphElement>().ForEach(element =>
            {
                if (element is IBindableView bindableView)
                {
                    bindableView.UnBindingProperties();
                }
            });

            Model.UnBindingProperty<Vector3>(BaseGraph.PAN_NAME, OnPositionChanged);
            Model.UnBindingProperty<Vector3>(BaseGraph.ZOOM_NAME, OnScaleChanged);

            Model.OnNodeAdded -= OnNodeAdded;
            Model.OnNodeRemoved -= OnNodeRemoved;

            Model.OnConnected -= OnConnected;
            Model.OnDisconnected -= OnDisconnected;

            OnUnbindingProperties();
        }

        /// <summary> 生成所有NodeView </summary>
        IEnumerator GenerateNodeViews()
        {
            int step = 0;
            foreach (var node in Model.Nodes.Values)
            {
                if (node == null) continue;
                AddNodeView(node);
                step++;
                if (step % 5 == 0)
                    yield return null;
            }
        }

        /// <summary> 连接节点 </summary>
        IEnumerator LinkNodeViews()
        {
            int step = 0;
            foreach (var connection in Model.Connections)
            {
                if (connection == null) continue;
                BaseNodeView fromNodeView, toNodeView;
                if (!NodeViews.TryGetValue(connection.FromNodeGUID, out fromNodeView)) throw new NullReferenceException($"找不到From节点{connection.FromNodeGUID}");
                if (!NodeViews.TryGetValue(connection.ToNodeGUID, out toNodeView)) throw new NullReferenceException($"找不到To节点{connection.ToNodeGUID}");
                ConnectView(fromNodeView, toNodeView, connection);
                step++;
                if (step % 5 == 0)
                    yield return null;
            }
        }

        /// <summary> 生成所有GroupView </summary>
        IEnumerator GenerateGroupViews()
        {
            int step = 0;
            foreach (var group in Model.Groups)
            {
                if (group == null) continue;
                AddGroupView(group);
                step++;
                if (step % 5 == 0)
                    yield return null;
            }
        }
        #endregion

        #region Callbacks
        void OnPositionChanged(Vector3 position)
        {
            viewTransform.position = position;
            SetDirty();
        }

        void OnScaleChanged(Vector3 scale)
        {
            viewTransform.scale = scale;
            SetDirty();
        }

        void OnNodeAdded(BaseNode node)
        {
            AddNodeView(node);
            SetDirty();
        }

        void OnNodeRemoved(BaseNode node)
        {
            RemoveNodeView(NodeViews[node.GUID]);
            SetDirty();
        }

        void OnGroupAdded(Group group)
        {
            AddGroupView(group);
            SetDirty();
        }

        void OnGroupRemoved(Group group)
        {
            RemoveGroupView(GroupViews[group]);
            SetDirty();
        }

        void OnConnected(BaseConnection connection)
        {
            var from = NodeViews[connection.FromNodeGUID];
            var to = NodeViews[connection.ToNodeGUID];
            ConnectView(from, to, connection);
            SetDirty();
        }

        void OnDisconnected(BaseConnection connection)
        {
            edges.ForEach(edge =>
            {
                if (edge.userData != connection) return;
                DisconnectView(edge as BaseConnectionView);
            });
            SetDirty();
        }

        /// <summary> GraphView发生改变时调用 </summary>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.movedElements != null)
            {
                CommandDispacter.BeginGroup();
                // 当节点移动之后，与之连接的接口重新排序
                Dictionary<BaseNode, Vector2> newPos = new Dictionary<BaseNode, Vector2>();
                Dictionary<Group, Vector2> groupNewPos = new Dictionary<Group, Vector2>();
                HashSet<BasePort> ports = new HashSet<BasePort>();

                changes.movedElements.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case BaseNodeView nodeView:
                            newPos[nodeView.Model] = nodeView.GetPosition().position;
                            // 记录需要重新排序的接口
                            foreach (var port in nodeView.Model.Ports.Values)
                            {
                                foreach (var connection in port.Connections)
                                {
                                    if (port.direction == BasePort.Direction.Input)
                                        ports.Add(connection.FromNode.Ports[connection.FromPortName]);
                                    else
                                        ports.Add(connection.ToNode.Ports[connection.ToPortName]);
                                }
                            }
                            return true;
                        case BaseGroupView groupView:
                            groupNewPos[groupView.Model] = groupView.GetPosition().position;
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
                        var node = Model.Nodes[nodeGUID];
                        var nodeView = NodeViews[nodeGUID];
                        newPos[node] = nodeView.GetPosition().position;
                    }
                }
                CommandDispacter.Do(new MoveNodesCommand(newPos));
                CommandDispacter.Do(new MoveGroupsCommand(groupNewPos));
                // 排序
                foreach (var port in ports)
                {
                    port.Resort();
                }
                CommandDispacter.EndGroup();
            }
            if (changes.elementsToRemove != null)
            {
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

                CommandDispacter.BeginGroup();
                changes.elementsToRemove.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case BaseConnectionView edgeView:
                            if (edgeView.selected)
                                CommandDispacter.Do(new DisconnectCommand(Model, edgeView.Model));
                            return true;
                        case BaseNodeView nodeView:
                            if (nodeView.selected)
                                CommandDispacter.Do(new RemoveNodeCommand(Model, nodeView.Model));
                            return true;
                        case BaseGroupView groupView:
                            if (groupView.selected)
                                CommandDispacter.Do(new RemoveGroupCommand(Model, groupView.Model));
                            return true;
                    }
                    return false;
                });

                CommandDispacter.EndGroup();

                UpdateInspector();
            }
            return changes;
        }

        /// <summary> 转换发生改变时调用 </summary>
        void ViewTransformChangedCallback(GraphView view)
        {
            Model.Pan = viewTransform.position;
            Model.Zoom = viewTransform.scale;
        }
        #endregion

        #region 方法
        public BaseNodeView AddNodeView(BaseNode node)
        {
            BaseNodeView nodeView = NewNodeView(node);
            nodeView.SetUp(node, this);
            nodeView.BindingProperties();
            NodeViews[node.GUID] = nodeView;
            AddElement(nodeView);
            return nodeView;
        }

        public void RemoveNodeView(BaseNodeView nodeView)
        {
            nodeView.UnBindingProperties();
            RemoveElement(nodeView);
            NodeViews.Remove(nodeView.Model.GUID);
        }

        public BaseGroupView AddGroupView(Group group)
        {
            BaseGroupView groupView = new BaseGroupView();
            groupView.SetUp(group, this);
            groupView.BindingProperties();
            GroupViews[group] = groupView;
            AddElement(groupView);
            return groupView;
        }

        public void RemoveGroupView(BaseGroupView groupView)
        {
            groupView.UnBindingProperties();
            groupView.RemoveFromHierarchy();
            GroupViews.Remove(groupView.Model);
        }

        public BaseConnectionView ConnectView(BaseNodeView from, BaseNodeView to, BaseConnection connection)
        {
            var connectionoView = NewConnectionView(connection);
            connectionoView.SetUp(connection, this);
            connectionoView.BindingProperties();
            connectionoView.userData = connection;
            connectionoView.output = from.portViews[connection.FromPortName];
            connectionoView.input = to.portViews[connection.ToPortName];
            from.portViews[connection.FromPortName].Connect(connectionoView);
            to.portViews[connection.ToPortName].Connect(connectionoView);
            AddElement(connectionoView);
            return connectionoView;
        }

        public void DisconnectView(BaseConnectionView connectionView)
        {
            BasePortView inputPortView = connectionView.input as BasePortView;
            BaseNodeView inputNodeView = inputPortView.node as BaseNodeView;
            if (inputPortView != null)
            {
                inputPortView.Disconnect(connectionView);
            }
            inputPortView.Disconnect(connectionView);

            BasePortView outputPortView = connectionView.output as BasePortView;
            BaseNodeView outputNodeView = outputPortView.node as BaseNodeView;
            if (outputPortView != null)
            {
                outputPortView.Disconnect(connectionView);
            }
            outputPortView.Disconnect(connectionView);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            connectionView.UnBindingProperties();
            RemoveElement(connectionView);
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

        public sealed override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
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

        public void SetUndirty()
        {
            onUndirty?.Invoke();
        }
        #endregion
    }
}
#endif