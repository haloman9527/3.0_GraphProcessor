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
            BaseGraphWindow window = LoadGraph(_graphOwner.GraphAsset);
            if (window != null)
            {
                window.GraphOwner = _graphOwner;
                window.GraphOwner.GraphAsset.Graph.InitializePropertyMapping(_graphOwner);
            }
        }

        [OnOpenAsset(0)]
        public static bool OnGraphOpened(int instanceID, int line)
        {
            var graph = EditorUtility.InstanceIDToObject(instanceID) as BaseGraphAsset;

            if (graph != null)
            {
                LoadGraph(graph);
                return true;
            }

            return false;
        }

        public static BaseGraphWindow LoadGraph(BaseGraphAsset _graphAsset)
        {
            if (_graphAsset == null) return null;
            Type type = NodeEditorUtility.GetGraphWindowType(_graphAsset.GetType());

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
                window.LoadGraphInternal(_graphAsset);
            }
            else
            {
                window.Focus();
                if (window.GraphAsset != _graphAsset)
                    window.LoadGraphInternal(_graphAsset);
            }
            return window;
        }

        public static BaseGraphWindow GetWindow(BaseGraphAsset _graphData)
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
        BaseGraphAsset graphData;

        public GraphOwner GraphOwner
        {
            get { return graphOwner; }
            private set { graphOwner = value; if (graphOwner != null) graphOwnerInstanceID = graphOwner.GetInstanceID(); }
        }
        public BaseGraphAsset GraphAsset { get { return graphData; } private set { graphData = value; } }
        public BaseGraph Graph { get { return GraphAsset.Graph; } }
        public BaseGraphView GraphView { get { return graphView; } private set { graphView = value; } }
        public ToolbarView Toolbar { get { return toolbar; } private set { toolbar = value; } }

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Default Graph");
            graphViewElement = new VisualElement();
            graphViewElement.name = "GraphView";
            graphViewElement.StretchToParentSize();
            rootVisualElement.Add(graphViewElement);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphOwner;

            if (GraphView == null && GraphAsset != null)
                EditorApplication.delayCall += ReloadGraph;
        }


        void OnPlayModeChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphOwner;
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
            if (GraphView != null)
                GraphView.SaveGraphToDisk();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        void LoadGraphInternal(BaseGraphAsset _graphData)
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
            if (GraphView != null)
                GraphView.RemoveFromHierarchy();
            GraphView = null;

            if (Toolbar != null)
                Toolbar.RemoveFromHierarchy();
            Toolbar = null;

            GraphOwner = null;
        }

        void InitializeWindow(BaseGraphAsset _graphData)
        {
            GraphAsset = _graphData;
            Toolbar = new ToolbarView(this);
            rootVisualElement.Add(toolbar);
            if (GraphAsset == null) return;
            GraphView = InitializeGraphView(_graphData);
            if (GraphView == null) return;

            Toolbar.AddButton("Show In Project", () => EditorGUIUtility.PingObject(GraphView.GraphAsset), false);
            Toolbar.AddButton("Save Assets", () => { GraphView.SaveGraphToDisk(true); }, false);
            Toolbar.AddButton("Reload", ReloadGraph, false);

            Undo.undoRedoPerformed += ReloadGraph;
            graphViewElement.Add(GraphView);
            graphViewElement.style.top = 20;
            rootVisualElement.Add(graphViewElement);

            OnInitializedWindow();
        }

        protected virtual BaseGraphView InitializeGraphView(BaseGraphAsset _graphAsset)
        {
            BaseGraphView graphView = new BaseGraphView();
            GraphView.Initialize(this, _graphAsset);
            return graphView;
        }

        protected virtual void OnInitializedWindow() { }

        void ReloadGraph()
        {
            GraphOwner owner = GraphOwner;
            LoadGraphInternal(GraphAsset);
            GraphOwner = owner;
            if (Graph != null && GraphOwner != null)
                Graph.InitializePropertyMapping(GraphOwner);
        }

        protected virtual void OnDestroy()
        {
            if (Selection.activeObject is ObjectInspector objectInspector
                && objectInspector.TargetObject is BaseNode node
                && node.Owner.Owner == GraphAsset)
            {
                Selection.activeObject = null;
            }
        }
    }
}