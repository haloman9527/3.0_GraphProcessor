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
using OdinSerializer;
using System;
using System.Collections;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using GraphElement = UnityEditor.Experimental.GraphView.GraphElement;
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class BaseGraphView : GraphView, IBindableView<BaseGraph>
    {
        #region 属性
        public bool IsDirty { get; private set; } = false;
        public CommandDispatcher CommandDispatcher { get; }
        public BlackboardView Blackboard { get; private set; }
        public CreateNodeMenuWindow CreateNodeMenu { get; private set; }
        public BaseGraphWindow GraphWindow { get; protected set; }
        public UnityObject GraphAsset { get { return GraphWindow.GraphAsset; } }
        protected override bool canCopySelection { get { return true; } }
        protected override bool canCutSelection { get { return true; } }
        public Dictionary<string, BaseNodeView> NodeViews { get; private set; } = new Dictionary<string, BaseNodeView>();
        public Dictionary<string, StackView> StackViews { get; private set; } = new Dictionary<string, StackView>();
        public Dictionary<GroupPanel, GroupView> GroupViews { get; private set; } = new Dictionary<GroupPanel, GroupView>();

        public BaseGraph Model { get; set; }
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

        public BaseGraphView(BaseGraph _graph, CommandDispatcher _commandDispatcher, BaseGraphWindow _window) : this()
        {
            Model = _graph;
            CommandDispatcher = _commandDispatcher;
            GraphWindow = _window;
            EditorCoroutine coroutine = GraphWindow.StartCoroutine(Init());
            RegisterCallback<DetachFromPanelEvent>(evt => { GraphWindow.StopCoroutine(coroutine); });
        }

        #region 数据监听回调
        void OnPositionChanged(Vector3 _position)
        {
            viewTransform.position = _position;
            SetDirty();
        }
        void OnScaleChanged(Vector3 _scale)
        {
            viewTransform.scale = _scale;
            SetDirty();
        }
        void OnNodeAdded(BaseNode _node)
        {
            BaseNodeView nodeView = AddNodeView(_node);
            //nodeView.RefreshExpandedState();
            nodeView.Initialized();
            SetDirty();
        }
        void OnNodeRemoved(BaseNode _node)
        {
            RemoveNodeView(NodeViews[_node.GUID]);
            SetDirty();
        }
        void OnEdgeAdded(BaseEdge _edge)
        {
            var input = NodeViews[_edge.InputNodeGUID].PortViews[_edge.InputFieldName];
            var output = NodeViews[_edge.OutputNodeGUID].PortViews[_edge.OutputFieldName];
            ConnectView(input, output, _edge);
            SetDirty();
        }
        void OnEdgeRemoved(BaseEdge _edge)
        {
            edges.ForEach(edge =>
            {
                if (edge.userData != _edge) return;
                DisconnectView(edge as BaseEdgeView);
            });
            SetDirty();
        }
        //void OnStackAdded(StackPanel _stack)
        //{
        //    AddStackNodeView(_stack);
        //}
        //void OnStackRemoved(StackPanel _stack)
        //{
        //    RemoveStackNodeView(StackViews[_stack.GUID]);
        //}
        void OnGroupAdded(GroupPanel _group)
        {
            AddGroupView(_group);
            SetDirty();
        }
        void OnGroupRemoved(GroupPanel _group)
        {
            RemoveGroupView(_group);
            SetDirty();
        }

        protected virtual void BindingProperties()
        {
            // 初始化
            viewTransform.position = Model.Position;
            viewTransform.scale = Model.Scale;

            Model.BindingProperty<Vector3>(nameof(Model.Position), OnPositionChanged);
            Model.BindingProperty<Vector3>(nameof(Model.Scale), OnScaleChanged);

            Model.onNodeAdded += OnNodeAdded;
            Model.onNodeRemoved += OnNodeRemoved;

            Model.onEdgeAdded += OnEdgeAdded;
            Model.onEdgeRemoved += OnEdgeRemoved;

            //Model.onStackAdded += OnStackAdded;
            //Model.onStackRemoved += OnStackRemoved;

            Model.onGroupAdded += OnGroupAdded;
            Model.onGroupRemoved += OnGroupRemoved;
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

            Model.UnBindingProperty<Vector3>(nameof(Model.Position), OnPositionChanged);
            Model.UnBindingProperty<Vector3>(nameof(Model.Scale), OnScaleChanged);

            Model.onNodeAdded -= OnNodeAdded;
            Model.onNodeRemoved -= OnNodeRemoved;

            Model.onEdgeAdded -= OnEdgeAdded;
            Model.onEdgeRemoved -= OnEdgeRemoved;

            //Model.onStackAdded -= OnStackAdded;
            //Model.onStackRemoved -= OnStackRemoved;

            Model.onGroupAdded -= OnGroupAdded;
            Model.onGroupRemoved -= OnGroupRemoved;
        }
        #endregion

        #region Initialize
        IEnumerator Init()
        {
            Blackboard = new BlackboardView(this);
            Add(Blackboard);

            InitializeCallbacks();
            InitializeToolbarButtons();

            // 绑定
            BindingProperties();
            RegisterCallback<DetachFromPanelEvent>(evt => { UnBindingProperties(); });

            yield return GlobalEditorCoroutineMachine.StartCoroutine(GenerateNodeViews());
            yield return GlobalEditorCoroutineMachine.StartCoroutine(GenerateGroupViews());
            yield return GlobalEditorCoroutineMachine.StartCoroutine(LinkNodeViews());
            yield return GlobalEditorCoroutineMachine.StartCoroutine(NotifyNodeViewsInitialized());

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
            btnSave.clicked += () => SaveGraphToDisk();
            GraphWindow.Toolbar.AddButtonToRight(btnSave);

            ToolbarToggle toggleBlackboard = new ToolbarToggle()
            {
                text = "Blackboard",
                value = Model.BlackboardVisible
            };
            toggleBlackboard.RegisterValueChangedCallback(e =>
            {
                Model.BlackboardVisible = e.newValue;
            });
            GraphWindow.Toolbar.AddToggleToLeft(toggleBlackboard, 100);
        }

        void InitializeCallbacks()
        {
            graphViewChanged = GraphViewChangedCallback;
            serializeGraphElements = SerializeGraphElementsCallback;
            canPasteSerializedData = CanPasteSerializedDataCallback;
            unserializeAndPaste = DeserializeAndPasteCallback;
            viewTransformChanged = ViewTransformChangedCallback;

            RegisterCallback<DragPerformEvent>(DragPerformedCallback);
            RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
            RegisterCallback<KeyDownEvent>(KeyDownCallback);
            RegisterCallback<MouseUpEvent>(MouseUpCallback);

            CreateNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            CreateNodeMenu.Initialize(this, GetNodeTypes());
            nodeCreationRequest = c => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), CreateNodeMenu);
        }

        /// <summary> 生成所有NodeView </summary>
        IEnumerator GenerateNodeViews()
        {
            foreach (var node in Model.Nodes)
            {
                yield return 0;
                if (node.Value == null) continue;
                AddNodeView(node.Value);
            }
        }

        ///// <summary> 生成所有StackView </summary>
        //void GenerateStackViews()
        //{
        //    foreach (var stack in Model.Stacks.Values)
        //        AddStackNodeView(stack);
        //}

        /// <summary> 生成所有GroupView </summary>
        IEnumerator GenerateGroupViews()
        {
            foreach (var group in Model.Groups)
            {
                yield return 0;
                AddGroupView(group);
            }
        }

        /// <summary> 连接节点 </summary>
        IEnumerator LinkNodeViews()
        {
            foreach (var serializedEdge in Model.Edges)
            {
                yield return 0;
                if (serializedEdge.Value == null) continue;
                BaseNodeView inputNodeView, outputNodeView;
                if (!NodeViews.TryGetValue(serializedEdge.Value.InputNodeGUID, out inputNodeView)) yield break;
                if (!NodeViews.TryGetValue(serializedEdge.Value.OutputNodeGUID, out outputNodeView)) yield break;
                ConnectView(inputNodeView.PortViews[serializedEdge.Value.InputFieldName]
                    , outputNodeView.PortViews[serializedEdge.Value.OutputFieldName]
                    , serializedEdge.Value);
            }
        }

        IEnumerator NotifyNodeViewsInitialized()
        {
            foreach (var nodeView in NodeViews.Values)
            {
                yield return 0;
                nodeView.Initialized();
            }
        }

        protected virtual void OnInitialized() { }
        #endregion

        #region 回调方法
        /// <summary> GraphView发生改变时调用 </summary>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {
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
                            case BlackboardField bbField:
                                return 2;
                            case StackNode stackNodeView:
                                return 3;
                            case GroupView groupView:
                                return 4;
                        }
                        return 5;
                    }
                    return GetPriority(element1).CompareTo(GetPriority(element2));
                });
                changes.elementsToRemove.RemoveAll(element =>
                {
                    switch (element)
                    {
                        case BaseEdgeView edgeView:
                            if (edgeView.selected)
                                Model.DisconnectEdge(edgeView.Model.GUID);
                            return true;
                        case BaseNodeView nodeView:
                            if (nodeView.selected)
                                Model.RemoveNode(nodeView.Model);
                            return true;
                        case BlackboardField blackboardField:
                            Model.RemoveData_BB(blackboardField.text);
                            RemoveFromSelection(blackboardField);
                            return true;
                        //case StackView stackNodeView:
                        //    Model.RemoveStackNode(Model.Stacks[stackNodeView.Model.GUID]);
                        //    return true;
                        case GroupView groupView:
                            Model.RemoveGroup(groupView.Model);
                            return true;
                    }
                    return false;
                });

                UpdateInspector();
            }
            SetDirty();
            return changes;
        }

        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new ClipBoard();

            foreach (var element in elements)
            {
                switch (element)
                {
                    case BaseNodeView nodeView:
                        data.copiedNodes.Add(nodeView.Model);
                        continue;
                    case BaseEdgeView edgeView:
                        data.copiedEdges.Add(edgeView.Model);
                        continue;
                    //case StackView stackView:
                    //    data.copiedStacks.Add(stackView.Model);
                    //    continue;
                    case GroupView groupView:
                        data.copiedGroups.Add(groupView.Model);
                        continue;
                    default:
                        continue;
                }
            }
            return Encoding.UTF8.GetString(SerializationUtility.SerializeValue(data, DataFormat.JSON, out ClipBoard.objectReferences));
        }

        bool CanPasteSerializedDataCallback(string _serializedData)
        {
            return !string.IsNullOrEmpty(_serializedData);
        }

        void DeserializeAndPasteCallback(string _operationName, string _serializedData)
        {
            ClearSelection();
            var data = SerializationUtility.DeserializeValue<ClipBoard>(Encoding.UTF8.GetBytes(_serializedData), DataFormat.JSON, ClipBoard.objectReferences);
            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();
            foreach (var node in data.copiedNodes)
            {
                if (node == null)
                    continue;
                string sourceGUID = node.GUID;
                // 新节点重置id
                BaseNode.IDAllocation(node);
                // 新节点与旧id存入字典
                copiedNodesMap[sourceGUID] = node;
                Model.AddNode(node).ClearConnectionsWithoutNotification();
                node.Position += new Vector2(20, 20);
                AddToSelection(NodeViews[node.GUID]);
            }

            foreach (var group in data.copiedGroups)
            {
                group.Position = new Rect(new Vector2(20, 20), group.Position.size);

                var oldGUIDList = group.InnerNodeGUIDs.ToList();
                group.InnerNodeGUIDs.Clear();

                foreach (var guid in oldGUIDList)
                {
                    if (copiedNodesMap.TryGetValue(guid, out var node))
                        group.InnerNodeGUIDs.Add(node.GUID);
                }
                Model.AddGroup(group);
            }

            foreach (var edge in data.copiedEdges)
            {
                //edge.Enable(Model);
                copiedNodesMap.TryGetValue(edge.InputNodeGUID, out var inputNode);
                copiedNodesMap.TryGetValue(edge.OutputNodeGUID, out var outputNode);

                inputNode = inputNode == null ? Model.Nodes[edge.InputNodeGUID] : Model.Nodes[inputNode.GUID];
                outputNode = outputNode == null ? Model.Nodes[edge.OutputNodeGUID] : Model.Nodes[outputNode.GUID];

                if (inputNode == null || outputNode == null) continue;

                inputNode.TryGetPort(edge.InputFieldName, out NodePort inputPort);
                outputNode.TryGetPort(edge.OutputFieldName, out NodePort outputPort);
                if (!inputPort.Multiple && inputPort.IsConnected) continue;
                if (!outputPort.Multiple && outputPort.IsConnected) continue;

                if (NodeViews.TryGetValue(inputNode.GUID, out BaseNodeView inputNodeView)
                    && NodeViews.TryGetValue(outputNode.GUID, out BaseNodeView outputNodeView))
                {
                    Model.Connect(inputNodeView.Model.Ports[edge.InputFieldName], outputNodeView.Model.Ports[edge.OutputFieldName]);
                }
            }

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
                        SaveGraphToDisk();
                        evt.StopPropagation();
                        break;
                    default:
                        break;
                }
            }
        }

        void MouseUpCallback(MouseUpEvent evt)
        {
            UpdateInspector();
        }

        /// <summary> 拖拽物体悬停到GraphView时触发 </summary>
        void DragUpdatedCallback(DragUpdatedEvent evt)
        {
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            if (dragData != null && dragData.OfType<BlackboardField>().Any())
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        /// <summary> 拖拽物体到GraphView松开时触发 </summary>
        /// <param name="evt"></param>
        void DragPerformedCallback(DragPerformEvent evt)
        {
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            if (dragData == null) return;

            var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            var blackboardFields = dragData.OfType<BlackboardField>();
            if (blackboardFields.Any())
            {
                foreach (var paramFieldView in blackboardFields)
                {
                    Model.AddParameterNode(paramFieldView.text, mousePos);
                    mousePos += Vector2.one * 30;
                }
                SetDirty();
            }
        }
        #endregion

        #region Overrides
        public override Blackboard GetBlackboard()
        {
            return Blackboard;
        }

        /// <summary> 构建右键菜单 </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            //evt.menu.AppendAction("New Stack", (e) =>
            //{
            //    ViewModel.AddStackNode(BaseStack.CreateStack(position));
            //}, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("New Group", (e) =>
            {
                Model.AddGroup("New Group", position);
            }, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(GraphAsset), DropdownMenuAction.AlwaysEnabled);

            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Save Asset", (e) =>
            {
                SaveGraphToDisk();
            }, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Help/Reset Blackboard Windows", e =>
            {
                Model.BlackboardPosition = new Rect(Vector2.zero, BaseGraph.DefaultBlackboardSize);
            });
        }

        /// <summary> 获取兼容接口 </summary>
        public override List<Port> GetCompatiblePorts(Port _startPortView, NodeAdapter _nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            NodePortView startPortView = _startPortView as NodePortView;
            ports.ForEach(_portView =>
            {
                if (_portView is NodePortView portView
                    && NodePort.IsCompatible(startPortView.Model, portView.Model))
                {
                    compatiblePorts.Add(_portView);
                }
            });
            return compatiblePorts;
        }
        #endregion

        #region Virtuals
        protected virtual Type GetDefaultNodeViewType(BaseNode _nodeViewModel)
        {
            return typeof(DefaultNodeView);
        }

        protected virtual Type GetEdgeViewType()
        {
            return typeof(BaseEdgeView);
        }

        protected virtual IEnumerable<Type> GetNodeTypes()
        {
            foreach (var type in Utility_Reflection.GetChildrenTypes<BaseNode>())
            {
                if (type.IsAbstract) continue;
                Debug.Log(type.Name);
                yield return type;
            }
        }

        public virtual void UpdateInspector()
        {
            foreach (var element in selection)
            {
                switch (element)
                {
                    case BaseNodeView nodeView:
                        EditorGUILayoutExtension.DrawFieldsInInspector(nodeView.title, nodeView.Model);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    case BaseEdgeView edgeView:
                        EditorGUILayoutExtension.DrawFieldsInInspector(edgeView.title, edgeView.Model);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    case GroupView groupView:
                        EditorGUILayoutExtension.DrawFieldsInInspector(groupView.title, groupView.Model);
                        Selection.activeObject = ObjectInspector.Instance;
                        return;
                    default:
                        break;
                }
            }

            Selection.activeObject = null;
        }
        #endregion

        #region API
        public BaseNodeView AddNodeView(BaseNode _node)
        {
            Type nodeViewType = null;
            if (_node is ParameterNode parameterNode)
                nodeViewType = typeof(ParameterNodeView);
            else
                nodeViewType = GraphProcessorEditorUtility.GetNodeViewType(_node.GetType());

            if (nodeViewType == null)
                nodeViewType = GetDefaultNodeViewType(_node);
            BaseNodeView nodeView = Activator.CreateInstance(nodeViewType) as BaseNodeView;

            nodeView.SetUp(_node, CommandDispatcher, this);
            NodeViews[_node.GUID] = nodeView;
            AddElement(nodeView);
            return nodeView;
        }

        public void RemoveNodeView(BaseNodeView _nodeView)
        {
            RemoveElement(_nodeView);
            NodeViews.Remove(_nodeView.Model.GUID);
        }

        public GroupView AddGroupView(GroupPanel _group)
        {
            var groupView = new GroupView();
            groupView.SetUp(_group, CommandDispatcher, this);
            GroupViews[_group] = groupView;
            AddElement(groupView);
            AddSelectionsToGroup(groupView);
            return groupView;
        }

        public void AddSelectionsToGroup(GroupView _groupView)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    if (_groupView.ContainsElement(selectedNode as GraphElement))
                        continue;

                    _groupView.AddElement(selectedNode as BaseNodeView);
                }
            }
        }

        public void RemoveGroupView(GroupPanel _group)
        {
            if (!GroupViews.TryGetValue(_group, out GroupView _groupView))
                return;

            RemoveElement(_groupView);
            GroupViews.Remove(_group);
        }

        //public StackView AddStackNodeView(StackPanel _stackNode)
        //{
        //    var stackView = new StackView();
        //    stackView.SetUp(_stackNode, CommandDispatcher, this);
        //    AddElement(stackView);
        //    StackViews[_stackNode.GUID] = stackView;
        //    return stackView;
        //}

        //public void RemoveStackNodeView(StackView _stackNodeView)
        //{
        //    RemoveElement(_stackNodeView);
        //    StackViews.Remove(_stackNodeView.Model.GUID);
        //}

        //public void RemoveStackNodeViews()
        //{
        //    foreach (var stackView in StackViews)
        //        RemoveElement(stackView.Value);
        //    StackViews.Clear();
        //}

        public BaseEdgeView ConnectView(NodePortView _inputPortView, NodePortView _outputPortView, BaseEdge _serializableEdge)
        {
            var edgeView = Activator.CreateInstance(GetEdgeViewType()) as BaseEdgeView;
            edgeView.userData = _serializableEdge;
            edgeView.input = _inputPortView;
            edgeView.output = _outputPortView;
            edgeView.SetUp(_serializableEdge, CommandDispatcher, this);
            _inputPortView.Connect(edgeView);
            _outputPortView.Connect(edgeView);
            AddElement(edgeView);
            return edgeView;
        }

        public void DisconnectView(BaseEdgeView _edgeView)
        {
            RemoveElement(_edgeView);
            NodePortView inputPortView = _edgeView.input as NodePortView;
            BaseNodeView inputNodeView = inputPortView.node as BaseNodeView;
            if (inputPortView != null)
            {
                inputPortView.Disconnect(_edgeView);
            }

            NodePortView outputPortView = _edgeView.output as NodePortView;
            BaseNodeView outputNodeView = outputPortView.node as BaseNodeView;
            if (outputPortView != null)
            {
                _edgeView.output.Disconnect(_edgeView);
            }

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();
        }

        public virtual void ResetPositionAndZoom()
        {
            Model.Position = Vector3.zero;
            Model.Scale = Vector3.one;
        }

        #region 帮助方法

        public void SaveGraphToDisk()
        {
            (GraphAsset as IGraphAsset)?.SaveGraph();
            if (GraphWindow.GraphOwner != null)
            {
                GraphWindow.GraphOwner.SaveVariables();
                EditorUtility.SetDirty(GraphWindow.GraphOwner as UnityObject);
            }
            SetDirty(true);
            AssetDatabase.SaveAssets();
            if (GraphWindow.titleContent.text.EndsWith(" *"))
                GraphWindow.titleContent.text = GraphWindow.titleContent.text.Replace(" *", "");
        }

        public void SetDirty(bool _immediately = false)
        {
            if (!GraphWindow.titleContent.text.EndsWith(" *"))
                GraphWindow.titleContent.text += " *";
            if (_immediately)
                EditorUtility.SetDirty(GraphAsset);
            else
                IsDirty = true;
        }
        #endregion
        #endregion
    }

    public class BaseGraphView<M> : BaseGraphView where M : BaseGraph
    {
        public BaseGraphView(BaseGraph _graph, CommandDispatcher _commandDispatcher, BaseGraphWindow _window) : base(_graph, _commandDispatcher, _window) { }

        public M T_Model { get { return Model as M; } }
    }

    public sealed class DefaultGraphView : BaseGraphView<BaseGraph>
    {
        public DefaultGraphView(BaseGraph _graph, CommandDispatcher _commandDispatcher, BaseGraphWindow _window) : base(_graph, _commandDispatcher, _window) { }
    }
}