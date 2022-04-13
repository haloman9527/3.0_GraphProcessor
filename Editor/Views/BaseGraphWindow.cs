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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    [Serializable]
    public class BaseGraphWindow : BasicEditorWindow
    {
        #region 字段
        [SerializeField] protected UnityObject graphOwner;
        [SerializeField] protected UnityObject graphAsset;
        [SerializeField] protected bool locked = false;
        #endregion

        #region 属性
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
        public IGraph Graph
        {
            get;
            private set;
        }
        public BaseGraphView GraphView
        {
            get; private set;
        }
        public ToolbarView Toolbar
        {
            get { return GraphViewParent?.Toolbar; }
        }
        public GraphViewContainer GraphViewParent
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
            rootVisualElement.styleSheets.Add(GraphProcessorStyles.BasicStyle);

            Reload();
        }

        protected virtual void OnDestroy()
        {
            if (Selection.activeObject is ObjectInspector objectInspector && objectInspector.Target is GraphElement)
            {
                Selection.activeObject = null;
            }
        }
        #endregion

        #region 方法
        protected virtual void BuildToolbar(ToolbarView toolbar)
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
            toolbar.AddButtonToLeft(btnOverview);
            btnOverview.style.width = 80;


            IMGUIContainer drawName = new IMGUIContainer(() =>
            {
                GUILayout.BeginHorizontal();
                if (GraphAsset != null && GUILayout.Button(GraphAsset.name, EditorStyles.toolbarButton))
                {
                    EditorGUIUtility.PingObject(GraphAsset);
                }
                GUILayout.EndHorizontal();
            });
            drawName.style.flexGrow = 1;
            toolbar.AddToLeft(drawName);

            ToolbarButton btnReload = new ToolbarButton()
            {
                text = "Reload",
                tooltip = "重新加载",
                style = { width = 70 }
            };
            btnReload.clicked += Reload;
            toolbar.AddButtonToRight(btnReload);
        }

        protected virtual void KeyDownCallback(KeyDownEvent evt)
        {
            if (evt.commandKey || evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.Z:
                        GraphView.CommandDispacter.Undo();
                        evt.StopPropagation();
                        break;
                    case KeyCode.Y:
                        GraphView.CommandDispacter.Redo();
                        evt.StopPropagation();
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void Clear()
        {
            if (GraphViewParent != null)
            {
                GraphViewParent.RemoveFromHierarchy();
            }

            Graph = null;
            GraphView = null;
            GraphViewParent = null;
            GraphAsset = null;
            GraphOwner = null;
            CommandDispatcher = null;
        }

        // 重新加载Graph
        public virtual void Reload()
        {
            if (GraphOwner is IGraphAssetOwner graphAssetOwner && graphAssetOwner.GraphAsset != null)
            {
                Load(graphAssetOwner);
            }
            else if (GraphOwner is IGraphOwner graphOwner)
            {
                Load(graphOwner);
            }
            else if (GraphAsset is IGraphAsset graphAsset)
            {
                Load(graphAsset);
            }
            else if (Graph is BaseGraph graph)
            {
                Load(graph);
            }
        }

        protected void InternalLoad(IGraph graph, CommandDispatcher commandDispatcher)
        {
            GraphView = NewGraphView(graph);
            if (GraphView == null)
                return;
            OnGraphViewUndirty();
            GraphView.SetUp(graph, this, commandDispatcher);
            GraphView.onDirty += OnGraphViewDirty;
            GraphView.onUndirty += OnGraphViewUndirty;
            Graph = graph;
            GraphViewParent = new GraphViewContainer();
            GraphViewParent.StretchToParentSize();
            rootVisualElement.Add(GraphViewParent);
            GraphView.RegisterCallback<KeyDownEvent>(KeyDownCallback);
            GraphViewParent.GraphViewElement.Add(GraphView);

            BuildToolbar(GraphViewParent.Toolbar);
        }

        // 从GraphOwner加载
        public void Load(IGraphOwner graphOwner)
        {
            Clear();

            GraphOwner = graphOwner;
            GraphAsset = (UnityObject)graphOwner;
            CommandDispatcher = new CommandDispatcher();

            GraphOwner.Graph.Initialize(GraphOwner);
            InternalLoad(graphOwner.Graph, CommandDispatcher);
        }

        // 从GraphAssetOwner加载
        public void Load(IGraphAssetOwner graphAssetOwner)
        {
            Clear();

            GraphOwner = graphAssetOwner;
            GraphAsset = graphAssetOwner.GraphAsset as UnityObject;
            CommandDispatcher = new CommandDispatcher();

            GraphOwner.Graph.Initialize(GraphOwner);
            InternalLoad(graphAssetOwner.Graph, CommandDispatcher);
        }

        // 从Graph资源加载
        public void Load(IGraphAsset graphAsset)
        {
            Clear();

            GraphOwner = null;
            GraphAsset = graphAsset as UnityObject;
            CommandDispatcher = new CommandDispatcher();

            InternalLoad(graphAsset.DeserializeGraph(), CommandDispatcher);
        }

        // 直接加载Graph对象
        public void Load(BaseGraph graph)
        {
            Clear();

            GraphAsset = null;
            GraphOwner = null;
            CommandDispatcher = new CommandDispatcher();

            InternalLoad(graph, CommandDispatcher);
        }

        public void OnGraphViewDirty()
        {
            if (!titleContent.text.EndsWith(" *"))
                titleContent.text += " *";
            if (GraphAsset != null)
                EditorUtility.SetDirty(GraphAsset);
            if (GraphOwner is UnityObject uobj && uobj != null)
                EditorUtility.SetDirty(uobj);
        }

        public void OnGraphViewUndirty()
        {
            if (titleContent.text.EndsWith(" *"))
                titleContent.text = titleContent.text.Replace(" *", "");
        }
        #endregion

        #region Overrides
        protected virtual BaseGraphView NewGraphView(IGraph graph)
        {
            return new BaseGraphView();
        }
        #endregion

        #region Static
        /// <summary> 从Graph类型获取对应的GraphWindow </summary>
        public static BaseGraphWindow GetGraphWindow(Type graphType)
        {
            var windowType = GraphProcessorEditorUtil.GetGraphWindowType(graphType);
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
            var window = GetGraphWindow(graphOwner.Graph.GetType());
            window.Load(graphOwner);
            return window;
        }

        /// <summary> 从GraphAssetOwner打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAssetOwner graphAssetOwner)
        {
            if (graphAssetOwner == null) return null;
            if (graphAssetOwner.GraphAsset == null) return null;
            var window = GetGraphWindow(graphAssetOwner.Graph.GetType());
            window.Load(graphAssetOwner);
            return window;
        }

        /// <summary> 从GraphAsset打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAsset graphAsset)
        {
            if (graphAsset == null) return null;
            var window = GetGraphWindow(graphAsset.GraphType);
            window.Load(graphAsset);
            return window;
        }

        /// <summary> 打开Graph </summary>
        public static BaseGraphWindow Open(BaseGraph graph)
        {
            if (graph == null) return null;
            var window = GetGraphWindow(graph.GetType());
            window.Load(graph);
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