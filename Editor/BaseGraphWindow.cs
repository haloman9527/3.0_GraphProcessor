using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using CZToolKit.Core.Editors;
using UnityEditor.UIElements;

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
            var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;

            if (asset != null)
            {
                LoadGraph(asset);
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

        public GraphOwner GraphOwner { get { return graphOwner; } private set { graphOwner = value; if (graphOwner != null) graphOwnerInstanceID = graphOwner.GetInstanceID(); } }
        public BaseGraph GraphData { get { return graphData; } }
        public BaseGraphView GraphView { get { return graphView; } protected set { graphView = value; } }
        public ToolbarView Toolbar { get { return toolbar; } }

        protected virtual void OnEnable()
        {
            graphViewElement = new VisualElement();
            graphViewElement.name = "GraphView";
            graphViewElement.StretchToParentSize();
            rootVisualElement.Add(graphViewElement);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (graphData != null)
            {
                LoadGraphInternal(graphData);
            }
            GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphOwner;
            if (GraphOwner != null)
                GraphData.InitializePropertyMapping(GraphOwner.GetBehaviorSource());
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
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    GraphOwner = (EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as GraphOwner);
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
            if (graphData != null && graphView != null)
                graphView.SaveGraphToDisk();
        }

        void LoadGraphInternal(BaseGraph _graphData)
        {
            if (graphData != null && graphData != _graphData)
            {
                EditorUtility.SetDirty(graphData);
                AssetDatabase.SaveAssets();
            }
            ClearWindow();
            graphData = _graphData;
            InitializeWindow(graphData);
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
                rootVisualElement.Remove(toolbar);
            toolbar = null;

            graphOwner = null;
        }

        protected virtual void InitializeWindow(BaseGraph graph)
        {
            titleContent = new GUIContent("Default Graph");

            toolbar = new ToolbarView(this);
            toolbar.AddButton("Show In Project", () => EditorGUIUtility.PingObject(GraphView.GraphData), false);
            toolbar.AddButton("Reload", () => { GraphOwner g = GraphOwner; LoadGraphInternal(GraphData); GraphOwner = g; }, false);
            rootVisualElement.Add(toolbar);

            InitializeGraphView();
            graphViewElement.Add(graphView);
            graphViewElement.style.top = 20;

            rootVisualElement.Add(graphViewElement);
        }

        protected virtual void InitializeGraphView()
        {
            graphView = new BaseGraphView();
            graphView.Initialize(this, graphData);
        }

        protected virtual void OnDestroy()
        {
            if (Selection.activeObject is NodeInspectorObject)
                Selection.activeObject = null;
        }
    }
}