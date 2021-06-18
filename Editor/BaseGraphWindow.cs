using CZToolKit.Core.Editors;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEngine.Profiling;

using UnityObject = UnityEngine.Object;
using UnityEditor.UIElements;

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
                window.Show();
                window.LoadGraphInternal(_graph);
            }
            else
            {
                window.Focus();
                if (window.Graph != _graph)
                    window.LoadGraphInternal(_graph);
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
            titleContent = new GUIContent("Default Graph");

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

        void LoadGraphInternal(IGraph _graph)
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

            GraphView = CreateGraphView(Graph);
            if (GraphView == null) return;

            ToolbarButton btnPing = new ToolbarButton()
            {
                text = "Ping",
                style = { alignSelf = Align.Center, width = 60, unityTextAlign = TextAnchor.MiddleCenter, color = Color.black }
            };
            btnPing.clicked += () => EditorGUIUtility.PingObject(GraphView.GraphAsset);
            GraphViewParent.Toolbar.AddToRight(btnPing);

            ToolbarButton btnSave = new ToolbarButton()
            {
                text = "Save",
                style = { alignSelf = Align.Center, width = 60, unityTextAlign = TextAnchor.MiddleCenter, color = Color.black }
            };
            btnSave.clicked += () => GraphView.SaveGraphToDisk();
            GraphViewParent.Toolbar.AddToRight(btnSave);

            ToolbarButton btnReload = new ToolbarButton()
            {
                text = "Reload",
                style = { alignSelf = Align.Center, width = 70, unityTextAlign = TextAnchor.MiddleCenter, color = Color.black }
            };
            btnReload.clicked += ReloadGraph;
            GraphViewParent.Toolbar.AddToRight(btnReload);

            GraphViewParent.SetUp(GraphView);

            OnLoadedGraph();
        }

        void ReloadGraph()
        {
            IGraphOwner tempGraphOwner = GraphOwner;
            LoadGraphInternal((GraphAsset as IGraphAsset).Graph);
            GraphOwner = tempGraphOwner;
            if (Graph != null && GraphOwner != null)
                Graph.InitializePropertyMapping(GraphOwner);
        }
        #endregion

        #region Overrides
        protected virtual BaseGraphView CreateGraphView(IGraph _graph)
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