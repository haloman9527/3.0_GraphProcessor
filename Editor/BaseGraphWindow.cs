using CZToolKit.Core.Editors;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using System.Diagnostics;
using UnityEngine.Profiling;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    [Serializable]
    public class BaseGraphWindow : BasicEditorWindow
    {
        #region 静态
        [OnOpenAsset(0)]
        static bool OnGraphAssetOpened(int _instanceID, int _line)
        {
            var graphAsset = EditorUtility.InstanceIDToObject(_instanceID) as BaseGraphAsset;

            if (graphAsset != null)
            {
                OpenGraphAsset(graphAsset);
                return true;
            }

            return false;
        }

        public static void Open(GraphAssetOwner _graphAssetOwner)
        {
            _graphAssetOwner.Graph.InitializePropertyMapping(_graphAssetOwner);
            LoadGraph(_graphAssetOwner.Graph);
        }

        public static BaseGraphWindow OpenGraphAsset(IGraphAsset _graphAsset)
        {
            return LoadGraph(_graphAsset.Graph);
        }

        public static BaseGraphWindow LoadGraph(IGraph _graph)
        {
            if (_graph == null) return null;
            Type windowType = NodeEditorUtility.GetGraphWindowType(_graph.GetType());

            UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(windowType);
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

        [SerializeField] int graphOwnerInstanceID;
        [SerializeField] IGraphOwner graphOwner;
        [SerializeField] UnityObject graphAsset;

        public IGraphOwner GraphOwner
        {
            get { return graphOwner; }
            private set { graphOwner = value; if (graphOwner != null) graphOwnerInstanceID = graphOwner.GetInstanceID(); }
        }
        public UnityObject GraphAsset { get { return graphAsset; } private set { graphAsset = value; } }
        public IGraph Graph { get; private set; }
        public BaseGraphView GraphView { get; private set; }
        public ToolbarView Toolbar { get; private set; }
        public VisualElement GraphViewElement { get; private set; }
        public CommandDispatcher CommandDispatcher { get; private set; }

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Default Graph");

            GraphViewElement = new VisualElement();
            GraphViewElement.name = "GraphView";
            GraphViewElement.StretchToParentSize();
            rootVisualElement.Add(GraphViewElement);

            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            CommandDispatcher = new CommandDispatcher(CreateGraphState());
            CommandDispatcherHelper.RegisterDefaultCommandHandlers(CommandDispatcher);

            GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as IGraphOwner;

            if (GraphView == null && GraphAsset != null)
                EditorApplication.delayCall += ReloadGraph;
        }

        void OnPlayModeChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphAssetOwner;
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnGUI()
        {
            if (Toolbar != null)
                GUILayoutUtility.GetRect(Toolbar.style.width.value.value, Toolbar.style.height.value.value);
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

        protected virtual void OnDisable()
        {
            if (GraphView != null)
                GraphView.SaveGraphToDisk();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        public virtual void Clear()
        {
            if (GraphView != null)
                GraphView.RemoveFromHierarchy();
            GraphView = null;

            if (Toolbar != null)
                Toolbar.RemoveFromHierarchy();
            Toolbar = null;

            GraphOwner = null;
        }

        protected virtual BaseGraphView CreateGraphView(IGraph _graph)
        {
            BaseGraphView graphView = new BaseGraphView(_graph, CommandDispatcher, this);
            //graphView.Initialize(this, _graph);
            return graphView;
        }

        void LoadGraphInternal(IGraph _graph)
        {
            if (GraphView != null)
                GraphView.SaveGraphToDisk();
            Clear();

            Graph = _graph;
            GraphAsset = (Graph as IGraphFromAsset)?.Asset;
            if (GraphAsset == null) return;

            Toolbar = new ToolbarView(this);
            GraphView = CreateGraphView(Graph);
            if (GraphView == null) return;

            Toolbar.AddButton("Show In Project", () => EditorGUIUtility.PingObject(GraphView.GraphAsset), false);
            Toolbar.AddButton("Save Assets", () => { GraphView.SaveGraphToDisk(); }, false);
            Toolbar.AddButton("Reload", ReloadGraph, false);
            rootVisualElement.Add(Toolbar);

            GraphViewElement.Add(GraphView);
            GraphViewElement.style.top = 20;
            rootVisualElement.Add(GraphViewElement);
        }

        protected virtual void OnLoadedGraph() { }

        void ReloadGraph()
        {
            IGraphOwner tempGraphOwner = GraphOwner;
            LoadGraphInternal((GraphAsset as IGraphAsset).Graph);
            GraphOwner = tempGraphOwner;
            if (Graph != null && GraphOwner != null)
                Graph.InitializePropertyMapping(GraphOwner);
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

        #region Overrides
        protected virtual GraphState CreateGraphState()
        {
            return new GraphState();
        }
        #endregion
    }
}