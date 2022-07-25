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
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BaseGraph))]
    public class BaseGraphWindow : BasicEditorWindow
    {
        #region 字段
        [SerializeField] protected UnityObject graphOwner;
        [SerializeField] protected UnityObject graphAsset;
        #endregion

        #region Properties
        private VisualElement GraphViewContainer
        {
            get;
            set;
        }
        public Toolbar ToolbarLeft
        {
            get;
            private set;
        }
        public Toolbar ToolbarCenter
        {
            get;
            private set;
        }
        public Toolbar ToolbarRight
        {
            get;
            private set;
        }
        public IGraphOwner GraphOwner
        {
            get { return graphOwner as IGraphOwner; }
            protected set { graphOwner = value as UnityObject; }
        }
        public UnityObject GraphAsset
        {
            get { return graphAsset; }
            protected set { graphAsset = value; }
        }
        public BaseGraphVM Graph
        {
            get;
            private set;
        }
        public BaseGraphView GraphView
        {
            get;
            private set;
        }
        public CommandDispatcher CommandDispatcher
        {
            get;
            protected set;
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
        void InitRootVisualElement()
        {
            GraphProcessorStyles.GraphWindowTree.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(GraphProcessorStyles.BasicStyle);

            ToolbarLeft = rootVisualElement.Q<Toolbar>("ToolbarLeft", "unity-toolbar");
            ToolbarCenter = rootVisualElement.Q<Toolbar>("ToolbarCenter", "unity-toolbar");
            ToolbarRight = rootVisualElement.Q<Toolbar>("ToolbarRight", "unity-toolbar");
            GraphViewContainer = rootVisualElement.Q("GraphViewContainer");
        }
        #endregion

        #region Public Methods
        protected void Load(BaseGraphVM graph, CommandDispatcher commandDispatcher)
        {
            OnGraphViewUndirty();
            Graph = graph;
            GraphView = NewGraphView(Graph);

            GraphView.SetUp(Graph, this, commandDispatcher);
            GraphView.BindingProperties();
            GraphView.onDirty += OnGraphViewDirty;
            GraphView.onUndirty += OnGraphViewUndirty;
            GraphViewContainer.Add(GraphView);
            OnGraphLoaded();
        }

        public virtual void Clear()
        {
            ToolbarLeft.Clear();
            ToolbarCenter.Clear();
            ToolbarRight.Clear();
            GraphViewContainer.Clear();

            Graph = null;
            GraphView = null;
            GraphAsset = null;
            GraphOwner = null;
            CommandDispatcher = null;
        }

        // 重新加载Graph
        public virtual void Reload()
        {
            if (GraphOwner is IGraphAssetOwner graphAssetOwner && graphAssetOwner.GraphAsset != null)
            {
                ForceLoad(graphAssetOwner);
            }
            else if (GraphOwner is IGraphOwner graphOwner)
            {
                ForceLoad(graphOwner);
            }
            else if (GraphAsset is IGraphAsset graphAsset)
            {
                ForceLoad(graphAsset);
            }
            else if (Graph is BaseGraphVM graphVM)
            {
                ForceLoad(graphVM);
            }
        }

        // 从GraphOwner加载
        public void ForceLoad(IGraphOwner graphOwner)
        {
            Clear();
            GraphOwner = graphOwner;
            GraphAsset = (UnityObject)graphOwner;
            CommandDispatcher = new CommandDispatcher();

            Load(GraphOwner.Graph, CommandDispatcher);
        }

        // 从GraphAssetOwner加载
        public void ForceLoad(IGraphAssetOwner graphAssetOwner)
        {
            Clear();
            GraphOwner = graphAssetOwner;
            GraphAsset = graphAssetOwner.GraphAsset;
            CommandDispatcher = new CommandDispatcher();

            Load(GraphOwner.Graph, CommandDispatcher);
        }

        // 从Graph资源加载
        public void ForceLoad(IGraphAsset graphAsset)
        {
            var graphAssetOwner = Selection.activeGameObject?.GetComponent<IGraphAssetOwner>();
            if (graphAssetOwner != null && graphAssetOwner.GraphAsset == (UnityObject)graphAsset)
            {
                ForceLoad(graphAssetOwner);
                return;
            }
            Clear();

            GraphOwner = null;
            GraphAsset = graphAsset as UnityObject;
            CommandDispatcher = new CommandDispatcher();
            Load(GraphProcessorUtil.CreateViewModel(graphAsset.DeserializeGraph()) as BaseGraphVM, CommandDispatcher);
        }

        // 直接加载Graph对象
        public void ForceLoad(BaseGraphVM graph)
        {
            Clear();
            GraphAsset = null;
            GraphOwner = null;
            CommandDispatcher = new CommandDispatcher();

            Load(graph, CommandDispatcher);
        }

        // 直接加载Graph对象
        public void ForceLoad(BaseGraph graph)
        {
            Clear();
            GraphAsset = null;
            GraphOwner = null;
            CommandDispatcher = new CommandDispatcher();

            Load(GraphProcessorUtil.CreateViewModel(GraphProcessorUtil.CreateViewModel(graph) as BaseGraphVM) as BaseGraphVM, CommandDispatcher);
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

        void OnGraphViewDirty()
        {
            if (!titleContent.text.EndsWith(" *"))
                titleContent.text += " *";
            if (GraphAsset != null)
                EditorUtility.SetDirty(GraphAsset);
            if (GraphOwner is UnityObject uobj && uobj != null)
                EditorUtility.SetDirty(uobj);
        }

        void OnGraphViewUndirty()
        {
            if (titleContent.text.EndsWith(" *"))
                titleContent.text = titleContent.text.Replace(" *", "");
        }
        #endregion

        #region Overrides
        protected virtual BaseGraphView NewGraphView(BaseGraphVM graph)
        {
            return new BaseGraphView();
        }

        protected virtual void OnGraphLoaded()
        {
            ToolbarButton btnOverview = new ToolbarButton()
            {
                text = "Overview",
                tooltip = "查看所有节点"
            };
            btnOverview.clicked += () =>
            {
                GraphView.FrameAll();
            };
            btnOverview.style.width = 80;
            btnOverview.style.unityTextAlign = TextAnchor.MiddleCenter;
            ToolbarLeft.Add(btnOverview);

            IMGUIContainer drawName = new IMGUIContainer(() =>
            {
                GUILayout.BeginHorizontal();
                if (GraphAsset != null && GUILayout.Button(GraphAsset.name, EditorStyles.toolbarButton))
                    EditorGUIUtility.PingObject(GraphAsset);
                GUILayout.EndHorizontal();
            });
            drawName.style.flexGrow = 1;
            ToolbarCenter.Add(drawName);

            ToolbarButton btnReload = new ToolbarButton()
            {
                text = "Reload",
                tooltip = "重新加载",
                style = { width = 70 }
            };
            btnReload.clicked += Reload;
            btnReload.style.width = 80;
            btnReload.style.unityTextAlign = TextAnchor.MiddleCenter;
            ToolbarRight.Add(btnReload);
        }
        #endregion

        #region Static
        /// <summary> 从Graph类型获取对应的GraphWindow </summary>
        public static BaseGraphWindow GetGraphWindow(Type graphType)
        {
            var windowType = GraphProcessorEditorUtil.GetViewType(graphType);
            UnityObject[] objs = Resources.FindObjectsOfTypeAll(windowType);
            BaseGraphWindow window = null;
            foreach (var obj in objs)
            {
                if (obj.GetType() == windowType)
                {
                    window = obj as BaseGraphWindow;
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
            if (graphOwner == null) return null;
            if (graphOwner.Graph == null) return null;
            var window = GetGraphWindow(graphOwner.Graph.ModelType);
            window.ForceLoad(graphOwner);
            return window;
        }

        /// <summary> 从GraphAssetOwner打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAssetOwner graphAssetOwner)
        {
            if (graphAssetOwner == null) return null;
            if (graphAssetOwner.GraphAsset == null) return null;
            var window = GetGraphWindow(graphAssetOwner.Graph.ModelType);
            window.ForceLoad(graphAssetOwner);
            return window;
        }

        /// <summary> 从GraphAsset打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAsset graphAsset)
        {
            if (graphAsset == null) return null;
            var window = GetGraphWindow(graphAsset.GraphType);
            window.ForceLoad(graphAsset);
            return window;
        }

        /// <summary> 打开Graph </summary>
        public static BaseGraphWindow Open(BaseGraphVM graph)
        {
            if (graph == null) return null;
            var window = GetGraphWindow(graph.GetType());
            window.ForceLoad(graph);
            return window;
        }

        /// <summary> 打开Graph </summary>
        public static BaseGraphWindow Open(BaseGraph graph)
        {
            if (graph == null) return null;
            var window = GetGraphWindow(graph.GetType());
            window.ForceLoad(graph);
            return window;
        }

        /// <summary> 双击资源 </summary>
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            UnityObject go = EditorUtility.InstanceIDToObject(instanceID);
            if (go == null) return false;
            IGraphAsset graphAsset = go as IGraphAsset;
            if (graphAsset == null)
                return false;
            Open(graphAsset);
            return true;
        }
        #endregion
    }
}
#endif