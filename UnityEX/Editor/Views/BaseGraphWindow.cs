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
using System.Collections.Generic;
using System.Reflection;
using Atom.UnityEditors;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityObject = UnityEngine.Object;

namespace Atom.GraphProcessor.Editors
{
    [EditorWindowTitle(title = "Graph Processor")]
    public abstract class BaseGraphWindow : BaseEditorWindow
    {
        #region Fields

        private Toolbar toolbarLeft;
        private Toolbar toolbarCenter;
        private Toolbar toolbarRight;
        private VisualElement graphViewContainer;
        private VisualElement inspectorView;
        private VisualElement inspectorHeader;
        private VisualElement inspectorBody;
        private VisualElement itemLibraryBody;
        private Label inspectorTitle;
        private Label inspectorHint;
        private ListView itemLibraryList;
        private TextField itemLibrarySearch;
        private List<NodeMenuWindow.INodeEntry> itemLibraryEntries;
        private List<NodeMenuWindow.INodeEntry> itemLibraryFiltered;
        private Foldout blackboardFoldout;
        private TextField blackboardKeyField;
        private TextField blackboardValueField;
        private Button blackboardSetButton;
        private Button blackboardRemoveButton;
        private ListView blackboardList;
        private readonly List<KeyValuePair<string, object>> blackboardEntries = new List<KeyValuePair<string, object>>(64);
        private readonly List<IGraphAsset> subgraphBreadcrumbStack = new List<IGraphAsset>(8);
        private VisualElement breadcrumbContainer;
        private Label breadcrumbPath;
        private Label diagnosticBadge;
        private ToolbarButton breadcrumbBackButton;

        [SerializeField] private UnityObject unityGraphOwner;
        [SerializeField] private UnityObject unityGraphAsset;
        private SerializedObject unityGraphAssetSO;
        private IGraphAsset graphAsset;
        private BaseGraphProcessor graphProcessor;
        private BaseGraphView graphView;
        private GraphViewContext context;
        private ToolbarButton togSnap;
        private ToolbarButton togInspector;
        private ToolbarButton togItemLibrary;

        #endregion

        #region Properties

        private VisualElement GraphViewContainer => graphViewContainer;
        private VisualElement InspectorView => inspectorView;

        public Toolbar ToolbarLeft => toolbarLeft;

        public Toolbar ToolbarCenter => toolbarCenter;

        public Toolbar ToolbarRight => toolbarRight;

        public IGraphOwner GraphOwner => unityGraphOwner as IGraphOwner;

        public IGraphAsset GraphAsset => graphAsset;

        public UnityObject UnityGraphAsset => unityGraphAsset;

        public SerializedObject UnityGraphAssetSO => unityGraphAssetSO;

        public BaseGraphProcessor GraphProcessor => graphProcessor;

        public BaseGraphView GraphView => graphView;

        #endregion

        #region Unity

        protected virtual void OnEnable()
        {
            InitRootVisualElement();
            Reload();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected virtual void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (Selection.activeObject is ObjectInspector objectInspector && objectInspector.target is GraphElement)
                Selection.activeObject = null;
            Clear();
        }

        protected virtual void OnGUI()
        {
            if (this.GraphView != null && this.GraphView.Context != null)
                this.GraphView.Context.FrameEnd();
        }

        public override void SaveChanges()
        {
            this.Save();
        }

        #endregion

        #region Private Methods

        protected virtual void InitRootVisualElement()
        {
            GraphProcessorEditorStyles.DefaultStyles.GraphWindowTree.CloneTree(rootVisualElement);
            rootVisualElement.name = "rootVisualContainer";
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDownCallback, TrickleDown.TrickleDown);
            rootVisualElement.styleSheets.Add(GraphProcessorEditorStyles.DefaultStyles.BasicStyle);

            toolbarLeft = rootVisualElement.Q<Toolbar>("ToolbarLeft", "unity-toolbar");
            toolbarCenter = rootVisualElement.Q<Toolbar>("ToolbarCenter", "unity-toolbar");
            toolbarRight = rootVisualElement.Q<Toolbar>("ToolbarRight", "unity-toolbar");
            graphViewContainer = rootVisualElement.Q("GraphViewContainer");
            inspectorView = rootVisualElement.Q("InspectorView");
        }

        protected virtual void BeforeLoad(BaseGraphProcessor graph, IGraphOwner graphOwner, IGraphAsset graphAsset)
        {
        }

