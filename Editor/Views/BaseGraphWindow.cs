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
        protected UnityObject graphOwner;
        protected UnityObject graphAsset;
        protected bool locked = false;
        [NonSerialized]
        Dictionary<Type, Action<UnityObject>> graphAssetProcessor = new Dictionary<Type, Action<UnityObject>>();
        #endregion

        #region 属性
        public IGraphOwner GraphOwner
        {
            get { return graphOwner as IGraphOwner; }
            protected set { if (value != null) graphOwner = value.Self(); }
        }
        public UnityObject GraphAsset { get { return graphAsset; } protected set { graphAsset = value; } }
        public BaseGraph Graph { get; private set; }
        public BaseGraphView GraphView { get; private set; }
        public ToolbarView Toolbar { get { return GraphViewParent?.Toolbar; } }
        public GraphViewParentElement GraphViewParent { get; private set; }
        public CommandDispatcher CommandDispatcher { get; private set; }
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
            ToolbarButton btnAllView = new ToolbarButton()
            {
                text = "全览",
                tooltip = "查看所有节点"
            };
            btnAllView.clicked += () =>
            {
                GraphView.FrameAll();
            };
            toolbar.AddButtonToLeft(btnAllView);


            IMGUIContainer drawName = new IMGUIContainer(() =>
            {
                if (GraphAsset != null && GUILayout.Button(GraphAsset.name, GUI.skin.label))
                {
                    EditorGUIUtility.PingObject(GraphAsset);
                }
            });
            drawName.style.flexGrow = 1;
            toolbar.AddToLeft(drawName);

            //ToolbarButton btnPing = new ToolbarButton()
            //{
            //    text = "Ping",
            //    tooltip = "提示正在编辑的Graph文件的位置",
            //    style = { width = 60 }
            //};
            //btnPing.clicked += () => ;
            //toolbar.AddButtonToRight(btnPing);

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
            if (GraphOwner is IGraphAssetOwner graphAssetOwner)
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

        protected void InternalLoad(BaseGraph graph)
        {
            CommandDispatcher commandDispatcher = new CommandDispatcher();
            GraphView = NewGraphView(graph, commandDispatcher);

            if (GraphView == null) return;

            Graph = graph;
            GraphViewParent = new GraphViewParentElement();
            GraphViewParent.StretchToParentSize();
            rootVisualElement.Add(GraphViewParent);

            BuildToolbar(GraphViewParent.Toolbar);

            GraphView.RegisterCallback<KeyDownEvent>(KeyDownCallback);
            GraphViewParent.GraphViewElement.Add(GraphView);
            CommandDispatcher = commandDispatcher;
        }

        // 从GraphOwner加载
        public void Load(IGraphOwner graphOwner)
        {
            Clear();

            GraphOwner = graphOwner;
            GraphAsset = graphOwner.Self();

            GraphOwner.Graph.Initialize(GraphOwner);
            InternalLoad(graphOwner.Graph);
        }

        // 从GraphAssetOwner加载
        public void Load(IGraphAssetOwner graphAssetOwner)
        {
            Clear();

            GraphOwner = graphAssetOwner;
            GraphAsset = graphAssetOwner.GraphAsset;

            GraphOwner.Graph.Initialize(GraphOwner);
            InternalLoad(graphAssetOwner.Graph);
        }

        // 从Graph资源加载
        public void Load(IGraphAsset graphAsset)
        {
            Clear();

            GraphOwner = null;
            GraphAsset = graphAsset as UnityObject;

            InternalLoad(graphAsset.DeserializeGraph());
        }

        // 直接加载Graph对象
        public void Load(BaseGraph graph)
        {
            Clear();

            GraphAsset = null;
            GraphOwner = null;

            InternalLoad(graph);
        }

        public void RegisterGraphAssetProcessor(Type targetType, Action<UnityObject> assetProcessor)
        {
            graphAssetProcessor[targetType] = assetProcessor;
        }
        #endregion

        #region
        protected virtual BaseGraphView NewGraphView(BaseGraph graph, CommandDispatcher commandDispatcher)
        {
            return new BaseGraphView(graph, this, commandDispatcher);
        }
        #endregion

        #region 静态
        /// <summary> 从Graph类型获取对应的GraphWindow </summary>
        public static BaseGraphWindow GetGraphWindow(Type graphType)
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