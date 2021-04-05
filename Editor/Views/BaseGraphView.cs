using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using CZToolKit.Core;
using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;

namespace GraphProcessor.Editors
{
    public class BaseGraphView : GraphView
    {
        const string GraphViewStylePath = "GraphProcessorStyles/BaseGraphView";
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

        /// <summary> Object to handle nodes that shows their UI in the inspector. </summary>
        [NonSerialized] protected NodeInspectorObject nodeInspector;

        ExposedParameterView blackboard;

        /// <summary> Connector listener that will create the edges between ports </summary>
        public BaseEdgeConnectorListener connectorListener;

        Dictionary<string, BaseNodeView> nodeViews = new Dictionary<string, BaseNodeView>();

        List<EdgeView> edgeViews = new List<EdgeView>();

        List<GroupView> groupViews = new List<GroupView>();

        Dictionary<string, BaseStackNodeView> stackNodeViews = new Dictionary<string, BaseStackNodeView>();

        public bool Initialized { get; private set; }
        public CreateNodeMenuWindow CreateNodeMenu { get; private set; }
        public BaseGraphWindow GraphWindow { get; private set; }
        public BaseGraph GraphData { get; private set; }
        public Dictionary<string, BaseNodeView> NodeViews { get { return nodeViews; } }
        public List<EdgeView> EdgeViews { get { return edgeViews; } }
        public List<GroupView> GroupViews { get { return groupViews; } }
        public Dictionary<string, BaseStackNodeView> StackNodeViews { get { return stackNodeViews; } }

