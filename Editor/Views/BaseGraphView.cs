using CZToolKit.Core;
using CZToolKit.Core.Editors;
using OdinSerializer;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;
using UnityObject = UnityEngine.Object;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseGraphView : GraphView, IGraphView
    {
        #region 事件
        public event Action onInitializeCompleted;
        #endregion

        #region 属性
        public bool Initialized { get; }
        public bool IsDirty { get; private set; } = false;
        public BaseEdgeConnectorListener ConnectorListener { get; }
        public CommandDispatcher CommandDispatcher { get; }
        private ExposedParameterView Blackboard { get; set; }
        public CreateNodeMenuWindow CreateNodeMenu { get; private set; }
        public BaseGraphWindow GraphWindow { get; private set; }
        public GraphViewParentElement Parent { get { return GraphWindow.GraphViewParent; } }
        public UnityObject GraphAsset { get; private set; }
        public IGraph Graph { get; private set; }
        public SerializedObject SerializedObject { get; private set; }
        public Dictionary<string, BaseNodeView> NodeViews { get; private set; } = new Dictionary<string, BaseNodeView>();
        public List<EdgeView> EdgeViews { get; private set; } = new List<EdgeView>();
        public List<GroupView> GroupViews { get; private set; } = new List<GroupView>();
        public Dictionary<string, BaseStackNodeView> StackNodeViews { get; private set; } = new Dictionary<string, BaseStackNodeView>();
        protected override bool canCopySelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView || e is BaseStackNodeView); }
        }
        protected override bool canCutSelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView || e is BaseStackNodeView); }
        }
        #endregion

        public BaseGraphView()
        {
            styleSheets.Add(GraphProcessorStyles.GraphViewStyle);

            Insert(0, new GridBackground());
            SetupZoom(0.05f, 2f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            InitializeCallbacks();

            this.StretchToParentSize();
        }

        public BaseGraphView(IGraph _graph, CommandDispatcher _commandDispatcher, BaseGraphWindow _window) : this()
        {
            CommandDispatcher = _commandDispatcher;
            GraphWindow = _window;
            Graph = _graph;
            GraphAsset = (_graph as IGraphFromAsset)?.Asset;
            SerializedObject = new SerializedObject(GraphAsset);

            ToolbarButton btnCenter = new ToolbarButton()
            {
                text = "Center",
                style = { alignSelf = Align.Center, width = 70, unityTextAlign = TextAnchor.MiddleCenter, color = Color.black }
            };
            btnCenter.clicked += () =>
            {
                ResetPositionAndZoom();
                UpdateViewTransform(Graph.Position, Graph.Scale);
            };
            Parent.Toolbar.AddToLeft(btnCenter);

            ToolbarToggle toggleBlackboard = new ToolbarToggle()
            {
                text = "Blackboard",
                value = Graph.BlackboardVisible,
                style = { alignSelf = Align.Center, width = 100, unityTextAlign = TextAnchor.MiddleCenter, color = Color.black }
            };
            toggleBlackboard.RegisterValueChangedCallback(e =>
            {
                GetBlackboard().style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                Graph.BlackboardVisible = e.newValue;
            });
            Parent.Toolbar.AddToggleToLeft(toggleBlackboard);

            ConnectorListener = CreateEdgeConnectorListener();

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

            InitializeGraphView();
            InitializeBlackboard();
            InitializeNodeViews();
            InitializeStacks();
            InitializeGroups();
            InitializeEdgeViews();

            onInitializeCompleted?.Invoke();
            OnInitialized();
            Initialized = true;
        }

        #region Initialize


        void GenerateNodeViews()
        {
            foreach (var node in Graph.NodesGUIDMapping)
            {
                if (node.Value == null) continue;
                AddNodeView(node.Value);
            }
        }

        void GenerateStackViews()
        {
            foreach (var stack in Graph.StackNodesGUIDMapping.Values)
                AddStackNodeView(stack);
        }

        void GenerateGroupViews()
        {
            foreach (var group in Graph.Groups)
                AddGroupView(group);
        }

        void LinkingNodeViews()
        {
            // 只连接，不会触发节点的OnPortConnected和OnPortDisconnected方法
        }

        protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener()
        {
            return new BaseEdgeConnectorListener(this);
        }

        protected virtual void OnInitialized() { }

        void InitializeCallbacks()
        {
            graphViewChanged = GraphViewChangedCallback;
            groupTitleChanged = GroupTitleChangedCallback;
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

        void InitializeGraphView()
        {
            viewTransform.position = Graph.Position;
            viewTransform.scale = Graph.Scale;
        }

        /// <summary> 初始化所有节点视图 </summary>
        void InitializeNodeViews()
        {
            foreach (var node in Graph.NodesGUIDMapping)
            {
                if (node.Value == null) continue;
                AddNodeView(node.Value);
            }
        }

        void InitializeStacks()
        {
            foreach (var stack in Graph.StackNodesGUIDMapping.Values)
                AddStackNodeView(stack);
        }

        /// <summary> 初始化所有Group的视图 </summary>
        void InitializeGroups()
        {
            foreach (var group in Graph.Groups)
                AddGroupView(group);
        }

        /// <summary> 初始化所有连接的视图 </summary>
        void InitializeEdgeViews()
        {
            foreach (var serializedEdge in Graph.EdgesGUIDMapping)
            {
                if (serializedEdge.Value == null) continue;
                BaseNodeView inputNodeView = null, outputNodeView = null;
                NodeViews.TryGetValue(serializedEdge.Value.InputNodeGUID, out inputNodeView);
                NodeViews.TryGetValue(serializedEdge.Value.OutputNodeGUID, out outputNodeView);
                if (inputNodeView == null || outputNodeView == null)
                    continue;
                ConnectView(inputNodeView.PortViews[serializedEdge.Value.InputFieldName], outputNodeView.PortViews[serializedEdge.Value.OutputFieldName], serializedEdge.Value);
            }
        }

        void InitializeBlackboard()
        {
            Blackboard = new ExposedParameterView(this);
            Blackboard.SetPosition(Graph.BlackboardPosition);
            Blackboard.style.display = Graph.BlackboardVisible ? DisplayStyle.Flex : DisplayStyle.None;
            Add(Blackboard);
        }

        #endregion

        #region Callbacks

        #region 系统回调
        public override Blackboard GetBlackboard()
        {
            return Blackboard;
        }

        /// <summary> GraphView发生改变时调用 </summary>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.elementsToRemove != null)
            {
                RegisterCompleteObjectUndo("Remove Graph Elements");

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
                        case EdgeView edgeView:
                            Disconnect(edgeView);
                            return true;
                        case BaseNodeView nodeView:
                            if (nodeView.selected)
                                RemoveNode(nodeView);
                            return true;
                        case BlackboardField blackboardField:
                            bool canDelete = true;
                            foreach (var parameterNode in Graph.NodesGUIDMapping.Values.OfType<ParameterNode>())
                            {
                                if (parameterNode.name == blackboardField.text)
                                {
                                    Debug.LogWarning("此参数正被节点引用");
                                    canDelete = false;
                                    break;
                                }
                            }
                            if (canDelete && Graph.Blackboard.RemoveData(blackboardField.text))
                                Blackboard.RemoveField(blackboardField);
                            return true;
                        case BaseStackNodeView stackNodeView:
                            RemoveStackNode(stackNodeView);
                            return true;
                        case GroupView groupView:
                            RemoveGroup(groupView);
                            return true;
                    }
                    return false;
                });

                UpdateNodeInspectorSelection();
            }
            SetDirty();
            return changes;
        }

        /// <summary> 修改Group标题后调用 </summary>
        /// <param name="_group"></param>
        /// <param name="_newName"></param>
        private void GroupTitleChangedCallback(Group _group, string _newName)
        {
            GroupView groupView = _group as GroupView;
            groupView.GroupData.title = _newName;
            SetDirty();
        }

        /// <summary> 构建右键菜单 </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("New Stack", (e) =>
            {
                BaseStack stackNode = BaseStack.CreateStack(position);
                stackNode.OnCreated();
                AddStackNode(stackNode);
            }, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("New Group", (e) =>
            {
                BaseGroup group = new BaseGroup("New Group", position);
                group.OnCreated();
                AddSelectionsToGroup(AddGroup(group));
            }, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(GraphAsset), DropdownMenuAction.AlwaysEnabled);

            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Save Asset", (e) =>
            {
                SaveGraphToDisk();
            }, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Help/Reset Blackboard Windows", e =>
            {
                Blackboard.SetPosition(new Rect(Vector2.zero, BaseGraph.DefaultBlackboardSize));
            });
        }

        /// <summary> 获取兼容接口 </summary>
        public override List<Port> GetCompatiblePorts(Port _startPortView, NodeAdapter _nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            PortView startPortView = _startPortView as PortView;
            ports.ForEach(_portView =>
            {
                PortView portView = _portView as PortView;

                if (portView.node == startPortView.node)
                    return;

                if (portView == null)
                    return;

                if (_portView.direction == _startPortView.direction)
                    return;

                if (_portView.connections.Any(edge => edge.input == _startPortView || edge.output == _startPortView))
                    return;

                if ((startPortView.TypeConstraint == PortTypeConstraint.None || portView.TypeConstraint == PortTypeConstraint.None)
                || (startPortView.TypeConstraint == PortTypeConstraint.Inherited && startPortView.DisplayType.IsAssignableFrom((Type)portView.DisplayType))
                || (startPortView.TypeConstraint == PortTypeConstraint.Strict && startPortView.DisplayType == portView.DisplayType))
                {
                    compatiblePorts.Add(_portView);
                    return;
                }
            });
            return compatiblePorts;
        }

        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new ClipBoard();

            foreach (var element in elements)
            {
                switch (element)
                {
                    case BaseNodeView nodeView:
                        data.copiedNodes.Add(nodeView.NodeData);
                        continue;
                    case EdgeView edgeView:
                        data.copiedEdges.Add(edgeView.EdgeData);
                        continue;
                    case BaseStackNodeView stackView:
                        data.copiedStacks.Add(stackView.stackNode);
                        continue;
                    case GroupView groupView:
                        data.copiedGroups.Add(groupView.GroupData);
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
            RegisterCompleteObjectUndo(_operationName);
            ClearSelection();
            var data = SerializationUtility.DeserializeValue<ClipBoard>(Encoding.UTF8.GetBytes(_serializedData), DataFormat.JSON, ClipBoard.objectReferences);
            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();
            foreach (var node in data.copiedNodes)
            {
                if (node == null)
                    continue;
                node.position.position += new Vector2(20, 20);
                string sourceGUID = node.GUID;
                // 新节点重置id
                BaseNode.IDAllocation(node);
                node.OnCreated();
                // 新节点与旧id存入字典
                copiedNodesMap[sourceGUID] = node;
                node.ClearConnectionsWithoutNotification();
                AddNode(node);
                AddToSelection(NodeViews[node.GUID]);
            }

            foreach (var group in data.copiedGroups)
            {
                group.position.position += new Vector2(20, 20);

                var oldGUIDList = group.innerNodeGUIDs.ToList();
                group.innerNodeGUIDs.Clear();

                foreach (var guid in oldGUIDList)
                {
                    if (copiedNodesMap.TryGetValue(guid, out var node))
                        group.innerNodeGUIDs.Add(node.GUID);
                }
                AddGroup(group);
            }

            foreach (var edge in data.copiedEdges)
            {
                edge.Enable(Graph);

                copiedNodesMap.TryGetValue(edge.InputNodeGUID, out var inputNode);
                copiedNodesMap.TryGetValue(edge.OutputNodeGUID, out var outputNode);

                inputNode = inputNode ?? edge.InputNode;
                outputNode = outputNode ?? edge.OutputNode;
                if (inputNode == null || outputNode == null) continue;

                inputNode.TryGetPort(edge.InputFieldName, out NodePort inputPort);
                outputNode.TryGetPort(edge.OutputFieldName, out NodePort outputPort);
                if (!inputPort.IsMulti && inputPort.IsConnected) continue;
                if (!outputPort.IsMulti && outputPort.IsConnected) continue;

                if (NodeViews.TryGetValue(inputNode.GUID, out BaseNodeView inputNodeView)
                    && NodeViews.TryGetValue(outputNode.GUID, out BaseNodeView outputNodeView))
                {
                    Connect(inputNodeView.PortViews[edge.InputFieldName], outputNodeView.PortViews[edge.OutputFieldName]);
                }
            }

            SetDirty();
        }

        /// <summary> 转换发生改变时调用 </summary>
        void ViewTransformChangedCallback(GraphView view)
        {
            if (GraphAsset != null)
            {
                Graph.Position = viewTransform.position;
                Graph.Scale = viewTransform.scale;
            }
        }
        #endregion

        void KeyDownCallback(KeyDownEvent evt)
        {
            if ((evt.commandKey || evt.ctrlKey) && evt.keyCode == KeyCode.S)
            {
                SaveGraphToDisk();
                evt.StopPropagation();
            }
        }

        void MouseUpCallback(MouseUpEvent evt)
        {
            UpdateNodeInspectorSelection();
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
                RegisterCompleteObjectUndo("Create Parameter Node");

                foreach (var paramFieldView in blackboardFields)
                {
                    var paramNode = BaseNode.CreateNew<ParameterNode>(mousePos);
                    paramNode.name = paramFieldView.text;
                    BaseNodeView paramNodeView = AddNode(paramNode);
                    mousePos += Vector2.one * 30;
                }
                SetDirty();
            }
        }
        #endregion

        #region Overrides
        protected virtual Type GetDefaultNodeViewType(Type _nodeDataType)
        {
            return typeof(BaseNodeView);
        }

        protected virtual IEnumerable<Type> GetNodeTypes()
        {
            foreach (var type in Utility_Refelection.GetChildrenTypes<BaseNode>())
            {
                if (type.IsAbstract) continue;
                yield return type;
            }
        }

        #endregion

        #region Graph修改

        public void UpdateNodeInspectorSelection()
        {
            foreach (var element in selection)
            {
                if (element is BaseNodeView nodeView)
                {
                    EditorGUILayoutExtension.DrawFieldsInInspector(nodeView.title, nodeView.NodeData);
                    Selection.activeObject = ObjectInspector.Instance;
                    break;
                }
            }
        }

        public RelayNodeView AddRelayNode(PortView _inputPortView, PortView _outputPortView, Vector2 _position)
        {
            var relayNode = BaseNode.CreateNew<RelayNode>(_position);
            var nodeView = AddNode(relayNode) as RelayNodeView;

            if (_outputPortView != null)
                Connect(nodeView.PortViews["input"], _outputPortView);

            if (_inputPortView != null)
                Connect(_inputPortView, nodeView.PortViews["output"]);
            return nodeView;
        }

        public void RemoveRelayNode(RelayNodeView _relayNodeView)
        {
            // 获取relayNodeViewinput侧接口
            // 获取relayNodeViewoutput侧接口

            // 如果两个接口都不为空，连接这两个接口
        }

        public BaseNodeView AddNode(BaseNode _nodeData)
        {
            RegisterCompleteObjectUndo("AddNode " + _nodeData.GetType().Name);
            Graph.AddNode(_nodeData);
            BaseNodeView nodeView = AddNodeView(_nodeData);
            SetDirty();
            return nodeView;
        }

        public BaseNodeView AddNodeView(BaseNode _nodeData)
        {
            Type nodeViewType = null;
            if (_nodeData is ParameterNode parameterNode)
            {
                if (parameterNode.Parameter != null)
                    nodeViewType = NodeEditorUtility.GetParameterNodeCustomViewType(parameterNode.Parameter.GetType());
                else
                    nodeViewType = typeof(ParameterNodeView);
            }
            else
                nodeViewType = NodeEditorUtility.GetNodeViewType(_nodeData.GetType());
            if (nodeViewType == null)
                nodeViewType = GetDefaultNodeViewType(_nodeData.GetType());

            BaseNodeView nodeView = Activator.CreateInstance(nodeViewType) as BaseNodeView;
            AddElement(nodeView);
            NodeViews[_nodeData.GUID] = nodeView;
            nodeView.SetUp(_nodeData, CommandDispatcher, this);
            return nodeView;
        }

        public void RemoveNode(BaseNodeView _nodeView)
        {
            Graph.RemoveNode(_nodeView.NodeData);

            // 然后移除节点View
            RemoveNodeView(_nodeView);
        }

        public void RemoveNodeView(BaseNodeView _nodeView)
        {
            NodeViews.Remove(_nodeView.NodeData.GUID);
            RemoveElement(_nodeView);
        }

        void RemoveNodeViews()
        {
            foreach (var nodeView in NodeViews.Values)
                RemoveElement(nodeView as GraphElement);
            NodeViews.Clear();
        }

        public GroupView AddGroup(BaseGroup _groupData)
        {
            Graph.AddGroup(_groupData);
            return AddGroupView(_groupData);
        }

        public GroupView AddGroupView(BaseGroup _groupData)
        {
            var groupView = new GroupView();
            groupView.SetUp(_groupData, CommandDispatcher, this);
            GroupViews.Add(groupView);
            AddElement(groupView);
            return groupView;
        }

        public void AddSelectionsToGroup(GroupView _groupView)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    if (GroupViews.Exists(groupView => (groupView as GroupView).ContainsElement(selectedNode as BaseNodeView)))
                        continue;

                    _groupView.AddElement(selectedNode as BaseNodeView);
                }
            }
        }

        public void RemoveGroup(GroupView _groupView)
        {
            Graph.RemoveGroup(_groupView.GroupData);
            RemoveGroupView(_groupView);
        }

        public void RemoveGroupView(GroupView _groupView)
        {
            GroupViews.Remove(_groupView);
            foreach (var item in _groupView.containedElements)
            {
                AddElement(item);
            }
            RemoveElement(_groupView);
        }

        public void RemoveGroups()
        {
            foreach (var groupView in GroupViews)
                RemoveElement(groupView as GroupView);
            GroupViews.Clear();
        }

        public BaseStackNodeView AddStackNode(BaseStack _stackData)
        {
            Graph.AddStackNode(_stackData);
            return AddStackNodeView(_stackData);
        }

        public BaseStackNodeView AddStackNodeView(BaseStack _stackNode)
        {
            var stackViewType = NodeEditorUtility.GetStackNodeCustomViewType(_stackNode.GetType());
            var stackView = Activator.CreateInstance(stackViewType) as BaseStackNodeView;
            stackView.SetUp(_stackNode, CommandDispatcher, this);
            AddElement(stackView);
            StackNodeViews[_stackNode.GUID] = stackView;
            return stackView;
        }

        public void RemoveStackNode(BaseStackNodeView _stackNodeView)
        {
            Graph.RemoveStackNode(_stackNodeView.stackNode);
            RemoveStackNodeView(_stackNodeView);
        }

        public void RemoveStackNodeView(BaseStackNodeView _stackNodeView)
        {
            RemoveElement(_stackNodeView);
            StackNodeViews.Remove(_stackNodeView.stackNode.GUID);
        }

        public void RemoveStackNodeViews()
        {
            foreach (var stackView in StackNodeViews)
                RemoveElement(stackView.Value);
            StackNodeViews.Clear();
        }

        public bool ConnectView(EdgeView _edgeView)
        {
            var inputPortView = _edgeView.input as PortView;
            var outputPortView = _edgeView.output as PortView;

            var inputNodeView = inputPortView.node as BaseNodeView;
            var outputNodeView = outputPortView.node as BaseNodeView;

            if (!inputPortView.PortData.IsMulti)
            {
                foreach (var edge in EdgeViews.Where(ev => ev.input == _edgeView.input).ToList())
                {
                    DisconnectView(edge);
                }
            }
            if (!outputPortView.PortData.IsMulti)
            {
                foreach (var edge in EdgeViews.Where(ev => ev.output == _edgeView.output).ToList())
                {
                    DisconnectView(edge);
                }
            }

            inputPortView.Connect(_edgeView);
            outputPortView.Connect(_edgeView);

            _edgeView.isConnected = true;

            AddElement(_edgeView);
            EdgeViews.Add(_edgeView);

            schedule.Execute(() =>
            {
                _edgeView.UpdateEdgeControl();
            }).ExecuteLater(1);

            outputNodeView.OnPortConnected(outputPortView, inputPortView);
            inputNodeView.OnPortConnected(inputPortView, outputPortView);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();
            return true;
        }

        public bool ConnectView(PortView _inputPortView, PortView _outputPortView, SerializableEdge _serializableEdge)
        {
            var edgeView = new EdgeView()
            {
                userData = _serializableEdge,
                input = _inputPortView,
                output = _outputPortView,
            };
            edgeView.SetUp(_serializableEdge, CommandDispatcher, this);
            return ConnectView(edgeView);
        }

        public bool Connect(PortView _inputPortView, PortView _outputPortView)
        {
            var newEdge = Graph.Connect(_inputPortView.PortData, _outputPortView.PortData);

            var edgeView = new EdgeView()
            {
                userData = newEdge,
                input = _inputPortView,
                output = _outputPortView,
            };
            return ConnectView(edgeView);
        }

        public bool Connect(EdgeView _edgeView)
        {
            var inputPortView = _edgeView.input as PortView;
            var outputPortView = _edgeView.output as PortView;
            var inputNodeView = inputPortView.node as BaseNodeView;
            var outputNodeView = outputPortView.node as BaseNodeView;
            inputNodeView.NodeData.TryGetPort(inputPortView.FieldName, out NodePort inputPort);
            outputNodeView.NodeData.TryGetPort(outputPortView.FieldName, out NodePort outputPort);

            _edgeView.userData = Graph.Connect(inputPort, outputPort);

            ConnectView(_edgeView);

            return true;
        }

        public void Disconnect(PortView _portView)
        {
            if (_portView == null) return;

            foreach (var edgeView in _portView.connections.ToArray())
            {
                Disconnect(edgeView as EdgeView);
            }
        }

        public void Disconnect(EdgeView _edgeView)
        {
            if (_edgeView == null) return;

            Graph.Disconnect(_edgeView.EdgeData);
            DisconnectView(_edgeView);
        }

        public void DisconnectView(EdgeView _edgeView)
        {
            RemoveElement(_edgeView);
            PortView inputPortView = _edgeView.input as PortView;
            BaseNodeView inputNodeView = inputPortView.node as BaseNodeView;
            if (_edgeView.input != null
                && inputPortView != null
                && inputNodeView != null)
            {
                inputPortView.Disconnect(_edgeView);
            }

            PortView outputPortView = _edgeView.output as PortView;
            BaseNodeView outputNodeView = outputPortView.node as BaseNodeView;
            if (_edgeView.output != null
                && outputPortView != null
                && outputNodeView != null)
            {
                _edgeView.output.Disconnect(_edgeView);
            }
            EdgeViews.Remove(_edgeView);

            var inputNode = inputPortView.node as BaseNodeView;
            inputNode.OnPortDisconnected(_edgeView.input as PortView, _edgeView.output as PortView);

            var outputNode = _edgeView.output.node as BaseNodeView;
            outputNode.OnPortDisconnected(_edgeView.output as PortView, _edgeView.input as PortView);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();
        }


        public void RegisterCompleteObjectUndo(string name)
        {
            //Undo.RegisterCompleteObjectUndo(GraphData, name);
        }

        public void SaveGraphToDisk()
        {
            (GraphAsset as IGraphAsset)?.SaveGraph();
            GraphWindow.GraphOwner?.SaveVariables();
            SetDirty(true);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveOpenScenes();
        }

        public virtual void ResetPositionAndZoom()
        {
            Graph.Position = Vector3.zero;
            Graph.Scale = Vector3.one;
        }
        #endregion

        #region 帮助方法
        public void SetDirty(bool _immediately = false)
        {
            if (_immediately)
                EditorUtility.SetDirty(GraphAsset);
            else
                IsDirty = true;
        }
        #endregion
    }
}