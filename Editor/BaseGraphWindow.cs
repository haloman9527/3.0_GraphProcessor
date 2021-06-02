using CZToolKit.Core.Editors;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

using UnityObject = UnityEngine.Object;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

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
                OpenGraph(graphAsset);
                return true;
            }

            return false;
        }

        public static void Open(GraphAssetOwner _graphAssetOwner)
        {
            _graphAssetOwner.Graph.InitializePropertyMapping(_graphAssetOwner);
            OpenGraph(_graphAssetOwner.Graph);
        }

        public static BaseGraphWindow OpenGraph(IGraphAsset _graphAsset)
        {
            return OpenGraph(_graphAsset.Graph);
        }

        public static BaseGraphWindow OpenGraph(IBaseGraph _graph)
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

            UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(type);
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
        [SerializeField] GraphAssetOwner graphOwner;

        [SerializeField] UnityObject graphAsset;

        public GraphAssetOwner GraphAssetOwner
        {
            get { return graphOwner; }
            private set { graphOwner = value; if (graphOwner != null) graphOwnerInstanceID = graphOwner.GetInstanceID(); }
        }
        public UnityObject GraphAsset { get { return graphAsset; } private set { graphAsset = value; } }
        public IBaseGraph Graph { get; private set; }
        public BaseGraphView GraphView { get; private set; }
        public ToolbarView Toolbar { get; private set; }
        VisualElement GraphViewElement { get; set; }

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Default Graph");
            GraphViewElement = new VisualElement();
            GraphViewElement.name = "GraphView";
            GraphViewElement.StretchToParentSize();
            rootVisualElement.Add(GraphViewElement);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            GraphAssetOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphAssetOwner;

            if (GraphView == null && GraphAsset != null)
                EditorApplication.delayCall += ReloadGraph;
        }


        void OnPlayModeChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    GraphAssetOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphAssetOwner;
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnGUI()
        {
            if (Toolbar != null)
                GUILayoutUtility.GetRect(Toolbar.style.width.value.value, Toolbar.style.height.value.value);

            if (GraphView != null)
                GraphView.OnGUI();
        }

        protected virtual void OnDisable()
        {
            //if (GraphView != null)
            //    GraphView.SaveGraphToDisk();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        void LoadGraphInternal(IBaseGraph _graph)
        {
            if (GraphView != null)
                GraphView.SaveGraphToDisk();
            ClearWindow();
            InitializeWindow(_graph);
        }

        public virtual void OnGraphDeleted()
        {
            ClearWindow();
        }

        protected virtual void ClearWindow()
        {
            if (GraphView != null)
                GraphView.RemoveFromHierarchy();
            GraphView = null;

            if (Toolbar != null)
                Toolbar.RemoveFromHierarchy();
            Toolbar = null;

            GraphAssetOwner = null;
        }

        void InitializeWindow(IBaseGraph _graph)
        {
            Graph = _graph;
            GraphAsset = (Graph as IBaseGraphFromAsset)?.From;
            Toolbar = new ToolbarView(this);
            rootVisualElement.Add(Toolbar);
            if (GraphAsset == null) return;
            GraphView = InitializeGraphView(Graph);
            if (GraphView == null) return;

            Toolbar.AddButton("Show In Project", () => EditorGUIUtility.PingObject(GraphView.GraphAsset), false);
            Toolbar.AddButton("Save Assets", () => { GraphView.SaveGraphToDisk(true); }, false);
            Toolbar.AddButton("Reload", ReloadGraph, false);

            Undo.undoRedoPerformed += ReloadGraph;
            GraphViewElement.Add(GraphView);
            GraphViewElement.style.top = 20;
            rootVisualElement.Add(GraphViewElement);

            GraphView.OnInitializeCompleted?.Invoke();

            OnInitializedWindow();
        }

        protected virtual BaseGraphView InitializeGraphView(IBaseGraph _graph)
        {
            BaseGraphView graphView = new BaseGraphView();
            graphView.Initialize(this, _graph);
            return graphView;
        }

        protected virtual void OnInitializedWindow() { }

        void ReloadGraph()
        {
            GraphAssetOwner owner = GraphAssetOwner;

            LoadGraphInternal((GraphAsset as IGraphAsset).Graph);
            GraphAssetOwner = owner;
            if (Graph != null
                && GraphAssetOwner != null
                && GraphAssetOwner.GraphAsset != null
                && GraphAssetOwner.GraphAsset == GraphAsset as BaseGraphAsset)
                Graph.InitializePropertyMapping(GraphAssetOwner);
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
    }
}