        protected void Load(BaseGraphProcessor graph, IGraphOwner graphOwner, IGraphAsset graphAsset)
        {
            Clear();

            BeforeLoad(graph, graphOwner, graphAsset);

            var historyLimit = Mathf.Max(50, GraphProcessorEditorSettings.CommandHistoryLimit.Value);
            this.context = new GraphViewContext() { graphWindow = this, commandDispatcher = new CommandDispatcher(historyLimit) };
            this.graphProcessor = graph;
            this.unityGraphOwner = graphOwner as UnityObject;
            this.graphAsset = graphAsset;
            this.unityGraphAsset = graphAsset as UnityObject;

            if (unityGraphAsset)
            {
                if (this.unityGraphAssetSO?.targetObject != unityGraphAsset)
                    this.unityGraphAssetSO = new SerializedObject(unityGraphAsset);
            }
            else
            {
                this.unityGraphAssetSO = null;
            }

            this.graphView = NewGraphView();
            this.graphView.SetUp(GraphProcessor, this.context);
            this.graphView.Init();
            this.GraphViewContainer.Add(graphView);

            BuildToolBar();

            GraphProcessorEditorSettings.MiniMapActive.onValueChanged += OnMiniMapActiveChanged;
            OnMiniMapActiveChanged(GraphProcessorEditorSettings.MiniMapActive.Value);
            GraphProcessorEditorSettings.GridSnapActive.onValueChanged += OnGridSnapActiveChanged;
            OnGridSnapActiveChanged(GraphProcessorEditorSettings.GridSnapActive.Value);

            AfterLoad();
        }

        protected virtual void AfterLoad()
        {
            RefreshBreadcrumbs();
            BuildInspectorPanel();
            BuildItemLibraryPanel();
            GraphView.graphViewChanged += OnGraphViewChangedRefreshInspector;
            GraphView.RegisterCallback<MouseUpEvent>(OnGraphMouseUpRefreshInspector);
            RefreshInspectorPanel();
            RefreshItemLibraryPanel();
        }

        protected void OnMiniMapActiveChanged(bool newValue)
        {
            graphView.MiniMapActive = newValue;
        }

        protected void OnGridSnapActiveChanged(bool newValue)
        {
            if (togSnap != null)
                togSnap.text = newValue ? "Snap:On" : "Snap:Off";
        }

        #endregion

        #region Public Methods

        public virtual void Clear()
        {
            if (graphView != null)
            {
                graphView.graphViewChanged -= OnGraphViewChangedRefreshInspector;
                graphView.UnregisterCallback<MouseUpEvent>(OnGraphMouseUpRefreshInspector);
            }

            ToolbarLeft.Clear();
            ToolbarCenter.Clear();
            ToolbarRight.Clear();
            GraphViewContainer.Clear();

            graphProcessor = null;
            graphView = null;
            unityGraphOwner = null;
            graphAsset = null;
            unityGraphAsset = null;
            context = null;

            this.rootVisualElement.Unbind();

            GraphProcessorEditorSettings.MiniMapActive.onValueChanged -= OnMiniMapActiveChanged;
            GraphProcessorEditorSettings.GridSnapActive.onValueChanged -= OnGridSnapActiveChanged;
            GraphProcessorEditorSettings.InspectorActive.onValueChanged -= OnInspectorActiveChanged;
            GraphProcessorEditorSettings.ItemLibraryActive.onValueChanged -= OnItemLibraryActiveChanged;
            togSnap = null;
            togInspector = null;
            togItemLibrary = null;
            breadcrumbBackButton = null;
            itemLibraryEntries = null;
            itemLibraryFiltered = null;
            blackboardFoldout = null;
            blackboardKeyField = null;
            blackboardValueField = null;
            blackboardSetButton = null;
            blackboardRemoveButton = null;
            blackboardList = null;
            blackboardEntries.Clear();
            breadcrumbContainer = null;
            breadcrumbPath = null;
            diagnosticBadge = null;

            this.SetHasUnsavedChanges(false);
        }

        // 重新加载Graph
        public virtual void Reload()
        {
            subgraphBreadcrumbStack.Clear();
            if (unityGraphOwner is IGraphAssetOwner graphAssetOwner && graphAssetOwner.GraphAsset != null)
            {
                LoadFromGraphAssetOwner(graphAssetOwner);
            }
            else if (graphAsset is IGraphOwner graphOwner)
            {
                LoadFromGraphOwner(graphOwner);
            }
            else if (graphAsset != null)
            {
                LoadFromGraphAsset(graphAsset);
            }
            else if (GraphProcessor is BaseGraphProcessor graphVM)
            {
                LoadFromGraphVM(graphVM);
            }
            else if (this.unityGraphAsset != null)
            {
                AssetDatabase.OpenAsset(this.unityGraphAsset);
            }
        }

        // 从GraphOwner加载
        public void LoadFromGraphOwner(IGraphOwner graphOwner)
        {
            Load(graphOwner.Graph, graphOwner, (IGraphAsset)graphOwner);
        }

