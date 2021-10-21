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
using CZToolKit.Core;
using CZToolKit.Core.Editors;
using System;
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
        #endregion

        #region 属性
        public IGraphOwner GraphOwner
        {
            get { return graphOwner as IGraphOwner; }
            private set { if (value != null) graphOwner = value.Self(); }
        }
        public UnityObject GraphAsset { get { return graphAsset; } private set { graphAsset = value; } }
        public BaseGraph Graph { get; private set; }
        public BaseGraphView GraphView { get; private set; }
        public ToolbarView Toolbar { get { return GraphViewParent.Toolbar; } }
        public GraphViewParentElement GraphViewParent { get; private set; }
        public CommandDispatcher CommandDispatcher { get; private set; }
        #endregion

        #region Unity
        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Graph Processor");
            rootVisualElement.styleSheets.Add(GraphProcessorStyles.BasicStyle);

            if (GraphAsset != null) Reload();
        }

        protected virtual void OnDisable()
        {
            if (GraphView != null && GraphAsset != null && EditorUtility.IsDirty(GraphAsset))
            {
                GraphView.Save();
            }
        }

        protected virtual void OnDestroy()
        {
            if (Selection.activeObject is ObjectInspector objectInspector && objectInspector.TargetObject is GraphElement)
            {
                Selection.activeObject = null;
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

        protected virtual void BuildToolbar(ToolbarView toolbar)
        {
            ToolbarButton btnCenter = new ToolbarButton()
            {
                text = "Center"
            };
            btnCenter.clicked += () =>
            {
                GraphView.ResetPositionAndZoom();
                GraphView.UpdateViewTransform(GraphView.Model.Position, GraphView.Model.Scale);
            };
            toolbar.AddButtonToLeft(btnCenter);

            ToolbarButton btnPing = new ToolbarButton()
            {
                text = "Ping",
                style = { width = 60 }
            };
            btnPing.clicked += () => EditorGUIUtility.PingObject(GraphAsset);
            toolbar.AddButtonToRight(btnPing);

            ToolbarButton btnReload = new ToolbarButton()
            {
                text = "Reload",
                style = { width = 70 }
            };
            btnReload.clicked += Reload;
            toolbar.AddButtonToRight(btnReload);

            ToolbarButton btnSave = new ToolbarButton()
            {
                text = "Save",
                style = { width = 60 }
            };
            btnSave.clicked += () => GraphView.Save();
            toolbar.AddButtonToRight(btnSave);
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
                    case KeyCode.S:
                        GraphView.Save();
                        evt.StopPropagation();
                        break;
                    default:
                        break;
                }
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
            CommandDispatcher = null;
        }

        protected void InternalLoad(BaseGraph graph)
        {
            if (GraphView != null && GraphAsset != null && EditorUtility.IsDirty(GraphAsset))
            {
                GraphView.Save();
            }
            Clear();

            Graph = graph;

            GraphViewParent = new GraphViewParentElement();
            GraphViewParent.StretchToParentSize();
            rootVisualElement.Add(GraphViewParent);

            CommandDispatcher commandDispatcher = new CommandDispatcher();
            GraphView = NewGraphView(Graph, commandDispatcher);
            if (GraphView == null) return;
            BuildToolbar(GraphViewParent.Toolbar);
            GraphView.RegisterCallback<KeyDownEvent>(KeyDownCallback);
            GraphViewParent.GraphViewElement.Add(GraphView);
            CommandDispatcher = commandDispatcher;
        }

        // 重新加载Graph
        public void Reload()
        {
            if (GraphOwner is IGraphAssetOwner graphAssetOwner)
            {
                Load(graphAssetOwner);
            }
            else if (GraphOwner is IGraphOwner graphOwner)
            {
                Load(graphOwner);
            }
            else if (GraphAsset != null)
            {
                Load(GraphAsset as IGraphAsset);
            }
            else
            {
                Load(Graph);
            }
        }

        // 从GraphOwner加载
        public void Load(IGraphOwner graphOwner)
        {
            GraphAsset = graphOwner.Self();
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
            GraphAsset = graphAsset as UnityObject;
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
        #endregion

        #region 抽象方法
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
                window = CreateInstance(windowType) as BaseGraphWindow;
                window.Show();
            }
            window.Focus();
            return window;
        }

        /// <summary> 从GraphOwner打开Graph </summary>\
        public static BaseGraphWindow Open(IGraphOwner graphOwner)
        {
            if (graphOwner == null) return null;
            var window = GetGraphWindow(graphOwner.Graph.GetType());
            window.Load(graphOwner);
            return window;
        }

        /// <summary> 从GraphAssetOwner打开Graph </summary>\
        public static BaseGraphWindow Open(IGraphAssetOwner graphAssetOwner)
        {
            if (graphAssetOwner == null) return null;
            var window = GetGraphWindow(graphAssetOwner.Graph.GetType());
            window.Load(graphAssetOwner);
            return window;
        }

        /// <summary> 从GraphAsset打开Graph </summary>\
        public static BaseGraphWindow Open(IGraphAsset graphAsset)
        {
            if (graphAsset == null) return null;
            var window = GetGraphWindow(graphAsset.Graph.GetType());
            window.Load(graphAsset);
            return window;
        }

        /// <summary> 打开Graph </summary>\
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