        protected override bool canCopySelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView || e is BaseStackNodeView); }
        }

        protected override bool canCutSelection
        {
            get { return selection.Any(e => e is BaseNodeView || e is GroupView || e is BaseStackNodeView); }
        }

        #region Initialize
        public void Initialize(BaseGraphWindow _window, BaseGraph _graphData)
        {
            if (Initialized) return;

            GraphWindow = _window;
            GraphData = _graphData;

            connectorListener = CreateEdgeConnectorListener();

            InitViewAndCallbacks();
            InitializeGraphView();
            InitializeNodeViews();
            InitializeEdgeViews();
            InitializeStackNodes();
            InitializeGroups();
            InitializeBlackboard();

            OnInitialized();
            Initialized = true;
        }

        protected virtual void OnInitialized() { }

        void InitViewAndCallbacks()
        {
            styleSheets.Add(GraphViewStyle);
            GridBackground gridBackground = new GridBackground();
            gridBackground.style.backgroundColor = new Color(1f, 1f, 1f);
            Insert(0, gridBackground);
            SetupZoom(0.05f, 2f);
            this.StretchToParentSize();

            serializeGraphElements = SerializeGraphElementsCallback;
            canPasteSerializedData = CanPasteSerializedDataCallback;
            unserializeAndPaste = UnserializeAndPasteCallback;

            graphViewChanged = GraphViewChangedCallback;
            viewTransformChanged = ViewTransformChangedCallback;
            elementResized = ElementResizedCallback;

            EditorSceneManager.sceneSaved += _ => SaveGraphToDisk();

            RegisterCallback<DetachFromPanelEvent>(OnDetachPanel);
            RegisterCallback<KeyDownEvent>(KeyDownCallback);
            RegisterCallback<DragPerformEvent>(DragPerformedCallback);
            RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
            RegisterCallback<MouseDownEvent>(MouseDownCallback);
            RegisterCallback<MouseUpEvent>(MouseUpCallback);

            InitializeManipulators();

            CreateNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
            CreateNodeMenu.Initialize(this, GetNodeTypes());

            if (nodeInspector == null)
                nodeInspector = CreateNodeInspectorObject();

            Undo.undoRedoPerformed += ReloadView;
        }

        private void OnDetachPanel(DetachFromPanelEvent evt)
        {
            Undo.undoRedoPerformed -= ReloadView;
        }

        protected virtual void InitializeManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        void InitializeGraphView()
        {
            //graphData.onGraphChanges += GraphChangesCallback;
            viewTransform.position = GraphData.position;
            viewTransform.scale = GraphData.scale;
            nodeCreationRequest = (c) => SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), CreateNodeMenu);
        }

        /// <summary> 初始化所有节点视图 </summary>
        void InitializeNodeViews()
        {
            // 清理空节点
            foreach (var key in GraphData.Nodes.Keys.ToList())
            {
                if (GraphData.Nodes[key] == null)
                    GraphData.Nodes.Remove(key);
            }

            foreach (var node in GraphData.Nodes.Values)
            {
                var v = AddNodeView(node);
            }
        }

        /// <summary> 初始化所有连接的视图 </summary>
        void InitializeEdgeViews()
        {
            // 清理空连接
            foreach (var item in GraphData.Edges.Keys.ToList())
            {
                if (GraphData.Edges[item] == null)
                    GraphData.Edges.Remove(item);
            }

            foreach (var serializedEdge in GraphData.Edges.Values)
            {
                BaseNodeView inputNodeView = null, outputNodeView = null;
                if (serializedEdge.InputNode != null)
                    nodeViews.TryGetValue(serializedEdge.InputNodeGUID, out inputNodeView);
                if (serializedEdge.OutputNode != null)
                    nodeViews.TryGetValue(serializedEdge.OutputNodeGUID, out outputNodeView);
                if (inputNodeView == null || outputNodeView == null)
                    continue;

                var edgeView = new EdgeView()
                {
                    userData = serializedEdge,
                    input = inputNodeView.PortViews[serializedEdge.InputFieldName],
                    output = outputNodeView.PortViews[serializedEdge.OutputFieldName]
                };

                ConnectView(edgeView);
            }
        }

        /// <summary> 初始化所有Group的视图 </summary>
        void InitializeGroups()
        {
            foreach (var group in GraphData.Groups)
                AddGroupView(group);
        }

        void InitializeStackNodes()
        {
            foreach (var stackNode in GraphData.StackNodes.Values)
                AddStackNodeView(stackNode);
        }

        void InitializeBlackboard()
        {
            blackboard = new ExposedParameterView(this);
            blackboard.SetPosition(GraphData.blackboardPosition);
            blackboard.style.display = GraphData.blackboardoVisible ? DisplayStyle.Flex : DisplayStyle.None;
            Add(blackboard);
        }

        protected virtual void Reload() { }
        #endregion

        public override Blackboard GetBlackboard() { return blackboard; }

        protected virtual IEnumerable<Type> GetNodeTypes()
        {
            return ChildrenTypeCache.GetChildrenTypes<BaseNode>();
        }

        protected virtual NodeInspectorObject CreateNodeInspectorObject()
        {
            var inspector = ScriptableObject.CreateInstance<NodeInspectorObject>();
            inspector.name = "Node Inspector";
            inspector.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;

            return inspector;
        }

        #region Callbacks

        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            var data = new CopyPasteHelper();

            foreach (BaseNodeView nodeView in elements.Where(e => e is BaseNodeView))
                data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.NodeData));

            foreach (BaseStackNodeView stackNodeView in elements.Where(e => e is BaseStackNodeView))
                data.copiedStacks.Add(JsonSerializer.Serialize(stackNodeView.stackNode));

            foreach (GroupView groupView in elements.Where(e => e is GroupView))
                data.copiedGroups.Add(JsonSerializer.Serialize(groupView.group));

            foreach (EdgeView edgeView in elements.Where(e => e is EdgeView))
                data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.serializedEdge));

            ClearSelection();

            return JsonUtility.ToJson(data, true);
        }

        bool CanPasteSerializedDataCallback(string serializedData)
        {
            try
            {
                return JsonUtility.FromJson(serializedData, typeof(CopyPasteHelper)) != null;
            }
            catch
            {
                return false;
            }
        }

        void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            var data = JsonUtility.FromJson<CopyPasteHelper>(serializedData);

            RegisterCompleteObjectUndo(operationName);

            Dictionary<string, BaseNode> copiedNodesMap = new Dictionary<string, BaseNode>();

            foreach (var serializedNode in data.copiedNodes)
            {
                var node = JsonSerializer.DeserializeNode(serializedNode);
                if (node == null)
                    continue;
                node.position.position += new Vector2(20, 20);
                string sourceGUID = node.GUID;
                // 新节点重置id
                node.OnCreated();
                // 新节点与旧id存入字典
                copiedNodesMap[sourceGUID] = node;
                AddNode(node);
                AddToSelection(nodeViews[node.GUID]);
            }

            foreach (var serializedGroup in data.copiedGroups)
            {
                var group = JsonSerializer.Deserialize<BaseGroup>(serializedGroup);

                //Same than for node
                group.OnCreated();

                // try to centre the created node in the screen
                group.position.position += new Vector2(20, 20);

                var oldGUIDList = group.innerNodeGUIDs.ToList();
                group.innerNodeGUIDs.Clear();
                foreach (var guid in oldGUIDList)
                {
                    GraphData.Nodes.TryGetValue(guid, out var node);

                    // In case group was copied from another graph
                    if (node == null)
                    {
                        copiedNodesMap.TryGetValue(guid, out node);
                        group.innerNodeGUIDs.Add(node.GUID);
                    }
                    else
                    {
                        group.innerNodeGUIDs.Add(copiedNodesMap[guid].GUID);
                    }
                }

                AddGroup(group);
            }

            foreach (var serializedEdge in data.copiedEdges)
            {
                var edge = JsonSerializer.Deserialize<SerializableEdge>(serializedEdge);

                copiedNodesMap.TryGetValue(edge.InputNodeGUID, out var inputNode);
                copiedNodesMap.TryGetValue(edge.OutputNodeGUID, out var outputNode);

                inputNode = inputNode ?? edge.InputNode;
                outputNode = outputNode ?? edge.OutputNode;
                if (inputNode == null || outputNode == null) continue;

                inputNode.TryGetPort(edge.InputPort.FieldName, out NodePort inputPort);
                outputNode.TryGetPort(edge.OutputPort.FieldName, out NodePort outputPort);
                if (!inputPort.IsMulti && inputPort.IsConnected) continue;
                if (!outputPort.IsMulti && outputPort.IsConnected) continue;

                if (nodeViews.TryGetValue(inputNode.GUID, out BaseNodeView inputNodeView)
                    && nodeViews.TryGetValue(outputNode.GUID, out BaseNodeView outputNodeView))
                {
                    Connect(inputNodeView.PortViews[edge.InputFieldName], outputNodeView.PortViews[edge.OutputFieldName]);
                }
            }
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

                //Handle ourselves the edge and node remove
                changes.elementsToRemove.RemoveAll(e =>
                {
                    switch (e)
                    {
                        case EdgeView edgeView:
                            Disconnect(edgeView);
                            return true;
                        case BaseNodeView nodeView:
                            RemoveNode(nodeView);
                            return true;
                        case BlackboardField blackboardField:
                            if (GraphData.RemoveExposedParameter(blackboardField.userData as ExposedParameter))
                                blackboard.RemoveField(blackboardField);
                            return true;
                        case GroupView groupView:
                            RemoveGroup(groupView);
                            return true;
                        case BaseStackNodeView stackNodeView:
                            RemoveStackNodeView(stackNodeView);
                            return true;
                    }

                    return false;
                });
            }

            return changes;
        }

        /// <summary> 转换发生改变时调用 </summary>
        void ViewTransformChangedCallback(GraphView view)
        {
            if (GraphData != null)
            {
                GraphData.position = viewTransform.position;
                GraphData.scale = viewTransform.scale;
            }
        }

        /// <summary> 元素大小发生改变时调用 </summary>
        void ElementResizedCallback(VisualElement elem)
        {
            var groupView = elem as GroupView;

            if (groupView != null)
                groupView.group.size = groupView.GetPosition().size;
        }

        #region 系统回调
        /// <summary> 构建右键菜单 </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("New Stack", (e) =>
            {
                BaseStackNode stackNode = new BaseStackNode(position);
                stackNode.OnCreated();
                AddStackNode(stackNode);
            }, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("New Group", (e) => AddSelectionsToGroup(AddGroup(new BaseGroup("New Group", position))), DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(GraphData), DropdownMenuAction.AlwaysEnabled);

            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Save Asset", (e) =>
            {
                EditorUtility.SetDirty(GraphData);
                AssetDatabase.SaveAssets();
            }, DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Help/Reset Blackboard Windows", e =>
            {
                blackboard.SetPosition(new Rect(Vector2.zero, BaseGraph.DefaultBlackboardSize));
            });
        }

        /// <summary> 获取兼容窗口 </summary>
        public override List<Port> GetCompatiblePorts(Port _startPortView, NodeAdapter _nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            PortView startPortView = _startPortView as PortView;

            ports.ForEach(_portView =>
            {
                PortView portView = _portView as PortView;

                if (portView.Owner == startPortView.Owner)
                    return;

                if (portView.direction == startPortView.direction)
                    return;

                if (portView.Edges.Any(e => e.input == startPortView || e.output == startPortView))
                    return;

                if (startPortView.portData.TypeConstraint == PortTypeConstraint.None || portView.portData.TypeConstraint == PortTypeConstraint.None)
                {
                    compatiblePorts.Add(_portView);
                    return;
                }
                if (startPortView.portData.TypeConstraint == PortTypeConstraint.Inherited && startPortView.portData.DisplayType.IsAssignableFrom(portView.portData.DisplayType))
                {
                    compatiblePorts.Add(_portView);
                    return;
                }
                if (startPortView.portData.TypeConstraint == PortTypeConstraint.Strict && startPortView.portData.DisplayType == portView.portData.DisplayType)
                {
                    compatiblePorts.Add(_portView);
                    return;
                }
            });
            return compatiblePorts;
        }

        #endregion

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
                foreach (var nodeView in nodeViews.Values)
                {
                    nodeView.CloseSettings();
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
                    paramNode.paramGUID = (paramFieldView.userData as ExposedParameter).GUID;
                    AddNode(paramNode);
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

        #region Initialization

        void ReloadView()
        {
            var selectedNodeGUIDs = new List<string>();
            foreach (var e in selection)
            {
                if (e is BaseNodeView v && this.Contains(v))
                    selectedNodeGUIDs.Add(v.NodeData.GUID);
            }

            // Remove everything
            RemoveNodeViews();
            RemoveEdgeViews();
            RemoveGroups();
            RemoveStackNodeViews();

            // And re-add with new up to date datas
            InitializeNodeViews();
            InitializeEdgeViews();
            InitializeStackNodes();
            InitializeGroups();

            Reload();

            // Restore selection after re-creating all views
            // selection = nodeViews.Where(v => selectedNodeGUIDs.Contains(v.nodeTarget.GUID)).Select(v => v as ISelectable).ToList();
            foreach (var guid in selectedNodeGUIDs)
            {
                AddToSelection(nodeViews[guid]);
            }

            UpdateNodeInspectorSelection();
        }

        public void ClearGraphElements()
        {
            RemoveGroups();
            RemoveNodeViews();
            RemoveEdgeViews();
            RemoveStackNodeViews();
        }

        /// <summary> Allow you to create your own edge connector listener </summary>
        protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener() { return new BaseEdgeConnectorListener(this); }

        #endregion

        #region Graph content modification

        public void UpdateNodeInspectorSelection()
        {
            if (nodeInspector.previouslySelectedObject != Selection.activeObject)
                nodeInspector.previouslySelectedObject = Selection.activeObject;

            HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
            nodeInspector.selectedNodes.Clear();
            foreach (var e in selection)
            {
                if (e is BaseNodeView v && this.Contains(v))
                    selectedNodeViews.Add(v);
            }

            nodeInspector.UpdateSelectedNodes(selectedNodeViews);

            if (selectedNodeViews.Count > 0)
            {
                if (Selection.activeObject != nodeInspector)
                    Selection.activeObject = nodeInspector;
            }
            else
            {
                Selection.activeObject = GraphData;
            }
        }

        public RelayNodeView AddRelayNode(PortView inputPort, PortView outputPort, Vector2 position)
        {
            var relayNode = BaseNode.CreateNew<RelayNode>(position);
            var view = AddNode(relayNode) as RelayNodeView;

            if (outputPort != null)
                Connect(view.PortViews["input"], outputPort);

            if (inputPort != null)
                Connect(inputPort, view.PortViews["output"]);
            return view;
        }

        public BaseNodeView AddNode(BaseNode node)
        {
            GraphData.AddNode(node);
            BaseNodeView nodeView = AddNodeView(node);
            EditorUtility.SetDirty(GraphData);
            RegisterCompleteObjectUndo("AddNode " + node.GetType().Name);
            return nodeView;
        }

        public BaseNodeView AddNodeView(BaseNode node)
        {
            Type nodeViewType = NodeEditorUtility.GetNodeViewType(node.GetType());

            BaseNodeView nodeView = Activator.CreateInstance(nodeViewType) as BaseNodeView;
            nodeView.Initialize(this, node);
            AddElement(nodeView);
            nodeViews[node.GUID] = nodeView;
            return nodeView;
        }

        public void RemoveNode(BaseNodeView _nodeView)
        {
            // 先断开所有连线
            foreach (var portView in _nodeView.PortViews.Values)
            {
                Disconnect(portView);
            }

            // 然后移除节点View
            RemoveNodeView(_nodeView);

            // 然后移除节点Data
            nodeInspector.NodeViewRemoved(_nodeView);
            GraphData.RemoveNode(_nodeView.NodeData);

            // 然后更新绘制
            if (Selection.activeObject == nodeInspector)
                UpdateNodeInspectorSelection();
        }

        public void RemoveNodeView(BaseNodeView _nodeView)
        {
            RemoveElement(_nodeView);
            nodeViews.Remove(_nodeView.NodeData.GUID);
        }

        void RemoveNodeViews()
        {
            foreach (var nodeView in nodeViews.Values)
                RemoveElement(nodeView);
            nodeViews.Clear();
        }

        public GroupView AddGroup(BaseGroup _group)
        {
            GraphData.AddGroup(_group);
            _group.OnCreated();
            return AddGroupView(_group);
        }

        public GroupView AddGroupView(BaseGroup block)
        {
            var groupView = new GroupView();
            groupView.Initialize(this, block);
            AddElement(groupView);
            groupViews.Add(groupView);
            return groupView;
        }

        public void AddSelectionsToGroup(GroupView view)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))
                        continue;

                    view.AddElement(selectedNode as BaseNodeView);
                }
            }
        }

        public void RemoveGroup(GroupView _groupView)
        {
            GraphData.RemoveGroup(_groupView.group);
            RemoveElement(_groupView);
            groupViews.Remove(_groupView);
        }

        public void RemoveGroups()
        {
            foreach (var groupView in groupViews)
                RemoveElement(groupView);
            groupViews.Clear();
        }

        public BaseStackNodeView AddStackNode(BaseStackNode stackNode)
        {
            GraphData.AddStackNode(stackNode);
            return AddStackNodeView(stackNode);
        }

        public BaseStackNodeView AddStackNodeView(BaseStackNode _stackNode)
        {
            var stackViewType = NodeEditorUtility.GetStackNodeCustomViewType(_stackNode.GetType());
            var stackView = Activator.CreateInstance(stackViewType) as BaseStackNodeView;
            stackView.Initialize(this, _stackNode);
            AddElement(stackView);
            stackNodeViews[_stackNode.GUID] = stackView;
            return stackView;
        }

        public void RemoveStackNodeView(BaseStackNodeView _stackNodeView)
        {
            GraphData.RemoveStackNode(_stackNodeView.stackNode);
            stackNodeViews.Remove(_stackNodeView.stackNode.GUID);
            RemoveElement(_stackNodeView);
        }

        void RemoveStackNodeViews()
        {
            foreach (var stackView in stackNodeViews)
                RemoveElement(stackView.Value);
            stackNodeViews.Clear();
        }

        public bool ConnectView(EdgeView e)
        {
            var inputPortView = e.input as PortView;
            var outputPortView = e.output as PortView;
            var inputNodeView = inputPortView.node as BaseNodeView;
            var outputNodeView = outputPortView.node as BaseNodeView;

            if (!inputPortView.portData.IsMulti)
            {
                foreach (var edge in edgeViews.Where(ev => ev.input == e.input).ToList())
                {
                    DisconnectView(edge);
                }
            }
            if (!(e.output as PortView).portData.IsMulti)
            {
                foreach (var edge in edgeViews.Where(ev => ev.output == e.output).ToList())
                {
                    DisconnectView(edge);
                }
            }

            e.input.Connect(e);
            e.output.Connect(e);


            AddElement(e);
            edgeViews.Add(e);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            schedule.Execute(() =>
            {
                e.UpdateEdgeControl();
            }).ExecuteLater(1);

            e.isConnected = true;
            return true;
        }

        public bool Connect(PortView inputPortView, PortView outputPortView)
        {
            if (inputPortView.Owner.parent == null || outputPortView.Owner.parent == null)
                return false;


            var newEdge = GraphData.Connect(inputPortView.portData, outputPortView.portData);

            var edgeView = new EdgeView()
            {
                userData = newEdge,
                input = inputPortView,
                output = outputPortView,
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

            _edgeView.userData = GraphData.Connect(inputPort, outputPort);

            ConnectView(_edgeView);


            return true;
        }

        public void Disconnect(PortView _portView)
        {
            if (_portView == null) return;

            foreach (var edgeView in _portView.Edges.ToArray())
            {
                Disconnect(edgeView);
            }
        }

        public void Disconnect(EdgeView _edgeView)
        {
            if (_edgeView == null) return;
            DisconnectView(_edgeView);
            GraphData.Disconnect(_edgeView.serializedEdge);
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
            edgeViews.Remove(_edgeView);
        }

        /// <summary> 不影响数据 </summary>
        public void RemoveEdgeViews()
        {
            foreach (var edge in edgeViews)
                RemoveElement(edge);
            edgeViews.Clear();
        }

        public void RegisterCompleteObjectUndo(string name)
        {
            //Undo.RegisterCompleteObjectUndo(GraphData, name);
        }

        public void SaveGraphToDisk()
        {
            if (GraphData == null) return;

            EditorUtility.SetDirty(GraphData);
        }

        public void ResetPositionAndZoom()
        {
            GraphData.position = Vector3.zero;
            GraphData.scale = Vector3.one;

            UpdateViewTransform(GraphData.position, GraphData.scale);
        }

        /// <summary> Deletes the selected content, can be called form an IMGUI container </summary>
        public void DelayedDeleteSelection() => this.schedule.Execute(() => DeleteSelectionOperation("Delete", AskUser.DontAskUser)).ExecuteLater(0);

        #endregion
    }
}