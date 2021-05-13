using CZToolKit.Core.Editors;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

namespace CZToolKit.GraphProcessor.Editors
{
    [Serializable]
    public class BaseGraphWindow : BasicEditorWindow
    {
        #region 静态
        public static void Open(GraphOwner _graphOwner)
        {
            BaseGraphWindow window = LoadGraph(_graphOwner.Graph);
            if (window != null)
                window.GraphOwner = _graphOwner;
            _graphOwner.Graph.Initialize(_graphOwner);
        }

        [OnOpenAsset(0)]
        public static bool OnGraphOpened(int instanceID, int line)
        {
            var graph = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;

            if (graph != null)
            {
                LoadGraph(graph);
                return true;
            }

            return false;
        }

        public static BaseGraphWindow LoadGraph(BaseGraph _graphData)
        {
            if (_graphData == null) return null;
            Type type = NodeEditorUtility.GetGraphWindowType(_graphData.GetType());

            UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(type);
            BaseGraphWindow window = null;

            foreach (var obj in objs)
            {
                if (obj.GetType() == type)
                {
                    window = obj as BaseGraphWindow;
                    break;
                }
            }
            if (window == null)
            {
                window = CreateInstance(type) as BaseGraphWindow;
                window.Show();
                window.LoadGraphInternal(_graphData);
            }
            else
            {
                window.Focus();
                if (window.GraphData != _graphData)
                    window.LoadGraphInternal(_graphData);
            }
            return window;
        }

        public static BaseGraphWindow GetWindow(BaseGraph _graphData)
        {
            Type type = NodeEditorUtility.GetGraphWindowType(_graphData.GetType());

            UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(type);
            BaseGraphWindow window = null;
            foreach (var obj in objs)
            {
                if (obj.GetType() == type)
                {
                    window = obj as BaseGraphWindow;
                    if (window.graphData == _graphData)
                        return window;
                }
            }
            return null;
        }

        #endregion

        ToolbarView toolbar;
        VisualElement graphViewElement;
        BaseGraphView graphView;
        [SerializeField]
        GraphOwner graphOwner;
        [SerializeField]
        int graphOwnerInstanceID;
        [SerializeField]
        BaseGraph graphData;

        public GraphOwner GraphOwner
        {
            get { return graphOwner; }
            private set { graphOwner = value; if (graphOwner != null) graphOwnerInstanceID = graphOwner.GetInstanceID(); }
        }
        public BaseGraph GraphData { get { return graphData; } }
        public BaseGraphView GraphView { get { return graphView; } private set { graphView = value; } }
        public ToolbarView Toolbar { get { return toolbar; } }

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Default Graph");
            graphViewElement = new VisualElement();
            graphViewElement.name = "GraphView";
            graphViewElement.StretchToParentSize();
            rootVisualElement.Add(graphViewElement);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (GraphView == null && GraphData != null)
                EditorApplication.delayCall += ReloadGraph;

            GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphOwner;
            if (GraphOwner != null)
                GraphData.InitializePropertyMapping(GraphOwner);
        }

        void OnPlayModeChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    GraphOwner tempPlayable = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphOwner;
                    if (tempPlayable != null)
                        graphOwnerInstanceID = tempPlayable.gameObject.GetInstanceID();
                    GraphOwner = tempPlayable;
                    if (GraphOwner != null)
                        GraphData.InitializePropertyMapping(GraphOwner);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphOwner;
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnGUI()
        {
            if (toolbar != null)
                GUILayoutUtility.GetRect(toolbar.style.width.value.value, toolbar.style.height.value.value);

            if (GraphView != null)
                GraphView.OnGUI();
        }

        protected virtual void OnDisable()
        {
            if (graphView != null)
                graphView.SaveGraphToDisk();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        void LoadGraphInternal(BaseGraph _graphData)
        {
            if (GraphView != null)
                GraphView.SaveGraphToDisk();
            ClearWindow();
            InitializeWindow(_graphData);
        }

        public virtual void OnGraphDeleted()
        {
            ClearWindow();
        }

        protected virtual void ClearWindow()
        {
            if (graphView != null)
                graphView.RemoveFromHierarchy();
            graphView = null;

            if (toolbar != null)
                toolbar.RemoveFromHierarchy();
            toolbar = null;

            graphOwner = null;
        }

        void InitializeWindow(BaseGraph _graphData)
        {
            graphData = _graphData;
            toolbar = new ToolbarView(this);
            rootVisualElement.Add(toolbar);
            if (GraphData == null) return;
            GraphView = InitializeGraphView(_graphData);
            if (GraphView == null) return;

            toolbar.AddButton("Show In Project", () => EditorGUIUtility.PingObject(GraphView.GraphData), false);
            toolbar.AddButton("Save Assets", () => { GraphView.SaveGraphToDisk(true); }, false);
            toolbar.AddButton("Reload", ReloadGraph, false);

            Undo.undoRedoPerformed += ReloadGraph;
            graphViewElement.Add(GraphView);
            graphViewElement.style.top = 20;
            rootVisualElement.Add(graphViewElement);

            OnInitializedWindow();
        }

        protected virtual BaseGraphView InitializeGraphView(BaseGraph _graphData)
        {
            BaseGraphView graphView = new BaseGraphView();
            GraphView.Initialize(this, _graphData);
            return graphView;
        }

        protected virtual void OnInitializedWindow() { }

        void ReloadGraph()
        {
            GraphOwner owner = GraphOwner;
            LoadGraphInternal(GraphData);
            GraphOwner = owner;
            if (GraphOwner != null)
                GraphData.InitializePropertyMapping(GraphOwner);
        }

        protected virtual void OnDestroy()
        {
            if (Selection.activeObject is ObjectInspector objectInspector
                && objectInspector.targetObject is BaseNode node
                && node.Owner == GraphData)
            {
                Selection.activeObject = null;
            }
        }
    }
}