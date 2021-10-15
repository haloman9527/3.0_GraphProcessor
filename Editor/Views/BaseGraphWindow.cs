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
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

using UnityObject = UnityEngine.Object;
using CZToolKit.Core;

namespace CZToolKit.GraphProcessor.Editors
{
    [Serializable]
    public class BaseGraphWindow : BasicEditorWindow
    {
        #region 字段
        protected int graphOwnerInstanceID;
        protected IGraphOwner graphOwner;
        protected UnityObject graphAsset;
        protected bool locked = false;
        #endregion

        #region 属性
        public IGraphOwner GraphOwner
        {
            get { return graphOwner; }
            private set { graphOwner = value; if (graphOwner != null) graphOwnerInstanceID = (graphOwner as UnityObject) ? GetInstanceID() : -1; }
        }
        public UnityObject GraphAsset { get { return graphAsset; } private set { graphAsset = value; } }
        public BaseGraph Graph { get; private set; }
        public BaseGraphView GraphView { get; private set; }
        public GraphViewParentElement GraphViewParent { get; private set; }
        public ToolbarView Toolbar { get { return GraphViewParent.Toolbar; } }
        public VisualElement GraphViewElement { get { return GraphViewParent.GraphViewElement; } }
        #endregion

        #region Unity
        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Graph Processor");
            rootVisualElement.styleSheets.Add(GraphProcessorStyles.BasicStyle);

            GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as IGraphOwner;

            if (GraphView == null && GraphAsset != null)
                EditorApplication.delayCall += Reload;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        protected virtual void OnDisable()
        {
            if (GraphView != null && GraphAsset != null && EditorUtility.IsDirty(GraphAsset))
            {
                GraphView.Save();
            }
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

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
        void OnPlayModeChanged(PlayModeStateChange playMode)
        {
            switch (playMode)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    GraphOwner = EditorUtility.InstanceIDToObject(graphOwnerInstanceID) as IGraphAssetOwner;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 方法
        protected override void ShowButton(Rect rect)
        {
            base.ShowButton(rect);
            rect.x -= 28;
            rect.width = 20;
            if (GUI.Button(rect, locked ? EditorGUIUtility.IconContent("IN LockButton act@2x") : EditorGUIUtility.IconContent("IN LockButton on act@2x"), EditorStylesExtension.OnlyIconButtonStyle))
            {
                locked = !locked;
            }

        }

        public void Clear()
        {
            if (GraphViewParent != null)
            {
                GraphViewParent.RemoveFromHierarchy();
                GraphViewParent = null;
            }
            Graph = null;
            GraphAsset = null;
            GraphOwner = null;
        }

        void InternalLoad(BaseGraph graph)
        {
            if (GraphView != null && GraphAsset != null && EditorUtility.IsDirty(GraphAsset))
                GraphView.Save();
            Clear();

            Graph = graph;

            GraphViewParent = new GraphViewParentElement();
            GraphViewParent.StretchToParentSize();
            rootVisualElement.Add(GraphViewParent);

            GraphView = NewGraphView(Graph);
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
            btnReload.clicked += Reload;
            GraphViewParent.Toolbar.AddButtonToRight(btnReload);

            GraphViewParent.GraphViewElement.Add(GraphView);
        }

        // 重新加载Graph
        public void Reload()
        {
            IGraphOwner tempGraphOwner = GraphOwner;

            var targetGraph = GraphAsset == null ? Graph : (GraphAsset as IGraphAsset).Graph;
            if (targetGraph != null && tempGraphOwner != null)
                targetGraph.InitializePropertyMapping(tempGraphOwner);
            InternalLoad(targetGraph);
            GraphOwner = tempGraphOwner;
        }

        // 从GraphOwner加载
        public void Load(IGraphOwner graphOwner)
        {
            GraphAsset = null;
            GraphOwner = graphOwner;
            GraphOwner.Graph.InitializePropertyMapping(GraphOwner);
            InternalLoad(GraphOwner.Graph);
        }

        // 从GraphAssetOwner加载
        public void Load(IGraphAssetOwner graphAssetOwner)
        {
            GraphAsset = graphAssetOwner.GraphAsset;
            GraphOwner = graphAssetOwner;
            GraphOwner.Graph.InitializePropertyMapping(GraphOwner);
            InternalLoad(GraphOwner.Graph);
        }

        // 从Graph资源加载
        public void Load(IGraphAsset graphAsset)
        {
            this.graphAsset = graphAsset as UnityObject;
            GraphOwner = null;
            InternalLoad(graphAsset.Graph);
        }

        // 直接加载Graph对象
        public void Load(BaseGraph graph)
        {
            GraphAsset = null;
            GraphOwner = null;
            InternalLoad(graph);
        }

        // 保存到硬盘(如果可以)
        public void Save()
        {
            if (GraphAsset == null) return;

            EditorUtility.SetDirty(GraphAsset);

            if (AssetDatabase.Contains(GraphAsset))
                AssetDatabase.SaveAssets();
        }
        #endregion

        #region 抽象方法
        protected virtual BaseGraphView NewGraphView(BaseGraph graph)
        {
            return new BaseGraphView(graph, this, new CommandDispatcher());
        }
        #endregion

        #region 静态
        /// <summary> 从Graph类型获取对应的GraphWindow </summary>
        static BaseGraphWindow GetGraphWindow(Type graphType)
        {
            var windowType = GraphProcessorEditorUtility.GetGraphWindowType(graphType);
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
            }
            window.Focus();
            return window;
        }

        /// <summary> 从GraphOwner打开Graph </summary>
        /// <param name="graphAsset"></param>
        public static BaseGraphWindow Open(IGraphOwner graphOwner)
        {
            if (graphOwner == null) return null;
            var window = GetGraphWindow(graphOwner.Graph.GetType());
            window.Load(graphOwner);
            return window;
        }

        /// <summary> 从GraphAssetOwner打开Graph </summary>
        /// <param name="graphAsset"></param>
        public static BaseGraphWindow Open(IGraphAssetOwner graphAssetOwner)
        {
            if (graphAssetOwner == null) return null;
            var window = GetGraphWindow(graphAssetOwner.Graph.GetType());
            window.Load(graphAssetOwner);
            return window;
        }

        /// <summary> 从GraphAsset打开Graph </summary>
        /// <param name="graphAsset"></param>
        public static BaseGraphWindow Open(IGraphAsset graphAsset)
        {
            if (graphAsset == null) return null;
            var window = GetGraphWindow(graphAsset.Graph.GetType());
            window.Load(graphAsset);
            return window;
        }

        /// <summary> 打开Graph </summary>
        /// <param name="graph"></param>
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
            IGraphAsset graphAsset = EditorUtility.InstanceIDToObject(instanceID) as IGraphAsset;
            if (graphAsset == null) return false;
            Open(graphAsset);
            return true;
        }
        #endregion
    }
}