        // 从GraphAssetOwner加载
        public void LoadFromGraphAssetOwner(IGraphAssetOwner graphAssetOwner)
        {
            Load(graphAssetOwner.Graph, graphAssetOwner, graphAssetOwner.GraphAsset);
        }

        // 从Graph资源加载
        public void LoadFromGraphAsset(IGraphAsset graphAsset)
        {
            Load(ViewModelFactory.ProduceViewModel(Atom.GraphProcessor.Editors.GraphProcessorEditorUtil.Clone(graphAsset.LoadGraph())) as BaseGraphProcessor, null, graphAsset);
        }

        // 直接加载GraphVM对象
        public void LoadFromGraphVM(BaseGraphProcessor graph)
        {
            Load(graph, null, null);
        }

        // 直接加载Graph对象
        public void LoadFromGraph(BaseGraph graph)
        {
            Load(ViewModelFactory.ProduceViewModel(graph) as BaseGraphProcessor, null, null);
        }

        public void SetHasUnsavedChanges(bool value)
        {
            this.hasUnsavedChanges = value;
        }

        public virtual void OpenSubgraph(IGraphAsset subgraphAsset)
        {
            if (subgraphAsset == null)
                return;

            if (graphAsset != null)
                subgraphBreadcrumbStack.Add(graphAsset);

            LoadFromGraphAsset(subgraphAsset);
        }

        public virtual bool TryGoBackFromSubgraph()
        {
            if (subgraphBreadcrumbStack.Count == 0)
                return false;

            var parent = subgraphBreadcrumbStack[subgraphBreadcrumbStack.Count - 1];
            subgraphBreadcrumbStack.RemoveAt(subgraphBreadcrumbStack.Count - 1);
            LoadFromGraphAsset(parent);
            return true;
        }

        #endregion

        #region Callbacks

        protected virtual void Save()
        {
            GraphAsset?.SaveGraph(Atom.GraphProcessor.Editors.GraphProcessorEditorUtil.Clone(GraphProcessor.Model));
            if (UnityGraphAsset)
                EditorUtility.SetDirty(UnityGraphAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            this.hasUnsavedChanges = false;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    Reload();
                    break;
            }
        }

        #endregion

        #region Overrides

        protected virtual BaseGraphView NewGraphView()
        {
            return new DefaultGraphView();
        }

