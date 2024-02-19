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
using CZToolKit;
using System;
using CZToolKitEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class BaseGraphWindow : BaseEditorWindow
    {
        #region Fields

        private VisualElement graphViewContainer;
        private Toolbar toolbarLeft;
        private Toolbar toolbarCenter;
        private Toolbar toolbarRight;


        [SerializeField] private UnityObject unityGraphOwner;
        [SerializeField] private UnityObject unityGraphAsset;
        private IGraphOwner graphOwner;
        private IGraphAsset graphAsset;
        private BaseGraphProcessor graph;
        private BaseGraphView graphView;
        private CommandDispatcher commandDispatcher;

        #endregion

        #region Properties

        private VisualElement GraphViewContainer
        {
            get { return graphViewContainer; }
        }

        public Toolbar ToolbarLeft
        {
            get { return toolbarLeft; }
        }

        public Toolbar ToolbarCenter
        {
            get { return toolbarCenter; }
        }

        public Toolbar ToolbarRight
        {
            get { return toolbarRight; }
        }

        public IGraphOwner GraphOwner
        {
            get
            {
                if (graphOwner == null)
                    graphOwner = unityGraphOwner as IGraphOwner;
                return unityGraphOwner as IGraphOwner;
            }
            protected set
            {
                graphOwner = value;
                unityGraphOwner = value as UnityObject;
            }
        }


        public IGraphAsset GraphAsset
        {
            get { return graphAsset; }
            protected set
            {
                graphAsset = value;
                unityGraphAsset = graphAsset?.UnityAsset;
            }
        }

        public BaseGraphProcessor Graph
        {
            get { return graph; }
        }

        public BaseGraphView GraphView
        {
            get { return graphView; }
        }

        public CommandDispatcher CommandDispatcher
        {
            get { return commandDispatcher; }
        }

        #endregion

        #region Unity

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Graph Processor");
            InitRootVisualElement();

            Reload();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected virtual void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (Selection.activeObject is ObjectInspector objectInspector && objectInspector.Target is GraphElement)
                Selection.activeObject = null;
        }

        #endregion

        #region Private Methods

        protected virtual void InitRootVisualElement()
        {
            var tree = Resources.Load<VisualTreeAsset>(GraphProcessorStyles.GraphWindowUXMLFile);
            tree.CloneTree(rootVisualElement);
            rootVisualElement.name = "rootVisualContainer";
            rootVisualElement.styleSheets.Add(GraphProcessorStyles.BasicStyle);

            toolbarLeft = rootVisualElement.Q<Toolbar>("ToolbarLeft", "unity-toolbar");
            toolbarCenter = rootVisualElement.Q<Toolbar>("ToolbarCenter", "unity-toolbar");
            toolbarRight = rootVisualElement.Q<Toolbar>("ToolbarRight", "unity-toolbar");
            graphViewContainer = rootVisualElement.Q("GraphViewContainer");
        }

        protected virtual void BeforeLoad(BaseGraphProcessor graph, IGraphOwner graphOwner, IGraphAsset graphAsset)
        {
        }

        protected void Load(BaseGraphProcessor graph, IGraphOwner graphOwner, IGraphAsset graphAsset)
        {
            Clear();
            BeforeLoad(graph, graphOwner, graphAsset);

            this.commandDispatcher = new CommandDispatcher();
            this.graph = graph;
            this.GraphOwner = graphOwner;
            this.GraphAsset = graphAsset;

            this.graphView = NewGraphView();
            this.graphView.Init();
            this.graphViewContainer.Add(graphView);

            graphView.RegisterCallback<KeyDownEvent>(OnKeyDownCallback);
            BuildToolBar();
            AfterLoad();
        }

        protected virtual void AfterLoad()
        {
        }

        #endregion

        #region Public Methods

        public virtual void Clear()
        {
            SetGraphUndirty();

            ToolbarLeft.Clear();
            ToolbarCenter.Clear();
            ToolbarRight.Clear();
            GraphViewContainer.Clear();

            graph = null;
            graphView = null;
            GraphAsset = null;
            GraphOwner = null;
            commandDispatcher = null;
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
            else if (Graph is BaseGraphProcessor graphVM)
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
            Load(ViewModelFactory.CreateViewModel(graphAsset.DeserializeGraph()) as BaseGraphProcessor, null, graphAsset);
        }

        // 直接加载GraphVM对象
        public void LoadFromGraphVM(BaseGraphProcessor graph)
        {
            Load(graph, null, null);
        }

        // 直接加载Graph对象
        public void LoadFromGraph(BaseGraph graph)
        {
            Load(ViewModelFactory.CreateViewModel(ViewModelFactory.CreateViewModel(graph) as BaseGraphProcessor) as BaseGraphProcessor, null, null);
        }

        public void SetGraphDirty()
        {
            if (!titleContent.text.EndsWith(" *"))
                titleContent.text += " *";
            if (graphAsset != null && graphAsset.UnityAsset != null)
                EditorUtility.SetDirty(graphAsset.UnityAsset);
            if (GraphOwner is UnityObject uobj && uobj != null)
                EditorUtility.SetDirty(uobj);
        }

        public void SetGraphUndirty()
        {
            if (titleContent.text.EndsWith(" *"))
                titleContent.text = titleContent.text.Replace(" *", "");
        }

        #endregion

        #region Callbacks

        void OnPlayModeStateChanged(PlayModeStateChange obj)
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
            return new DefaultGraphView(Graph, this, new CommandDispatcher());
        }

        protected virtual void OnBtnSaveClick()
        {
            if (GraphAsset is IGraphAsset graphSerialization)
                graphSerialization.SaveGraph(Graph.Model);

            if (GraphAsset is UnityObject uo)
                EditorUtility.SetDirty(uo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GraphView.SetUnDirty();
        }

        protected virtual void OnKeyDownCallback(KeyDownEvent evt)
        {
            if (evt.commandKey || evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.S:
                        OnBtnSaveClick();
                        evt.StopImmediatePropagation();
                        break;
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

            var togMiniMap = new ToolbarToggle()
            {
                name = "togMiniMap",
                text = "MiniMap",
                tooltip = "小地图",
                value = EditorPrefs.GetBool("GraphView.MiniMap.Active", false),
            };
            togMiniMap.RegisterValueChangedCallback(_v =>
            {
                EditorPrefs.SetBool("GraphView.MiniMap.Active", _v.newValue);
                GraphView.MiniMapActive = _v.newValue;
            });
            ToolbarLeft.Add(togMiniMap);
            GraphView.MiniMapActive = togMiniMap.value;

            if (graphAsset.UnityAsset != null)
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
            btnSave.clicked += OnBtnSaveClick;
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