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
using CZToolKit.Core.Editors;
using System;
//using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
//using UnityEngine.Profiling;

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

        /// <summary> 从GraphAssetOwner中加载 </summary>
        public static BaseGraphWindow LoadGraphFromOwner(GraphOwner _graphOwner)
        {
            BaseGraphWindow graphWindow = LoadGraph(_graphOwner.Graph);
            graphWindow.GraphAsset = _graphOwner;
            graphWindow.GraphView.Model.InitializePropertyMapping(_graphOwner);
            graphWindow.GraphOwner = _graphOwner;
            return graphWindow;
        }

        /// <summary> 从GraphAssetOwner中加载 </summary>
        public static BaseGraphWindow LoadGraphFromAssetOwner(GraphAssetOwner _graphAssetOwner)
        {
            BaseGraphWindow graphWindow = LoadGraph(_graphAssetOwner.Graph);
            graphWindow.GraphAsset = _graphAssetOwner.GraphAsset;
            graphWindow.GraphView.Model.InitializePropertyMapping(_graphAssetOwner);
            graphWindow.GraphOwner = _graphAssetOwner;
            return graphWindow;
        }

        /// <summary> 从GraphAsset中加载 </summary>
        public static BaseGraphWindow LoadGraphFromAsset(IGraphAsset _graphAsset)
        {
            BaseGraphWindow graphWindow = LoadGraph(_graphAsset.Graph);
            graphWindow.GraphAsset = _graphAsset as UnityObject;
            return graphWindow;
        }

        /// <summary> 加载Graph </summary>
        /// <param name="_graph"></param>
        public static BaseGraphWindow LoadGraph(BaseGraph _graph)
        {
            Type windowType = GraphProcessorEditorUtility.GetGraphWindowType(_graph.GetType());

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
        public BaseGraph Graph { get; private set; }
        public BaseGraphView GraphView { get; private set; }
        public GraphViewParentElement GraphViewParent { get; private set; }
        public ToolbarView Toolbar { get { return GraphViewParent.Toolbar; } }
        public VisualElement GraphViewElement { get { return GraphViewParent.GraphViewElement; } }
        public CommandDispatcher CommandDispatcher { get; private set; }
        #endregion

        #region Unity
        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Graph Processor");
            rootVisualElement.styleSheets.Add(GraphProcessorStyles.BasicStyle);

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
            {
                GraphView.SaveGraphToDisk();
            }
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        //protected override void Update()
        //{
        //    base.Update();

        //    Profiler.BeginSample("CZToolKit.GraphProcessor");
        //    Stopwatch sw = new Stopwatch();

        //    CommandDispatcher.NotifyObservers();

        //    sw.Stop();
        //    Profiler.EndSample();
        //}

        protected virtual void OnDestroy()
        {
            if (Selection.activeObject is ObjectInspector objectInspector
                && objectInspector.TargetObject is BaseNode node)
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

        void SetUp(BaseGraph _graph)
        {
            if (GraphView != null && GraphAsset != null && EditorUtility.IsDirty(GraphAsset))
                GraphView.SaveGraphToDisk();
            Clear();

            Graph = _graph;

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
            btnPing.clicked += () => EditorGUIUtility.PingObject(GraphAsset);
            GraphViewParent.Toolbar.AddButtonToRight(btnPing);

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
                GraphView.Model.InitializePropertyMapping(GraphOwner);
        }
        #endregion

        #region Overrides
        protected virtual BaseGraphView GenerateGraphView(BaseGraph _graph)
        {
            return new DefaultGraphView(_graph, CommandDispatcher, this);
        }

        protected virtual void OnLoadedGraph() { }

        protected virtual GraphState CreateGraphState()
        {
            return new GraphState();
        }
        #endregion
    }
}