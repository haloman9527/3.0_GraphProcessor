using CZToolKit.Core;
using CZToolKit.Core.Editors;
using OdinSerializer;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseGraphView : GraphView
    {
        const string GraphViewStylePath = "GraphProcessor/Styles/BaseGraphView";
        static StyleSheet graphViewStyle;
        public static StyleSheet GraphViewStyle
        {
            get
            {
                if (graphViewStyle == null)
                    graphViewStyle = Resources.Load<StyleSheet>(GraphViewStylePath);
                return graphViewStyle;
            }
        }

        public BaseEdgeConnectorListener connectorListener;

        List<IOnGUIObserver> onGUIObservers = new List<IOnGUIObserver>(16);

        protected virtual Type GetDefaultNodeViewType(Type _nodeDataType) { return typeof(BaseNodeView); }

        public bool Initialized { get; private set; }
        public Action OnInitializeCompleted { get; set; }
        public bool IsDirty { get; private set; } = false;
        private ExposedParameterView Blackboard { get; set; }
        public CreateNodeMenuWindow CreateNodeMenu { get; private set; }
        public BaseGraphWindow GraphWindow { get; private set; }
        public UnityObject GraphAsset { get; private set; }
        public IBaseGraph Graph { get; private set; }
        public SerializedObject SerializedObject { get; private set; }
        public Dictionary<string, BaseNodeView> NodeViews { get; private set; } = new Dictionary<string, BaseNodeView>();
        public List<EdgeView> EdgeViews { get; private set; } = new List<EdgeView>();
        public List<GroupView> GroupViews { get; private set; } = new List<GroupView>();
        public Dictionary<string, BaseStackNodeView> StackNodeViews { get; private set; } = new Dictionary<string, BaseStackNodeView>();
        public List<IOnGUIObserver> OnGUIObservers { get { return onGUIObservers; } }

        protected override bool canCopySelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView || e is BaseStackNodeView); }
        }

        protected override bool canCutSelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView || e is BaseStackNodeView); }
        }

        public BaseGraphView()
        {
            styleSheets.Add(GraphViewStyle);
            GridBackground gridBackground = new GridBackground();
            gridBackground.style.backgroundColor = new Color(1f, 1f, 1f);
            Insert(0, gridBackground);
            SetupZoom(0.05f, 2f);
            this.StretchToParentSize();
        }

        #region Initialize
        protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener()
        {
            return new BaseEdgeConnectorListener(this);
        }

        public void Initialize(BaseGraphWindow _window, IBaseGraph _graph)
        {
            if (Initialized) return;
            GraphWindow = _window;
            Graph = _graph;
            GraphAsset = (_graph as IBaseGraphFromAsset)?.From;
            SerializedObject = new SerializedObject(GraphAsset);
            GraphWindow.Toolbar.AddButton("Center", () =>
            {
                ResetPositionAndZoom();
                UpdateViewTransform(Graph.Position, Graph.Scale);
            });
            GraphWindow.Toolbar.AddToggle("Show Parameters", Graph.BlackboardVisible, (v) =>
            {
                GetBlackboard().style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
                Graph.BlackboardVisible = v;
            });

            connectorListener = CreateEdgeConnectorListener();

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
            InitViewAndCallbacks();
            InitializeGraphView();
            InitializeNodeViews();
            InitializeEdgeViews();
            InitializeStackNodes();
            InitializeGroups();
            InitializeBlackboard();

            OnInitializeCompleted += OnInitialized;
            Initialized = true;
        }

        protected virtual void OnInitialized() { }

        void InitViewAndCallbacks()
        {
            serializeGraphElements = SerializeGraphElementsCallback;
            canPasteSerializedData = CanPasteSerializedDataCallback;
            unserializeAndPaste = DeserializeAndPasteCallback;
            graphViewChanged = GraphViewChangedCallback;
            viewTransformChanged = ViewTransformChangedCallback;

            RegisterCallback<KeyDownEvent>(KeyDownCallback);
            RegisterCallback<DragPerformEvent>(DragPerformedCallback);
            RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
            RegisterCallback<MouseDownEvent>(MouseDownCallback);
            RegisterCallback<MouseUpEvent>(MouseUpCallback);

            InitializeManipulators();

            CreateNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            CreateNodeMenu.Initialize(this, GetNodeTypes());
        }

        protected virtual void InitializeManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        void InitializeGraphView()
        {
            viewTransform.position = Graph.Position;
            viewTransform.scale = Graph.Scale;
            nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), CreateNodeMenu);
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

        /// <summary> 初始化所有连接的视图 </summary>
        void InitializeEdgeViews()
        {
            foreach (var serializedEdge in Graph.EdgesGUIDMapping)
            {
                if (serializedEdge.Value == null) continue;
                BaseNodeView inputNodeView = null, outputNodeView = null;
                if (serializedEdge.Value.InputNode != null)
                    NodeViews.TryGetValue(serializedEdge.Value.InputNodeGUID, out inputNodeView);
                if (serializedEdge.Value.OutputNode != null)
                    NodeViews.TryGetValue(serializedEdge.Value.OutputNodeGUID, out outputNodeView);
                if (inputNodeView == null || outputNodeView == null)
                    continue;
                ConnectView(inputNodeView.PortViews[serializedEdge.Value.InputFieldName], outputNodeView.PortViews[serializedEdge.Value.OutputFieldName], serializedEdge.Value);
            }
        }

        /// <summary> 初始化所有Group的视图 </summary>
        void InitializeGroups()
        {
            foreach (var group in Graph.Groups)
                AddGroupView(group);
        }

        void InitializeStackNodes()
        {
            foreach (var stackNode in Graph.StackNodesGUIDMapping.Values)
                AddStackNodeView(stackNode);
        }

        void InitializeBlackboard()
        {
            Blackboard = new ExposedParameterView(this);
            Blackboard.SetPosition(Graph.BlackboardPosition);
            Blackboard.style.display = Graph.BlackboardVisible ? DisplayStyle.Flex : DisplayStyle.None;
            Add(Blackboard);
        }

        #endregion

        public virtual void OnGUI()
        {
            foreach (var observer in OnGUIObservers)
                observer.OnGUI();
        }

        public override Blackboard GetBlackboard()
        {
            return Blackboard;
        }

        protected virtual IEnumerable<Type> GetNodeTypes()
        {
            foreach (var type in Utility_Refelection.GetChildrenTypes<BaseNode>())
            {
                if (type.IsAbstract) continue;
                yield return type;
            }
        }

        #region Callbacks

        #region 系统回调
        /// <summary> 构建右键菜单 </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("New Stack", (e) =>
            {
                BaseStack stackNode = new BaseStack(position);
                stackNode.OnCreated();
                AddStackNode(stackNode);
            }, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("New Group", (e) => AddSelectionsToGroup(AddGroup(new BaseGroup("New Group", position))), DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(GraphAsset), DropdownMenuAction.AlwaysEnabled);

            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Save Asset", (e) =>
            {
                SetDirty();
                AssetDatabase.SaveAssets();
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

            IBasePortView startPortView = _startPortView as IBasePortView;
            ports.ForEach(_portView =>
            {
                IBasePortView portView = _portView as IBasePortView;

                if (portView.Owner == startPortView.Owner)
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
        #endregion

        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new CopyPasteHelper();

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
            ClearSelection();
            return Encoding.UTF8.GetString(SerializationUtility.SerializeValue(data, DataFormat.JSON, out CopyPasteHelper.objectReferences));
        }

        bool CanPasteSerializedDataCallback(string _serializedData)
        {
            return !string.IsNullOrEmpty(_serializedData);
        }

        void DeserializeAndPasteCallback(string _operationName, string _serializedData)
        {
            RegisterCompleteObjectUndo(_operationName);
            var data = SerializationUtility.DeserializeValue<CopyPasteHelper>(Encoding.UTF8.GetBytes(_serializedData), DataFormat.JSON, CopyPasteHelper.objectReferences);

            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();
            foreach (var node in data.copiedNodes)
            {
                //var node = JsonSerializer.Deserialize(node) as BaseNode;
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
                //var group = JsonSerializer.Deserialize<BaseGroup>(group);
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
                edge.Initialize(Graph);
                //var edge = JsonSerializer.Deserialize<SerializableEdge>(serializedEdge);

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

            this.SetDirty(true);
        }

        /// <summary> GraphView发生改变时调用 </summary>
        GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {
            if (changes.elementsToRemove != null)
            {
                RegisterCompleteObjectUndo("Remove Graph Elements");

                changes.elementsToRemove.Sort((e1, e2) =>
                {
                    int GetPriority(GraphElement e)
                    {
                        if (e is BaseNodeView)
                            return 0;
                        else
                            return 1;
                    }
                    return GetPriority(e1).CompareTo(GetPriority(e2));
                });

                // 捕获所有元素的移除请求
                changes.elementsToRemove.RemoveAll(_element =>
                {
                    switch (_element)
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
                        case GroupView groupView:
                            RemoveGroup(groupView);
                            return true;
                        case BaseStackNodeView stackNodeView:
                            RemoveStackNode(stackNodeView);
                            return true;
                    }

                    return false;
                });

                this.SetDirty();
            }

            return changes;
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

        protected virtual void KeyDownCallback(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.S && e.commandKey)
            {
                SaveGraphToDisk();
                e.StopPropagation();
            }
        }

        void MouseUpCallback(MouseUpEvent e)
        {
            schedule.Execute(() =>
            {
                if (DoesSelectionContainsInspectorNodes())
                    UpdateNodeInspectorSelection();
            }).ExecuteLater(1);
        }

        void MouseDownCallback(MouseDownEvent e)
        {
            // When left clicking on the graph (not a node or something else)
            if (e.button == 0)
            {
                foreach (var nodeView in NodeViews.Values)
                {
                    if (nodeView is IHasSettingNodeView settingNodeView)
                        settingNodeView.CloseSettings();
                }
            }

            if (DoesSelectionContainsInspectorNodes())
                UpdateNodeInspectorSelection();
        }

        bool DoesSelectionContainsInspectorNodes()
        {
            return true;
            //return selection.Any(s => s is BaseNodeView v && v.nodeData.needsInspector);
        }

        void DragPerformedCallback(DragPerformEvent e)
        {
            var mousePos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            if (dragData == null)
                return;

            var exposedParameterFieldViews = dragData.OfType<BlackboardField>();
            if (exposedParameterFieldViews.Any())
            {
                foreach (var paramFieldView in exposedParameterFieldViews)
                {
                    RegisterCompleteObjectUndo("Create Parameter Node");
                    var paramNode = BaseNode.CreateNew<ParameterNode>(mousePos);
                    paramNode.name = paramFieldView.text;
                    AddNode(paramNode);
                    this.SetDirty();
                }
            }
        }

        void DragUpdatedCallback(DragUpdatedEvent e)
        {
            var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            bool dragging = false;

            if (dragData != null)
            {
                // Handle drag from exposed parameter view
                if (dragData.OfType<BlackboardField>().Any())
                    dragging = true;
            }

            if (dragging)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            UpdateNodeInspectorSelection();
        }

        #endregion

        #region API
        public void SetDirty(bool _immediately = false)
        {
            if (_immediately)
                EditorUtility.SetDirty(GraphAsset);
            else
                IsDirty = true;
        }

        #endregion

        #region Graph content modification

        public void UpdateNodeInspectorSelection()
        {
            HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
            bool drawnNode = false;
            foreach (var element in selection)
            {
                if (element is BaseNodeView nodeView && Contains(nodeView))
                {
                    EditorGUILayoutExtension.DrawFieldsInInspector(nodeView.title, nodeView.NodeData);
                    drawnNode = true;
                }
            }
            if (!drawnNode)
                Selection.activeObject = null;
            //if (!drawnNode)
            //    Selection.activeObject = GraphAsset;
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
            nodeView.Initialize(this, _nodeData);
            if (nodeView is IOnGUIObserver observer)
                onGUIObservers.Add(observer);
            return nodeView;
        }

        public void RemoveNode(BaseNodeView _nodeView)
        {
            // 先断开所有连线
            foreach (var portView in _nodeView.PortViews.Values)
            {
                Disconnect(portView);
            }

            Graph.RemoveNode(_nodeView.NodeData);

            // 然后移除节点View
            RemoveNodeView(_nodeView);
            UpdateNodeInspectorSelection();
        }

        public void RemoveNodeView(BaseNodeView _nodeView)
        {
            NodeViews.Remove(_nodeView.NodeData.GUID);
            if (_nodeView is IOnGUIObserver observer)
                onGUIObservers.Remove(observer);
            RemoveElement(_nodeView);
            UpdateNodeInspectorSelection();
        }

        void RemoveNodeViews()
        {
            foreach (var nodeView in NodeViews.Values)
                RemoveElement(nodeView);
            NodeViews.Clear();
            onGUIObservers.Clear();
        }

        public GroupView AddGroup(BaseGroup _groupData)
        {
            Graph.AddGroup(_groupData);
            return AddGroupView(_groupData);
        }

        public GroupView AddGroupView(BaseGroup _groupData)
        {
            var groupView = new GroupView();
            groupView.Initialize(this, _groupData);
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
                    if (GroupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))
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
            //_groupView.Clear();
            RemoveElement(_groupView);
        }

        public void RemoveGroups()
        {
            foreach (var groupView in GroupViews)
                RemoveElement(groupView);
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
            stackView.Initialize(this, _stackNode);
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

        void RemoveStackNodeViews()
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
            if (!(_edgeView.output as PortView).PortData.IsMulti)
            {
                foreach (var edge in EdgeViews.Where(ev => ev.output == _edgeView.output).ToList())
                {
                    DisconnectView(edge);
                }
            }

            inputPortView.Connect(_edgeView);
            outputPortView.Connect(_edgeView);


            AddElement(_edgeView);
            EdgeViews.Add(_edgeView);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            schedule.Execute(() =>
            {
                _edgeView.UpdateEdgeControl();
            }).ExecuteLater(1);

            _edgeView.isConnected = true;
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
            return ConnectView(edgeView);
        }

        public bool Connect(PortView _inputPortView, PortView _outputPortView)
        {
            if (_inputPortView.Owner.parent == null || _outputPortView.Owner.parent == null)
                return false;

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
            var inputPortView = _edgeView.input as IBasePortView;
            var outputPortView = _edgeView.output as IBasePortView;
            var inputNodeView = inputPortView.Owner;
            var outputNodeView = outputPortView.Owner;
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

            if (_edgeView.input != null &&
                _edgeView.input is PortView inputPortView &&
                inputPortView.node is BaseNodeView inputNodeView)
            {
                inputPortView.Disconnect(_edgeView);
                inputNodeView.RefreshPorts();
            }
            if (_edgeView.output != null &&
                _edgeView.output is PortView ouputPortView &&
                ouputPortView.node is BaseNodeView outputNodeView)
            {
                _edgeView.output.Disconnect(_edgeView);
                outputNodeView.RefreshPorts();
            }
            EdgeViews.Remove(_edgeView);
        }

        /// <summary> 不影响数据 </summary>
        public void RemoveEdgeViews()
        {
            foreach (var edge in EdgeViews)
                RemoveElement(edge);
            EdgeViews.Clear();
        }

        public void RegisterCompleteObjectUndo(string name)
        {
            //Undo.RegisterCompleteObjectUndo(GraphData, name);
        }

        public void SaveGraphToDisk(bool _immediately = false)
        {
            if (GraphAsset == null) return;
            SetDirty(true);
            if (_immediately)
                AssetDatabase.SaveAssets();
        }

        public virtual void ResetPositionAndZoom()
        {
            Graph.Position = Vector3.zero;
            Graph.Scale = Vector3.one;
        }

        /// <summary> Deletes the selected content, can be called form an IMGUI container </summary>
        public void DelayedDeleteSelection() => this.schedule.Execute(() => DeleteSelectionOperation("Delete", AskUser.DontAskUser)).ExecuteLater(0);

        #endregion
    }
}