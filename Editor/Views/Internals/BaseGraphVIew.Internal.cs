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
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGraphView : GraphView, IBindableView<BaseGraph>
    {
        #region 属性
        public bool IsDirty { get; private set; } = false;
        public CreateNodeMenuWindow CreateNodeMenu { get; private set; }
        public BaseGraphWindow GraphWindow { get; private set; }
        public CommandDispatcher CommandDispacter { get; private set; }
        public UnityObject GraphAsset { get { return GraphWindow.GraphAsset; } }
        public Dictionary<string, BaseNodeView> NodeViews { get; private set; } = new Dictionary<string, BaseNodeView>();

        public BaseGraph Model { get; set; }

        #region 不建议使用自带复制粘贴功能，建议自己实现
        protected sealed override bool canCopySelection => false;
        protected sealed override bool canCutSelection => false;
        protected sealed override bool canDuplicateSelection => false;
        protected sealed override bool canPaste => false;
        #endregion

        #endregion

        private BaseGraphView()
        {
            styleSheets.Add(GraphProcessorStyles.GraphViewStyle);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.05f, maxScale = 2f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();
        }

        public BaseGraphView(BaseGraph graph, BaseGraphWindow window, CommandDispatcher commandDispacter) : this()
        {
            Model = graph;
            GraphWindow = window;
            CommandDispacter = commandDispacter;
            EditorCoroutine coroutine = GraphWindow.StartCoroutine(Initialize());
            RegisterCallback<DetachFromPanelEvent>(evt => { GraphWindow.StopCoroutine(coroutine); });
        }

        #region Initialize
        IEnumerator Initialize()
        {
            // 初始化
            viewTransform.position = Model.Pan == default ? Vector3.zero : Model.Pan;
            viewTransform.scale = Model.Zoom == default ? Vector3.one : Model.Zoom;

            // 绑定
            BindingProperties();
            RegisterCallback<DetachFromPanelEvent>(evt => { UnBindingProperties(); });

            InitializeCallbacks();

            yield return GraphWindow.StartCoroutine(GenerateNodeViews());
            yield return GraphWindow.StartCoroutine(LinkNodeViews());

            double nextCheckTime = EditorApplication.timeSinceStartup;
            Add(new IMGUIContainer(() =>
            {
                if (IsDirty && EditorApplication.timeSinceStartup > nextCheckTime)
                {
                    IsDirty = false;
                    SetDirty(true);
                    nextCheckTime += 1;
                }
            }));
            UpdateInspector();
            OnInitialized();
        }

        void InitializeCallbacks()
        {
            graphViewChanged = GraphViewChangedCallback;
            viewTransformChanged = ViewTransformChangedCallback;

            CreateNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            CreateNodeMenu.Initialize(this, GetNodeTypes());
            nodeCreationRequest = c => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), CreateNodeMenu);
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
            foreach (var edge in Model.Connections)
            {
                if (edge == null) continue;
                BaseNodeView fromNodeView, toNodeView;
                if (!NodeViews.TryGetValue(edge.FromNodeGUID, out fromNodeView)) yield break;
                if (!NodeViews.TryGetValue(edge.ToNodeGUID, out toNodeView)) yield break;

                ConnectView(fromNodeView, toNodeView, edge);
                step++;
                if (step % 5 == 0)
                    yield return null;
            }
        }
        #endregion

        #region 数据监听回调
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

        protected virtual void BindingProperties()
        {
            Model.BindingProperty<Vector3>(BaseGraph.PAN_NAME, OnPositionChanged);
            Model.BindingProperty<Vector3>(BaseGraph.ZOOM_NAME, OnScaleChanged);

            Model.onNodeAdded += OnNodeAdded;
            Model.onNodeRemoved += OnNodeRemoved;

            Model.onConnected += OnConnected;
            Model.onDisconnected += OnDisconnected;
        }

        public virtual void UnBindingProperties()
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

            Model.onNodeAdded -= OnNodeAdded;
            Model.onNodeRemoved -= OnNodeRemoved;

            Model.onConnected -= OnConnected;
            Model.onDisconnected -= OnDisconnected;
        }
        #endregion

        #region 回调方法
        /// <summary> GraphView发生改变时调用 </summary>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.movedElements != null)
            {
                CommandDispacter.BeginGroup();
                // 当节点移动之后，与之连接的接口重新排序
                HashSet<BasePort> ports = new HashSet<BasePort>();
                changes.movedElements.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case BaseNodeView nodeView:
                            CommandDispacter.Do(new MoveNodeCommand(nodeView.Model, nodeView.GetPosition().position));
                            // 记录需要重新排序的接口
                            foreach (var port in nodeView.Model.Ports.Values)
                            {
                                foreach (var connection in port.Connections)
                                {
                                    if (port.direction == BasePort.Direction.Input)
                                    {
                                        ports.Add(connection.FromNode.Ports[connection.FromPortName]);
                                    }
                                    else
                                    {
                                        ports.Add(connection.ToNode.Ports[connection.ToPortName]);
                                    }
                                }
                            }
                            return true;
                        default:
                            break;
                    }
                    return false;
                });
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
                            case BaseNodeView nodeView:
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

        //string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        //{
        //    var data = new ClipBoard();

        //    foreach (var element in elements)
        //    {
        //        switch (element)
        //        {
        //            case BaseNodeView nodeView:
        //                data.copiedNodes.Add(nodeView.Model);
        //                break;
        //            case BaseConnectionView edgeView:
        //                data.copiedEdges.Add(edgeView.Model);
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    return JsonSerializer.SerializeValue(data, out ClipBoard.objectReferences);
        //}

        //bool CanPasteSerializedDataCallback(string serializedData)
        //{
        //    return !string.IsNullOrEmpty(serializedData);
        //}

        //void DeserializeAndPasteCallback(string operationName, string serializedData)
        //{
        //    if (string.IsNullOrEmpty(serializedData))
        //        return;
        //    var data = JsonSerializer.DeserializeValue<ClipBoard>(serializedData, ClipBoard.objectReferences);
        //    if (data == null)
        //        return;

        //    CommandDispacter.BeginGroup();
        //    ClearSelection();
        //    Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();
        //    foreach (var node in data.copiedNodes)
        //    {
        //        if (node == null)
        //            continue;
        //        string sourceGUID = node.GUID;
        //        // 新节点重置id
        //        BaseNode.IDAllocation(node, Model);
        //        // 新节点与旧id存入字典
        //        copiedNodesMap[sourceGUID] = node;
        //        node.Position += new Vector2(20, 20);
        //        CommandDispacter.Do(new AddNodeCommand(Model, node));
        //        AddToSelection(NodeViews[node.GUID]);
        //    }
        //    foreach (var edge in data.copiedEdges)
        //    {
        //        copiedNodesMap.TryGetValue(edge.FromNodeGUID, out var fromNode);
        //        copiedNodesMap.TryGetValue(edge.ToNodeGUID, out var toNode);

        //        fromNode = fromNode == null ? Model.Nodes[edge.FromNodeGUID] : Model.Nodes[fromNode.GUID];
        //        toNode = toNode == null ? Model.Nodes[edge.ToNodeGUID] : Model.Nodes[toNode.GUID];

        //        if (fromNode == null || toNode == null) continue;

        //        if (NodeViews.TryGetValue(fromNode.GUID, out BaseNodeView inputNodeView)
        //            && NodeViews.TryGetValue(toNode.GUID, out BaseNodeView outputNodeView))
        //        {
        //            CommandDispacter.Do(new ConnectCommand(Model, inputNodeView.Model, edge.FromPortName, outputNodeView.Model, edge.ToPortName));
        //        }
        //    }

        //    SetDirty();
        //    CommandDispacter.EndGroup();
        //}
        #endregion

        #region 方法
        public BaseNodeView AddNodeView(BaseNode node)
        {
            Type nodeViewType = GetNodeViewType(node);
            BaseNodeView nodeView = Activator.CreateInstance(nodeViewType) as BaseNodeView;

            nodeView.SetUp(node, this);
            NodeViews[node.GUID] = nodeView;
            AddElement(nodeView);
            return nodeView;
        }

        public void RemoveNodeView(BaseNodeView nodeView)
        {
            RemoveElement(nodeView);
            NodeViews.Remove(nodeView.Model.GUID);
        }

        public BaseConnectionView ConnectView(BaseNodeView from, BaseNodeView to, BaseConnection connection)
        {
            var edgeView = Activator.CreateInstance(GetConnectionViewType(connection)) as BaseConnectionView;
            edgeView.SetUp(connection, this);
            edgeView.userData = connection;
            edgeView.output = from.portViews[connection.FromPortName];
            edgeView.input = to.portViews[connection.ToPortName];
            from.portViews[connection.FromPortName].Connect(edgeView);
            to.portViews[connection.ToPortName].Connect(edgeView);
            AddElement(edgeView);
            return edgeView;
        }

        public void DisconnectView(BaseConnectionView edgeView)
        {
            BasePortView inputPortView = edgeView.input as BasePortView;
            BaseNodeView inputNodeView = inputPortView.node as BaseNodeView;
            if (inputPortView != null)
            {
                inputPortView.Disconnect(edgeView);
            }
            inputPortView.Disconnect(edgeView);

            BasePortView outputPortView = edgeView.output as BasePortView;
            BaseNodeView outputNodeView = outputPortView.node as BaseNodeView;
            if (outputPortView != null)
            {
                outputPortView.Disconnect(edgeView);
            }
            outputPortView.Disconnect(edgeView);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            RemoveElement(edgeView);
        }

        public virtual void ResetPositionAndZoom()
        {
            Model.Pan = Vector3.zero;
            Model.Zoom = Vector3.one;
        }

        // 标记Dirty
        public void SetDirty(bool immediately = false)
        {
            if (!GraphWindow.titleContent.text.EndsWith(" *"))
                GraphWindow.titleContent.text += " *";

            if (immediately)
            {
                if (GraphAsset != null)
                {
                    EditorUtility.SetDirty(GraphAsset);
                }
                
                if (GraphWindow.GraphOwner is UnityObject uobj && uobj != null)
                {
                    EditorUtility.SetDirty(uobj);
                }
            }
            else
                IsDirty = true;
        }

        public void UnsetDirty()
        {
            if (GraphWindow.titleContent.text.EndsWith(" *"))
                GraphWindow.titleContent.text = GraphWindow.titleContent.text.Replace(" *", "");
            IsDirty = false;
        }
        #endregion
    }
}
#endif