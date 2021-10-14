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
using CZToolKit.Core;
using CZToolKit.Core.Editors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class InternalBaseGraphView : GraphView, IBindableView<BaseGraph>
    {
        #region 属性
        public bool IsDirty { get; private set; } = false;
        public CreateNodeMenuWindow CreateNodeMenu { get; private set; }
        public BaseGraphWindow GraphWindow { get; private set; }
        public CommandDispatcher CommandDispacter { get; private set; }
        public UnityObject GraphAsset { get { return GraphWindow.GraphAsset; } }
        protected override bool canCopySelection { get { return true; } }
        protected override bool canCutSelection { get { return true; } }
        public Dictionary<string, InternalBaseNodeView> NodeViews { get; private set; } = new Dictionary<string, InternalBaseNodeView>();

        public BaseGraph Model { get; set; }
        #endregion

        private InternalBaseGraphView()
        {
            styleSheets.Add(GraphProcessorStyles.GraphViewStyle);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.05f, maxScale = 2f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();
        }

        public InternalBaseGraphView(BaseGraph graph, BaseGraphWindow window, CommandDispatcher commandDispacter) : this()
        {
            Model = graph;
            GraphWindow = window;
            CommandDispacter = commandDispacter;
            EditorCoroutine coroutine = GraphWindow.StartCoroutine(Init());
            RegisterCallback<DetachFromPanelEvent>(evt => { GraphWindow.StopCoroutine(coroutine); });
        }

        #region Initialize
        IEnumerator Init()
        {
            InitializeCallbacks();
            yield return null;

            InitializeToolbarButtons();
            yield return null;

            // 绑定
            BindingProperties();
            RegisterCallback<DetachFromPanelEvent>(evt => { UnBindingProperties(); });
            yield return null;

            yield return GraphWindow.StartCoroutine(GenerateNodeViews());
            yield return GraphWindow.StartCoroutine(LinkNodeViews());
            yield return GraphWindow.StartCoroutine(NotifyNodeViewsInitialized());

            OnInitialized();

            double time = EditorApplication.timeSinceStartup;
            Add(new IMGUIContainer(() =>
            {
                if (IsDirty && EditorApplication.timeSinceStartup > time && GraphAsset != null)
                {
                    IsDirty = false;
                    EditorUtility.SetDirty(GraphAsset);
                    time += 1;
                }
            }));
            MarkDirtyRepaint();
        }

        void InitializeCallbacks()
        {
            graphViewChanged = GraphViewChangedCallback;
            serializeGraphElements = SerializeGraphElementsCallback;
            canPasteSerializedData = CanPasteSerializedDataCallback;
            unserializeAndPaste = DeserializeAndPasteCallback;
            viewTransformChanged = ViewTransformChangedCallback;

            RegisterCallback<KeyDownEvent>(KeyDownCallback);

            CreateNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            CreateNodeMenu.Initialize(this, GetNodeTypes());
            nodeCreationRequest = c => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), CreateNodeMenu);
        }

        /// <summary> 初始化ToolbarButton </summary>
        void InitializeToolbarButtons()
        {
            ToolbarButton btnCenter = new ToolbarButton()
            {
                text = "Center"
            };
            btnCenter.clicked += () =>
            {
                ResetPositionAndZoom();
                UpdateViewTransform(Model.Position, Model.Scale);
            };
            GraphWindow.Toolbar.AddButtonToLeft(btnCenter);

            ToolbarButton btnSave = new ToolbarButton()
            {
                text = "Save",
                style = { width = 60 }
            };
            btnSave.clicked += () => Save();
            GraphWindow.Toolbar.AddButtonToRight(btnSave);
        }

        /// <summary> 生成所有NodeView </summary>
        IEnumerator GenerateNodeViews()
        {
            foreach (var node in Model.Nodes)
            {
                yield return null;
                if (node.Value == null) continue;
                AddNodeView(node.Value);
            }
        }

        /// <summary> 连接节点 </summary>
        IEnumerator LinkNodeViews()
        {
            foreach (var edge in Model.Connections)
            {
                yield return null;
                if (edge == null) continue;
                InternalBaseNodeView fromNodeView, toNodeView;
                if (!NodeViews.TryGetValue(edge.FromNodeGUID, out fromNodeView)) yield break;
                if (!NodeViews.TryGetValue(edge.ToNodeGUID, out toNodeView)) yield break;

                ConnectView(fromNodeView, toNodeView, edge);
            }
        }

        IEnumerator NotifyNodeViewsInitialized()
        {
            foreach (var nodeView in NodeViews.Values)
            {
                yield return null;
                nodeView.Initialized();
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
            InternalBaseNodeView nodeView = AddNodeView(node);
            nodeView.Initialized();
            SetDirty();
        }

        void OnNodeRemoved(BaseNode node)
        {
            RemoveNodeView(NodeViews[node.GUID]);
            SetDirty();
        }

        void OnEdgeAdded(BaseConnection connection)
        {
            var from = NodeViews[connection.FromNodeGUID];
            var to = NodeViews[connection.ToNodeGUID];
            ConnectView(from, to, connection);
            SetDirty();
        }

        void OnEdgeRemoved(BaseConnection connection)
        {
            edges.ForEach(edge =>
            {
                if (edge.userData != connection) return;
                DisconnectView(edge as InternalBaseConnectionView);
            });
            SetDirty();
        }

        protected virtual void BindingProperties()
        {
            // 初始化
            viewTransform.position = Model.Position;
            viewTransform.scale = Model.Scale;

            Model.BindingProperty<Vector3>(BaseGraph.POSITION_NAME, OnPositionChanged);
            Model.BindingProperty<Vector3>(BaseGraph.SCALE_NAME, OnScaleChanged);

            Model.onNodeAdded += OnNodeAdded;
            Model.onNodeRemoved += OnNodeRemoved;

            Model.onEdgeAdded += OnEdgeAdded;
            Model.onConnectionRemoved += OnEdgeRemoved;
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

            Model.UnBindingProperty<Vector3>(BaseGraph.POSITION_NAME, OnPositionChanged);
            Model.UnBindingProperty<Vector3>(BaseGraph.SCALE_NAME, OnScaleChanged);

            Model.onNodeAdded -= OnNodeAdded;
            Model.onNodeRemoved -= OnNodeRemoved;

            Model.onEdgeAdded -= OnEdgeAdded;
            Model.onConnectionRemoved -= OnEdgeRemoved;
        }
        #endregion

        #region 回调方法
        /// <summary> GraphView发生改变时调用 </summary>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.movedElements != null)
            {
                CommandDispacter.BeginGroup();
                changes.movedElements.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case InternalBaseNodeView nodeView:
                            CommandDispacter.Do(new MoveNodeCommand(nodeView.Model, nodeView.GetPosition().position));
                            return true;
                        default:
                            break;
                    }
                    return false;
                });
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
                            case InternalBaseNodeView nodeView:
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
                        case InternalBaseConnectionView edgeView:
                            if (edgeView.selected)
                                CommandDispacter.Do(new DisconnectCommand(Model, edgeView.Model));
                            return true;
                        case InternalBaseNodeView nodeView:
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

        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new ClipBoard();

            foreach (var element in elements)
            {
                switch (element)
                {
                    case InternalBaseNodeView nodeView:
                        data.copiedNodes.Add(nodeView.Model);
                        break;
                    case InternalBaseConnectionView edgeView:
                        data.copiedEdges.Add(edgeView.Model);
                        break;
                    default:
                        break;
                }
            }
            return JsonSerializer.SerializeValue(data, out ClipBoard.objectReferences);
        }

        bool CanPasteSerializedDataCallback(string serializedData)
        {
            return !string.IsNullOrEmpty(serializedData);
        }

        void DeserializeAndPasteCallback(string operationName, string serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
                return;
            var data = JsonSerializer.DeserializeValue<ClipBoard>(serializedData, ClipBoard.objectReferences);
            if (data == null)
                return;
            ClearSelection();
            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();

            CommandDispacter.BeginGroup();
            foreach (var node in data.copiedNodes)
            {
                if (node == null)
                    continue;
                string sourceGUID = node.GUID;
                // 新节点重置id
                BaseNode.IDAllocation(node, Model);
                // 新节点与旧id存入字典
                copiedNodesMap[sourceGUID] = node;
                node.Position += new Vector2(20, 20);
                CommandDispacter.Do(new AddNodeCommand(Model, node));
                //Model.AddNode(node);
                AddToSelection(NodeViews[node.GUID]);
            }
            foreach (var edge in data.copiedEdges)
            {
                copiedNodesMap.TryGetValue(edge.FromNodeGUID, out var fromNode);
                copiedNodesMap.TryGetValue(edge.ToNodeGUID, out var toNode);

                fromNode = fromNode == null ? Model.Nodes[edge.FromNodeGUID] : Model.Nodes[fromNode.GUID];
                toNode = toNode == null ? Model.Nodes[edge.ToNodeGUID] : Model.Nodes[toNode.GUID];

                if (fromNode == null || toNode == null) continue;

                if (NodeViews.TryGetValue(fromNode.GUID, out InternalBaseNodeView inputNodeView)
                    && NodeViews.TryGetValue(toNode.GUID, out InternalBaseNodeView outputNodeView))
                {
                    CommandDispacter.Do(new ConnectCommand(Model, inputNodeView.Model, edge.FromSlotName, outputNodeView.Model, edge.ToSlotName));
                    //Model.Connect(inputNodeView.Model, outputNodeView.Model);
                }
            }

            CommandDispacter.EndGroup();

            SetDirty();
        }

        /// <summary> 转换发生改变时调用 </summary>
        void ViewTransformChangedCallback(GraphView view)
        {
            Model.Position = viewTransform.position;
            Model.Scale = viewTransform.scale;
        }

        void KeyDownCallback(KeyDownEvent evt)
        {
            if (evt.commandKey || evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.S:
                        Save();
                        evt.StopPropagation();
                        break;
                    case KeyCode.Z:
                        CommandDispacter.Undo();
                        evt.StopPropagation();
                        break;
                    case KeyCode.Y:
                        CommandDispacter.Redo();
                        evt.StopPropagation();
                        break;
                    default:
                        break;
                }
            }
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            UpdateInspector();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            UpdateInspector();
        }
        #endregion

        #region 方法
        public InternalBaseNodeView AddNodeView(BaseNode node)
        {
            Type nodeViewType = GetNodeViewType(node);
            InternalBaseNodeView nodeView = Activator.CreateInstance(nodeViewType) as InternalBaseNodeView;

            nodeView.SetUp(node, this);
            NodeViews[node.GUID] = nodeView;
            AddElement(nodeView);
            return nodeView;
        }

        public void RemoveNodeView(InternalBaseNodeView nodeView)
        {
            RemoveElement(nodeView);
            NodeViews.Remove(nodeView.Model.GUID);
        }

        public InternalBaseConnectionView ConnectView(InternalBaseNodeView from, InternalBaseNodeView to, BaseConnection connection)
        {
            var edgeView = Activator.CreateInstance(GetConnectionViewType(connection)) as InternalBaseConnectionView;
            edgeView.userData = connection;
            edgeView.output = from.portViews[connection.FromSlotName];
            edgeView.input = to.portViews[connection.ToSlotName];
            from.portViews[connection.FromSlotName].Connect(edgeView);
            to.portViews[connection.ToSlotName].Connect(edgeView);
            edgeView.SetUp(connection, this);
            AddElement(edgeView);
            return edgeView;
        }

        public void DisconnectView(InternalBaseConnectionView edgeView)
        {
            RemoveElement(edgeView);
            Port inputPortView = edgeView.input;
            InternalBaseNodeView inputNodeView = inputPortView.node as InternalBaseNodeView;
            if (inputPortView != null)
            {
                inputPortView.Disconnect(edgeView);
            }

            Port outputPortView = edgeView.output;
            InternalBaseNodeView outputNodeView = outputPortView.node as InternalBaseNodeView;
            if (outputPortView != null)
            {
                edgeView.output.Disconnect(edgeView);
            }

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();
        }

        public virtual void ResetPositionAndZoom()
        {
            Model.Position = Vector3.zero;
            Model.Scale = Vector3.one;
        }

        // 标记Dirty
        public void SetDirty(bool immediately = false)
        {
            if (!GraphWindow.titleContent.text.EndsWith(" *"))
                GraphWindow.titleContent.text += " *";
            if (immediately & GraphAsset != null)
                EditorUtility.SetDirty(GraphAsset);
            else
                IsDirty = true;
        }

        // 保存
        public void Save()
        {
            SetDirty(true);
            (GraphAsset as IGraphAsset)?.SaveGraph();
            if (GraphWindow.GraphOwner != null)
            {
                GraphWindow.GraphOwner.SaveVariables();
                EditorUtility.SetDirty(GraphWindow.GraphOwner as UnityObject);
            }
            AssetDatabase.SaveAssets();
            if (GraphWindow.titleContent.text.EndsWith(" *"))
                GraphWindow.titleContent.text = GraphWindow.titleContent.text.Replace(" *", "");
        }
        #endregion

        #region 抽象方法
        protected abstract void OnInitialized();

        protected abstract IEnumerable<Type> GetNodeTypes();

        protected abstract Type GetNodeViewType(BaseNode node);

        protected abstract Type GetConnectionViewType(BaseConnection connection);

        /// <summary> 获取兼容接口 </summary>
        public override List<Port> GetCompatiblePorts(Port startPortView, NodeAdapter nodeAdapter)
        {
            throw new NotImplementedException("你必须实现该方法才能正常使用");
        }

        protected abstract void UpdateInspector();
        #endregion
    }
}
