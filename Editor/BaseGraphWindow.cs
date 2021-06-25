using CZToolKit.Core.Editors;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine.Profiling;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    [Serializable]
    public class BaseGraphWindow : BasicEditorWindow
    {
        #region 静态方法
        [OnOpenAsset(0)]
        static bool OnGraphAssetOpened(int _instanceID, int _line)
        {
            var graphAsset = EditorUtility.InstanceIDToObject(_instanceID) as BaseGraphAsset;

            if (graphAsset != null)
            {
                LoadGraphFromAsset(graphAsset);
                return true;
            }

            return false;
        }

        /// <summary> 从GraphOwner中加载 </summary>
        /// <param name="_graphAssetOwner"></param>
        /// <returns></returns>
        public static BaseGraphWindow LoadGraphFromOwner(GraphAssetOwner _graphAssetOwner)
        {
            _graphAssetOwner.Graph.InitializePropertyMapping(_graphAssetOwner);
            return LoadGraph(_graphAssetOwner.Graph);
        }

        /// <summary> 从GraphAsset中加载 </summary>
        /// <param name="_graphAsset"></param>
        /// <returns></returns>
        public static BaseGraphWindow LoadGraphFromAsset(IGraphAsset _graphAsset)
        {
            return LoadGraph(_graphAsset.Graph);
        }

        /// <summary> 加载Graph </summary>
        /// <param name="_graph"></param>
        /// <returns></returns>
        public static BaseGraphWindow LoadGraph(IGraph _graph)
        {
            if (_graph == null) return null;
            Type windowType = NodeEditorUtility.GetGraphWindowType(_graph.GetType());

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
                window = CreateInstance(windowType) as BaseGraphWindow;
                window.SetUp(_graph);
                window.Show();
            }
            else
            {
                window.Focus();
                if (window.Graph != _graph)
                    window.SetUp(_graph);
            }

            return window;
        }

        /// <summary> 根据Graph获取已打开的Window </summary>
        /// <param name="_graph"></param>
        /// <returns></returns>
        public static BaseGraphWindow GetWindow(BaseGraph _graph)
        {
            Type type = NodeEditorUtility.GetGraphWindowType(_graph.GetType());

            UnityObject[] objs = Resources.FindObjectsOfTypeAll(type);
            BaseGraphWindow window = null;
            foreach (var obj in objs)
            {
                if (obj.GetType() == type)
                {
                    window = obj as BaseGraphWindow;
                    if (window.Graph == _graph)
                        return window;
                }
            }
            return null;
        }
        #endregion

        #region 字段
        [SerializeField] int graphOwnerInstanceID;
        [SerializeField] IGraphOwner graphOwner;
        [SerializeField] UnityObject graphAsset;
        #endregion

        #region 属性
        public IGraphOwner GraphOwner
        {
            get { return graphOwner; }
            private set { graphOwner = value; if (graphOwner != null) graphOwnerInstanceID = graphOwner.GetInstanceID(); }
        }
        public UnityObject GraphAsset { get { return graphAsset; } private set { graphAsset = value; } }
        public IGraph Graph { get; private set; }
        public BaseGraphView GraphView { get; private set; }
        public GraphViewParentElement GraphViewParent { get; private set; }
        public CommandDispatcher CommandDispatcher { get; private set; }
        #endregion

        #region Unity
        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Graph Processor");

            CommandDispatcher = new CommandDispatcher(CreateGraphState());
            CommandDispatcherHelper.RegisterDefaultCommandHandlers(CommandDispatcher);

            GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as IGraphOwner;

            if (GraphView == null && GraphAsset != null)
                EditorApplication.delayCall += ReloadGraph;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        protected virtual void OnDisable()
        {
            if (GraphView != null && GraphAsset != null && EditorUtility.IsDirty(GraphAsset))
                GraphView.SaveGraphToDisk();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        protected override void Update()
        {
            base.Update();

            Profiler.BeginSample("CZToolKit.GraphProcessor");
            Stopwatch sw = new Stopwatch();

            CommandDispatcher.NotifyObservers();

            sw.Stop();
            Profiler.EndSample();
        }

        protected virtual void OnDestroy()
        {
            if (Selection.activeObject is ObjectInspector objectInspector
                && objectInspector.TargetObject is BaseNode node
                && node.Owner == Graph)
            {
                Selection.activeObject = null;
            }
        }
        #endregion

        #region 回调
        void OnPlayModeChanged(PlayModeStateChange _playMode)
        {
            switch (_playMode)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphAssetOwner;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 帮助方法
        public void Clear()
        {
            if (GraphViewParent != null)
            {
                GraphViewParent.RemoveFromHierarchy();
                GraphViewParent = null;
            }

            GraphOwner = null;
        }

        void SetUp(IGraph _graph)
        {
            if (GraphView != null && GraphAsset != null && EditorUtility.IsDirty(GraphAsset))
                GraphView.SaveGraphToDisk();
            Clear();

            Graph = _graph;
            GraphAsset = (Graph as IGraphFromAsset)?.Asset;
            if (GraphAsset == null) return;

            GraphViewParent = new GraphViewParentElement();
            GraphViewParent.StretchToParentSize();
            rootVisualElement.Add(GraphViewParent);

            GraphView = GenerateGraphView(Graph);
            if (GraphView == null) return;

            ToolbarButton btnPing = new ToolbarButton()
            {
                text = "Ping",
                style = { width = 60 }
            };
            btnPing.clicked += () => EditorGUIUtility.PingObject(GraphView.GraphAsset);
            GraphViewParent.Toolbar.AddButtonToRight(btnPing);

            ToolbarButton btnSave = new ToolbarButton()
            {
                text = "Save",
                style = { width = 60 }
            };
            btnSave.clicked += () => GraphView.SaveGraphToDisk();
            GraphViewParent.Toolbar.AddButtonToRight(btnSave);

            ToolbarButton btnReload = new ToolbarButton()
            {
                text = "Reload",
                style = { width = 70 }
            };
            btnReload.clicked += ReloadGraph;
            GraphViewParent.Toolbar.AddButtonToRight(btnReload);

            GraphViewParent.GraphViewElement.Add(GraphView);

            OnLoadedGraph();
        }

        void ReloadGraph()
        {
            IGraphOwner tempGraphOwner = GraphOwner;
            SetUp((GraphAsset as IGraphAsset).Graph);
            GraphOwner = tempGraphOwner;
            if (Graph != null && GraphOwner != null)
                Graph.InitializePropertyMapping(GraphOwner);
        }
        #endregion

        #region Overrides
        protected virtual BaseGraphView GenerateGraphView(IGraph _graph)
        {
            BaseGraphView graphView = new BaseGraphView(_graph, CommandDispatcher, this);
            return graphView;
        }

        protected virtual void OnLoadedGraph() { }

        protected virtual GraphState CreateGraphState()
        {
            return new GraphState();
        }
        #endregion
    }
}