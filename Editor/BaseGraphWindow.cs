using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using CZToolKit.Core.Editors;

namespace GraphProcessor.Editors
{
    [Serializable]
    public class BaseGraphWindow : BasicEditorWindow<BaseGraphWindow>
    {
        #region 静态
        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;

            if (asset != null)
            {
                LoadGraph(asset);
                return true;
            }

            return false;
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

        public static void LoadGraph(BaseGraph _graphData)
        {
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
        }

        #endregion

        protected ToolbarView toolbar;
        protected VisualElement graphViewElement;
        protected BaseGraphView graphView;
        [SerializeField]
        protected BaseGraph graphData;

        public BaseGraph GraphData { get { return graphData; } }
        public BaseGraphView GraphView { get { return graphView; } }


        protected void OnEnable()
        {
            graphViewElement = new VisualElement();
            graphViewElement.name = "GraphView";
            graphViewElement.StretchToParentSize();
            rootVisualElement.Add(graphViewElement);

            if (graphData != null)
                LoadGraphInternal(graphData);
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
        }

        protected virtual void InitializeWindow(BaseGraph graph)
        {
            titleContent = new GUIContent("Default Graph");

            InitializeGraphView();
            graphViewElement.Add(graphView);

            toolbar = new ToolbarView(this);
            rootVisualElement.Add(toolbar);

            graphViewElement.style.top = 20;
            rootVisualElement.Add(graphViewElement);
        }

        protected virtual void InitializeGraphView()
        {
            graphView = new BaseGraphView();
            graphView.Initialize(this, graphData);
        }
    }
}