        protected virtual void OnKeyDownCallback(KeyDownEvent evt)
        {
            if (evt.commandKey || evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.Z:
                        context.commandDispatcher.Undo();
                        evt.StopPropagation();
                        break;
                    case KeyCode.Y:
                        context.commandDispatcher.Redo();
                        evt.StopPropagation();
                        break;
                    case KeyCode.S:
                    {
                        Save();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.C:
                    {
                        GraphView?.CopySelectionToClipboard();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.X:
                    {
                        GraphView?.CutSelectionToClipboard();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.V:
                    {
                        GraphView?.PasteClipboard();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.D:
                    {
                        GraphView?.DuplicateSelection();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.Alpha1:
                    {
                        GraphView?.AlignSelectionLeft();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.Alpha2:
                    {
                        GraphView?.AlignSelectionRight();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.Alpha3:
                    {
                        GraphView?.AlignSelectionTop();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.Alpha4:
                    {
                        GraphView?.AlignSelectionBottom();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.Alpha5:
                    {
                        GraphView?.DistributeSelectionHorizontal();
                        evt.StopImmediatePropagation();
                        break;
                    }
                    case KeyCode.Alpha6:
                    {
                        GraphView?.DistributeSelectionVertical();
                        evt.StopImmediatePropagation();
                        break;
                    }
                }
            }
        }

        protected virtual void BuildToolBar()
        {
            var btnOverview = new ToolbarButton()
            {
                name = "btnOverview",
                text = "Overview",
                tooltip = "查看所有节点",
            };
            btnOverview.clicked += () => { GraphView.FrameAll(); };
            ToolbarLeft.Add(btnOverview);

            var togMiniMap = new ToolbarButton()
            {
                name = "togMiniMap",
                text = "MiniMap",
                tooltip = "小地图",
            };
            togMiniMap.clicked += () => { GraphProcessorEditorSettings.MiniMapActive.Value = !GraphProcessorEditorSettings.MiniMapActive.Value; };
            ToolbarLeft.Add(togMiniMap);

            togSnap = new ToolbarButton()
            {
                name = "togSnap",
                text = GraphProcessorEditorSettings.GridSnapActive.Value ? "Snap:On" : "Snap:Off",
                tooltip = "网格吸附",
            };
            togSnap.clicked += () =>
            {
                GraphProcessorEditorSettings.GridSnapActive.Value = !GraphProcessorEditorSettings.GridSnapActive.Value;
            };
            ToolbarLeft.Add(togSnap);

            togInspector = new ToolbarButton()
            {
                name = "togInspector",
                text = GraphProcessorEditorSettings.InspectorActive.Value ? "Inspector:On" : "Inspector:Off",
                tooltip = "图检查器",
            };
            togInspector.clicked += () => { GraphProcessorEditorSettings.InspectorActive.Value = !GraphProcessorEditorSettings.InspectorActive.Value; };
            ToolbarLeft.Add(togInspector);

            togItemLibrary = new ToolbarButton()
            {
                name = "togItemLibrary",
                text = GraphProcessorEditorSettings.ItemLibraryActive.Value ? "Library:On" : "Library:Off",
                tooltip = "图项库",
            };
            togItemLibrary.clicked += () => { GraphProcessorEditorSettings.ItemLibraryActive.Value = !GraphProcessorEditorSettings.ItemLibraryActive.Value; };
            ToolbarLeft.Add(togItemLibrary);

            if (graphAsset != null && graphAsset is UnityObject)
            {
                IMGUIContainer drawName = new IMGUIContainer(() =>
                {
                    GUILayout.BeginHorizontal();
                    if (unityGraphOwner != null)
                    {
                        EditorGUILayout.ObjectField(unityGraphOwner, typeof(UnityObject), true, GUILayout.Height(25));
                    }

                    if (unityGraphAsset != null)
                    {
                        EditorGUILayout.ObjectField(unityGraphAsset, typeof(UnityObject), true, GUILayout.Height(25));
                    }

                    GUILayout.Space(2);
                    GUILayout.EndHorizontal();
                });
                drawName.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                drawName.style.flexGrow = 1;
                ToolbarCenter.Add(drawName);
            }

            BuildBreadcrumbs();

            var btnReload = new ToolbarButton()
            {
                name = "btnReload",
                text = "Reload",
                tooltip = "重新加载",
                style =
                {
                    width = 80,
                    // backgroundImage = EditorGUIUtility.FindTexture("Refresh"),
                }
            };
            btnReload.clicked += () => { Reload(); };
            ToolbarRight.Add(btnReload);

            var btnSave = new ToolbarButton()
            {
                name = "btnSave",
                text = "Save",
                tooltip = "保存",
                style =
                {
                    width = 80,
                    // backgroundImage = EditorGUIUtility.FindTexture("SaveActive"),
                }
            };
            btnSave.clicked += Save;
            ToolbarRight.Add(btnSave);

            GraphProcessorEditorSettings.InspectorActive.onValueChanged += OnInspectorActiveChanged;
            GraphProcessorEditorSettings.ItemLibraryActive.onValueChanged += OnItemLibraryActiveChanged;
            OnInspectorActiveChanged(GraphProcessorEditorSettings.InspectorActive.Value);
            OnItemLibraryActiveChanged(GraphProcessorEditorSettings.ItemLibraryActive.Value);
        }

        private void BuildBreadcrumbs()
        {
            breadcrumbContainer = new VisualElement() { name = "Breadcrumbs" };
            breadcrumbContainer.style.flexDirection = FlexDirection.Row;
            breadcrumbContainer.style.alignItems = Align.Center;
            breadcrumbContainer.style.flexGrow = 1;
            breadcrumbContainer.style.paddingLeft = 6;
            breadcrumbContainer.style.paddingRight = 6;

            breadcrumbPath = new Label();
            breadcrumbPath.style.unityTextAlign = TextAnchor.MiddleLeft;
            breadcrumbPath.style.flexGrow = 1;
            breadcrumbPath.style.color = new Color(0.85f, 0.9f, 0.98f, 0.95f);

            diagnosticBadge = new Label();
            diagnosticBadge.style.marginLeft = 6;
            diagnosticBadge.style.paddingLeft = 6;
            diagnosticBadge.style.paddingRight = 6;
            diagnosticBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            diagnosticBadge.style.backgroundColor = new Color(0.55f, 0.2f, 0.15f, 0.75f);
            diagnosticBadge.style.color = new Color(1f, 0.88f, 0.84f, 1f);
            diagnosticBadge.style.display = DisplayStyle.None;

            var btnRoot = new ToolbarButton()
            {
                text = "Root",
                tooltip = "返回根图",
            };
            btnRoot.style.width = 56;
            btnRoot.clicked += () =>
            {
                IGraphAsset rootAsset = null;
                if (subgraphBreadcrumbStack.Count > 0)
                    rootAsset = subgraphBreadcrumbStack[0];

                subgraphBreadcrumbStack.Clear();
                if (unityGraphOwner is IGraphAssetOwner graphAssetOwner && graphAssetOwner.GraphAsset != null)
                    LoadFromGraphAssetOwner(graphAssetOwner);
                else if (rootAsset != null)
                    LoadFromGraphAsset(rootAsset);
                else if (graphAsset != null)
                    LoadFromGraphAsset(graphAsset);
            };

            var btnBack = new ToolbarButton()
            {
                text = "Back",
                tooltip = "返回父图",
            };
            btnBack.style.width = 56;
            btnBack.clicked += () => { TryGoBackFromSubgraph(); };
            breadcrumbBackButton = btnBack;

            breadcrumbContainer.Add(btnRoot);
            breadcrumbContainer.Add(btnBack);
            breadcrumbContainer.Add(breadcrumbPath);
            breadcrumbContainer.Add(diagnosticBadge);
            ToolbarCenter.Add(breadcrumbContainer);
        }

        private void RefreshBreadcrumbs()
        {
            if (breadcrumbPath == null)
                return;

            var ownerName = unityGraphOwner != null ? unityGraphOwner.name : "NoOwner";
            var assetName = unityGraphAsset != null ? unityGraphAsset.name : (GraphProcessor?.ModelType?.Name ?? "Graph");
            if (subgraphBreadcrumbStack.Count == 0)
            {
                breadcrumbPath.text = $"{ownerName} / {assetName}";
            }
            else
            {
                var path = ownerName;
                for (var i = 0; i < subgraphBreadcrumbStack.Count; i++)
                {
                    path += " / " + (subgraphBreadcrumbStack[i] as UnityObject)?.name;
                }
                path += " / " + assetName;
                breadcrumbPath.text = path;
            }

            var diagnosticCount = GraphProcessor?.Diagnostics?.Count ?? 0;
            diagnosticBadge.text = $"Warnings: {diagnosticCount}";
            diagnosticBadge.style.display = diagnosticCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            breadcrumbBackButton?.SetEnabled(subgraphBreadcrumbStack.Count > 0);
        }

        private void OnInspectorActiveChanged(bool active)
        {
            if (togInspector != null)
                togInspector.text = active ? "Inspector:On" : "Inspector:Off";
            RefreshInspectorPanel();
        }

        private void OnItemLibraryActiveChanged(bool active)
        {
            if (togItemLibrary != null)
                togItemLibrary.text = active ? "Library:On" : "Library:Off";
            RefreshInspectorPanel();
            RefreshItemLibraryPanel();
        }

        private GraphViewChange OnGraphViewChangedRefreshInspector(GraphViewChange changes)
        {
            RefreshInspectorPanel();
            return changes;
        }

        private void OnGraphMouseUpRefreshInspector(MouseUpEvent evt)
        {
            RefreshInspectorPanel();
        }

        private void BuildInspectorPanel()
        {
            if (InspectorView == null)
                return;

            InspectorView.Clear();
            inspectorHeader = new VisualElement() { name = "InspectorHeader" };
            inspectorHeader.style.paddingLeft = 10;
            inspectorHeader.style.paddingRight = 10;
            inspectorHeader.style.paddingTop = 8;
            inspectorHeader.style.paddingBottom = 8;
            inspectorHeader.style.borderBottomWidth = 1;
            inspectorHeader.style.borderBottomColor = new Color(0.4f, 0.5f, 0.65f, 0.2f);

            inspectorTitle = new Label("Graph Inspector");
            inspectorTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            inspectorHint = new Label();
            inspectorHint.style.fontSize = 11;
            inspectorHint.style.color = new Color(0.7f, 0.78f, 0.9f, 0.9f);
            inspectorHeader.Add(inspectorTitle);
            inspectorHeader.Add(inspectorHint);

            inspectorBody = new VisualElement() { name = "InspectorBody" };
            inspectorBody.style.flexGrow = 1;
            inspectorBody.style.paddingLeft = 10;
            inspectorBody.style.paddingRight = 10;
            inspectorBody.style.paddingTop = 8;

            itemLibraryBody = new VisualElement() { name = "ItemLibraryBody" };
            itemLibraryBody.style.flexGrow = 1;
            itemLibraryBody.style.paddingLeft = 10;
            itemLibraryBody.style.paddingRight = 10;
            itemLibraryBody.style.paddingTop = 8;

            var libraryTitle = new Label("Graph Item Library");
            libraryTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            itemLibraryBody.Add(libraryTitle);

            itemLibrarySearch = new TextField() { name = "ItemLibrarySearch", value = string.Empty };
            itemLibrarySearch.label = "Search";
            itemLibrarySearch.RegisterValueChangedCallback(_ => RefreshItemLibraryPanel());
            itemLibraryBody.Add(itemLibrarySearch);

            itemLibraryList = new ListView();
            itemLibraryList.style.flexGrow = 1;
            itemLibraryList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            itemLibraryList.selectionType = SelectionType.Single;
            itemLibraryList.makeItem = () => new Label();
            itemLibraryList.bindItem = (element, index) =>
            {
                if (element is Label label && itemLibraryFiltered != null && index >= 0 && index < itemLibraryFiltered.Count)
                    label.text = itemLibraryFiltered[index].Path;
            };
            itemLibraryList.itemsChosen += _ =>
            {
                if (itemLibraryList.selectedIndex < 0 || itemLibraryFiltered == null || itemLibraryList.selectedIndex >= itemLibraryFiltered.Count)
                    return;
                GraphView?.CreateNodeFromLibraryEntry(itemLibraryFiltered[itemLibraryList.selectedIndex]);
            };
            itemLibraryBody.Add(itemLibraryList);

            blackboardFoldout = new Foldout() { text = "Blackboard (Local Variables)", value = false };
            blackboardFoldout.style.marginTop = 8;

            blackboardKeyField = new TextField() { label = "Key" };
            blackboardValueField = new TextField() { label = "Value" };

            var blackboardButtonsRow = new VisualElement();
            blackboardButtonsRow.style.flexDirection = FlexDirection.Row;
            blackboardButtonsRow.style.marginTop = 2;
            blackboardButtonsRow.style.marginBottom = 4;

            blackboardSetButton = new Button(OnSetBlackboardClicked) { text = "Set" };
            blackboardSetButton.style.width = 64;
            blackboardRemoveButton = new Button(OnRemoveBlackboardClicked) { text = "Remove" };
            blackboardRemoveButton.style.width = 72;
            blackboardRemoveButton.style.marginLeft = 6;

            blackboardButtonsRow.Add(blackboardSetButton);
            blackboardButtonsRow.Add(blackboardRemoveButton);

            blackboardList = new ListView();
            blackboardList.style.flexGrow = 1;
            blackboardList.style.height = 120;
            blackboardList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            blackboardList.selectionType = SelectionType.Single;
            blackboardList.makeItem = () => new Label();
            blackboardList.bindItem = (element, index) =>
            {
                if (!(element is Label label) || index < 0 || index >= blackboardEntries.Count)
                    return;

                var pair = blackboardEntries[index];
                label.text = $"{pair.Key} = {pair.Value}";
            };
            blackboardList.onSelectionChange += _ =>
            {
                var idx = blackboardList.selectedIndex;
                if (idx < 0 || idx >= blackboardEntries.Count)
                    return;
                blackboardKeyField.value = blackboardEntries[idx].Key;
                blackboardValueField.value = blackboardEntries[idx].Value?.ToString() ?? string.Empty;
            };

            blackboardFoldout.Add(blackboardKeyField);
            blackboardFoldout.Add(blackboardValueField);
            blackboardFoldout.Add(blackboardButtonsRow);
            blackboardFoldout.Add(blackboardList);
            inspectorBody.Add(blackboardFoldout);

            InspectorView.Add(inspectorHeader);
            InspectorView.Add(inspectorBody);
            InspectorView.Add(itemLibraryBody);
        }

        private void RefreshInspectorPanel()
        {
            if (InspectorView == null || inspectorBody == null || GraphView == null)
                return;

            var showInspector = GraphProcessorEditorSettings.InspectorActive.Value;
            var showLibrary = GraphProcessorEditorSettings.ItemLibraryActive.Value;
            InspectorView.style.display = (showInspector || showLibrary) ? DisplayStyle.Flex : DisplayStyle.None;
            inspectorHeader.style.display = showInspector ? DisplayStyle.Flex : DisplayStyle.None;
            inspectorBody.style.display = showInspector ? DisplayStyle.Flex : DisplayStyle.None;

            if (!showInspector)
                return;

            inspectorBody.Clear();
            var selectedCount = GraphView.selection.Count;
            inspectorHint.text = selectedCount == 0 ? "Graph level properties" : $"Selected elements: {selectedCount}";

            var diagnostics = GraphProcessor?.Diagnostics;
            if (diagnostics != null && diagnostics.Count > 0)
            {
                var diagnosticsFoldout = new Foldout() { text = $"Missing Item / Error ({diagnostics.Count})", value = true };
                diagnosticsFoldout.style.marginBottom = 6;
                for (var i = 0; i < diagnostics.Count; i++)
                {
                    var msg = new Label($"- {diagnostics[i]}");
                    msg.style.whiteSpace = WhiteSpace.Normal;
                    msg.style.fontSize = 10;
                    msg.style.color = new Color(1f, 0.72f, 0.68f, 0.95f);
                    diagnosticsFoldout.Add(msg);
                }
                inspectorBody.Add(diagnosticsFoldout);
            }

            if (selectedCount == 0)
            {
                var graphInfo = new Label($"Nodes: {GraphProcessor?.Model?.nodes?.Count ?? 0}\nConnections: {GraphProcessor?.Model?.connections?.Count ?? 0}\nGroups: {GraphProcessor?.Model?.groups?.Count ?? 0}\nNotes: {GraphProcessor?.Model?.notes?.Count ?? 0}");
                graphInfo.text += $"\nPlacemats: {GraphProcessor?.Model?.placemats?.Count ?? 0}";
                graphInfo.style.whiteSpace = WhiteSpace.Normal;
                graphInfo.style.color = new Color(0.82f, 0.86f, 0.95f, 0.95f);
                inspectorBody.Add(graphInfo);
                return;
            }

            var subgraphAssets = CollectSelectedSubgraphAssets();
            if (subgraphAssets.Count > 0)
            {
                var sectionTitle = new Label("Subgraph Navigation");
                sectionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                sectionTitle.style.marginTop = 6;
                sectionTitle.style.marginBottom = 4;
                inspectorBody.Add(sectionTitle);

                for (var i = 0; i < subgraphAssets.Count; i++)
                {
                    var subgraphAsset = subgraphAssets[i];
                    var btnOpenSubgraph = new Button(() => OpenSubgraph(subgraphAsset));
                    btnOpenSubgraph.text = $"Open {(subgraphAsset as UnityObject)?.name ?? subgraphAsset.GraphType.Name}";
                    btnOpenSubgraph.style.marginBottom = 2;
                    inspectorBody.Add(btnOpenSubgraph);
                }
            }

            foreach (var item in GraphView.selection)
            {
                var title = new Label(item.GetType().Name);
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.marginTop = 6;
                inspectorBody.Add(title);
                if (item is BaseNodeView node)
                    inspectorBody.Add(new Label($"{node.ViewModel.Title} ({node.ViewModel.ModelType.Name})"));
                else if (item is GroupView group)
                    inspectorBody.Add(new Label(group.ViewModel.GroupName));
                else if (item is StickyNoteView note)
                    inspectorBody.Add(new Label(note.ViewModel.Title));
                else if (item is PlacematView placemat)
                    inspectorBody.Add(new Label($"{placemat.ViewModel.Title} [{placemat.ViewModel.Size.x}x{placemat.ViewModel.Size.y}]"));
                else if (item is BaseConnectionView connection)
                    inspectorBody.Add(new Label($"{connection.ViewModel.FromNodeID}:{connection.ViewModel.FromPortName} -> {connection.ViewModel.ToNodeID}:{connection.ViewModel.ToPortName}"));
            }

            RefreshBlackboardList();
        }

        private void OnSetBlackboardClicked()
        {
            if (GraphProcessor?.Blackboard == null)
                return;

            var key = blackboardKeyField?.value?.Trim();
            if (string.IsNullOrEmpty(key))
                return;

            var oldExists = GraphProcessor.Blackboard.TryGet(key, out object oldValue);
            var newValue = blackboardValueField?.value ?? string.Empty;
            if (oldExists && Equals(oldValue, newValue))
                return;

            context?.Do(() =>
            {
                GraphProcessor.Blackboard.Set(key, newValue);
            }, () =>
            {
                if (oldExists)
                    GraphProcessor.Blackboard.Set(key, oldValue);
                else
                    GraphProcessor.Blackboard.Remove(key);
            });

            RefreshBlackboardList();
            RefreshInspectorPanel();
        }

        private void OnRemoveBlackboardClicked()
        {
            if (GraphProcessor?.Blackboard == null)
                return;

            var key = blackboardKeyField?.value?.Trim();
            if (string.IsNullOrEmpty(key))
                return;

            if (!GraphProcessor.Blackboard.TryGet(key, out object oldValue))
                return;

            context?.Do(() =>
            {
                GraphProcessor.Blackboard.Remove(key);
            }, () =>
            {
                GraphProcessor.Blackboard.Set(key, oldValue);
            });

            RefreshBlackboardList();
            RefreshInspectorPanel();
        }

        private void RefreshBlackboardList()
        {
            if (blackboardList == null)
                return;

            blackboardEntries.Clear();
            var blackboardRaw = GraphProcessor?.Blackboard?.Blackboard;
            if (blackboardRaw != null)
            {
                foreach (var pair in blackboardRaw.EnumerateValues())
                {
                    blackboardEntries.Add(new KeyValuePair<string, object>(pair.Key, pair.Value));
                }

                blackboardEntries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            }

            blackboardList.itemsSource = blackboardEntries;
            blackboardList.Rebuild();
        }

        private void BuildItemLibraryPanel()
        {
            itemLibraryEntries = GraphView?.CollectNodeMenuEntries() ?? new List<NodeMenuWindow.INodeEntry>();
            itemLibraryFiltered = new List<NodeMenuWindow.INodeEntry>(itemLibraryEntries);
            if (itemLibraryList != null)
            {
                itemLibraryList.itemsSource = itemLibraryFiltered;
                itemLibraryList.Rebuild();
            }
        }

        private void RefreshItemLibraryPanel()
        {
            if (itemLibraryBody == null)
                return;

            var showLibrary = GraphProcessorEditorSettings.ItemLibraryActive.Value;
            itemLibraryBody.style.display = showLibrary ? DisplayStyle.Flex : DisplayStyle.None;
            if (!showLibrary)
                return;

            if (itemLibraryEntries == null)
                BuildItemLibraryPanel();

            itemLibraryFiltered.Clear();
            var query = itemLibrarySearch?.value;
            if (string.IsNullOrWhiteSpace(query))
            {
                itemLibraryFiltered.AddRange(itemLibraryEntries);
            }
            else
            {
                query = query.Trim();
                for (var i = 0; i < itemLibraryEntries.Count; i++)
                {
                    var entry = itemLibraryEntries[i];
                    if (entry.Path.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        itemLibraryFiltered.Add(entry);
                }
            }

            itemLibraryList.itemsSource = itemLibraryFiltered;
            itemLibraryList.Rebuild();

            RefreshBreadcrumbs();
        }

        private List<IGraphAsset> CollectSelectedSubgraphAssets()
        {
            var result = new List<IGraphAsset>(4);
            if (GraphView == null)
                return result;

            var set = new HashSet<IGraphAsset>();
            foreach (var item in GraphView.selection)
            {
                if (!(item is BaseNodeView nodeView))
                    continue;

                var model = nodeView.ViewModel?.Model;
                if (model == null)
                    continue;

                var modelType = model.GetType();
                var fields = modelType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (!typeof(UnityObject).IsAssignableFrom(field.FieldType))
                        continue;

                    var value = field.GetValue(model);
                    if (value is IGraphAsset graphAsset && set.Add(graphAsset))
                        result.Add(graphAsset);
                }
            }

            return result;
        }

        #endregion

        #region Static

        /// <summary> 从Graph类型获取对应的GraphWindow </summary>
        public static BaseGraphWindow GetGraphWindow(Type graphType)
        {
            var windowType = GraphProcessorEditorUtil.GetViewType(graphType);
            var windows = Resources.FindObjectsOfTypeAll(windowType);
            BaseGraphWindow window = null;
            foreach (var _window in windows)
            {
                if (_window.GetType() == windowType)
                {
                    window = _window as BaseGraphWindow;
                    break;
                }
            }

            if (window == null)
            {
                window = GetWindow(windowType) as BaseGraphWindow;
            }

            window.Focus();
            return window;
        }

        /// <summary> 从GraphOwner打开Graph </summary>
        public static BaseGraphWindow Open(IGraphOwner graphOwner)
        {
            var window = GetGraphWindow(graphOwner.Graph.ModelType);
            window.LoadFromGraphOwner(graphOwner);
            return window;
        }

        /// <summary> 从GraphAssetOwner打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAssetOwner graphAssetOwner)
        {
            var window = GetGraphWindow(graphAssetOwner.Graph.ModelType);
            window.LoadFromGraphAssetOwner(graphAssetOwner);
            return window;
        }

        /// <summary> 从GraphAsset打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAsset graphAsset)
        {
            var window = GetGraphWindow(graphAsset.GraphType);
            window.LoadFromGraphAsset(graphAsset);
            return window;
        }

        /// <summary> 打开Graph </summary>
        public static BaseGraphWindow Open(BaseGraphProcessor graph)
        {
            var window = GetGraphWindow(graph.ModelType);
            window.LoadFromGraphVM(graph);
            return window;
        }

        /// <summary> 打开Graph </summary>
        public static BaseGraphWindow Open(BaseGraph graph)
        {
            var window = GetGraphWindow(graph.GetType());
            window.LoadFromGraph(graph);
            return window;
        }

        /// <summary> 双击资源 </summary>
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            UnityObject go = EditorUtility.InstanceIDToObject(instanceID);
            if (go == null)
                return false;
            IGraphAsset graphAsset = go as IGraphAsset;
            if (graphAsset == null)
                return false;
            if (Selection.activeGameObject != null)
            {
                var owner = Selection.activeGameObject.GetComponent<IGraphAssetOwner>();
                if (owner != null && owner.GraphAsset == graphAsset)
                    Open(owner);
                else
                    Open(graphAsset);
            }
            else
                Open(graphAsset);

            return true;
        }

        #endregion
    }
}
#endif
