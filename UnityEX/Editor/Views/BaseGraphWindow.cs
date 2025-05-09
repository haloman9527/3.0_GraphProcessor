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

        [SerializeField] private UnityObject unityGraphOwner;
        [SerializeField] private UnityObject unityGraphAsset;
        private SerializedObject unityGraphAssetSO;
        private IGraphAsset graphAsset;
        private BaseGraphProcessor graphProcessor;
        private BaseGraphView graphView;
        private GraphViewContext context;

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

            this.context = new GraphViewContext() { graphWindow = this, commandDispatcher = new CommandDispatcher() };
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

            AfterLoad();
        }

        protected virtual void AfterLoad()
        {
        }

        protected void OnMiniMapActiveChanged(bool newValue)
        {
            graphView.MiniMapActive = newValue;
        }

        #endregion

        #region Public Methods

        public virtual void Clear()
        {
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

            this.SetHasUnsavedChanges(false);
        }

        // 重新加载Graph
        public virtual void Reload()
        {
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
            Load(ViewModelFactory.ProduceViewModel(graphAsset.LoadGraph().Clone()) as BaseGraphProcessor, null, graphAsset);
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

        #endregion

        #region Callbacks

        protected virtual void Save()
        {
            GraphAsset?.SaveGraph(GraphProcessor.Model.Clone());
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