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
using CZToolKit;
using CZToolKit.VM;
using System;
using CZToolKitEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class BaseGraphWindow : BaseEditorWindow
    {
        #region Fields

        private IGraphOwner m_graphOwner;

        [SerializeField] private UnityObject graphOwner;
        [SerializeField] private UnityObject graphAsset;
        
        #endregion

        #region Properties

        private VisualElement GraphViewContainer { get; set; }
        public Toolbar ToolbarLeft { get; private set; }
        public Toolbar ToolbarCenter { get; private set; }
        public Toolbar ToolbarRight { get; private set; }

        public IGraphOwner GraphOwner
        {
            get
            {
                if (m_graphOwner == null && graphOwner != null)
                    m_graphOwner = graphOwner as IGraphOwner;
                return m_graphOwner;
            }
            protected set
            {
                m_graphOwner = value;
                graphOwner = value as UnityObject;
            }
        }

        public UnityObject GraphAsset
        {
            get { return graphAsset; }
            protected set { graphAsset = value; }
        }

        public BaseGraphVM Graph { get; protected set; }
        public BaseGraphView GraphView { get; private set; }
        public CommandDispatcher CommandDispatcher { get; protected set; }

        #endregion

        #region Unity

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent("Graph Processor");
            InitRootVisualElement();
            Reload();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected virtual void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (Selection.activeObject is ObjectInspector objectInspector && objectInspector.Target is GraphElement)
                Selection.activeObject = null;
        }

        #endregion

        #region Private Methods

        protected virtual void InitRootVisualElement()
        {
            GraphProcessorStyles.GraphWindowTree.CloneTree(rootVisualElement);
            rootVisualElement.name = "rootVisualContainer";
            rootVisualElement.styleSheets.Add(GraphProcessorStyles.BasicStyle);
            
            ToolbarLeft = rootVisualElement.Q<Toolbar>("ToolbarLeft", "unity-toolbar");
            ToolbarCenter = rootVisualElement.Q<Toolbar>("ToolbarCenter", "unity-toolbar");
            ToolbarRight = rootVisualElement.Q<Toolbar>("ToolbarRight", "unity-toolbar");
            GraphViewContainer = rootVisualElement.Q("GraphViewContainer");
            
            // foreach (var styleSheet in FindSkinStyleSheets("basic"))
            // {
            //     rootVisualElement.styleSheets.Add(styleSheet);
            // }
        }

        protected void Load(BaseGraphVM graph, IGraphOwner graphOwner, UnityObject graphAsset, object argument)
        {
            Clear();
            
            Graph = graph;
            GraphOwner = graphOwner;
            GraphAsset = graphAsset;

            BuildToolBar();

            GraphView = NewGraphView(argument);
            GraphView.onDirty += OnGraphViewDirty;
            GraphView.onUndirty += OnGraphViewUndirty;
            var coroutine = StartCoroutine(GraphView.Initialize());
            GraphView.RegisterCallback<DetachFromPanelEvent>(evt => { StopCoroutine(coroutine); });
            GraphViewContainer.Add(GraphView);
        }

        #endregion

        #region Public Methods

        // public virtual void ChangeSkin(UITKSkin skin)
        // {
        //     // 清理
        //     foreach (var styleSheet in FindSkinStyleSheets("basic"))
        //     {
        //         rootVisualElement.styleSheets.Remove(styleSheet);
        //     }
        //
        //     if (GraphView != null)
        //     {
        //         foreach (var styleSheet in FindSkinStyleSheets("graph"))
        //         {
        //             GraphView.styleSheets.Remove(styleSheet);
        //         }
        //
        //         var nodeSkins = FindSkinStyleSheets("node").ToArray();
        //         var portSkins = FindSkinStyleSheets("port").ToArray();
        //         foreach (var nodeView in GraphView.NodeViews.Values)
        //         {
        //             foreach (var styleSheet in nodeSkins)
        //             {
        //                 nodeView.styleSheets.Remove(styleSheet);
        //             }
        //             foreach (var portView in nodeView.PortViews.Values)
        //             {
        //                 foreach (var styleSheet in portSkins)
        //                 {
        //                     portView.styleSheets.Remove(styleSheet);
        //                 }
        //             }
        //         }
        //         
        //         var edgeSkins = FindSkinStyleSheets("edge").ToArray();
        //         foreach (var connectionView in GraphView.ConnectionViews.Values)
        //         {
        //             foreach (var styleSheet in edgeSkins)
        //             {
        //                 connectionView.styleSheets.Remove(styleSheet);
        //             }
        //         }
        //
        //         var groupSkins = FindSkinStyleSheets("edge").ToArray();
        //         foreach (var groupView in GraphView.GroupViews.Values)
        //         {
        //             foreach (var styleSheet in groupSkins)
        //             {
        //                 groupView.styleSheets.Remove(styleSheet);
        //             }
        //         }
        //     }
        //
        //     // 装载
        //     this.skin = skin;
        //     
        //     foreach (var styleSheet in FindSkinStyleSheets("basic"))
        //     {
        //         rootVisualElement.styleSheets.Add(styleSheet);
        //     }
        //     
        //     if (GraphView != null)
        //     {
        //         foreach (var styleSheet in FindSkinStyleSheets("graph"))
        //         {
        //             GraphView.styleSheets.Add(styleSheet);
        //         }
        //
        //         var nodeSkins = FindSkinStyleSheets("node").ToArray();
        //         var portSkins = FindSkinStyleSheets("port").ToArray();
        //         foreach (var nodeView in GraphView.NodeViews.Values)
        //         {
        //             foreach (var styleSheet in nodeSkins)
        //             {
        //                 nodeView.styleSheets.Add(styleSheet);
        //             }
        //             foreach (var portView in nodeView.PortViews.Values)
        //             {
        //                 foreach (var styleSheet in portSkins)
        //                 {
        //                     portView.styleSheets.Add(styleSheet);
        //                 }
        //             }
        //         }
        //         
        //         var edgeSkins = FindSkinStyleSheets("edge").ToArray();
        //         foreach (var connectionView in GraphView.ConnectionViews.Values)
        //         {
        //             foreach (var styleSheet in edgeSkins)
        //             {
        //                 connectionView.styleSheets.Add(styleSheet);
        //             }
        //         }
        //
        //         var groupSkins = FindSkinStyleSheets("group").ToArray();
        //         foreach (var groupView in GraphView.GroupViews.Values)
        //         {
        //             foreach (var styleSheet in groupSkins)
        //             {
        //                 groupView.styleSheets.Add(styleSheet);
        //             }
        //         }
        //     }
        // }
        //
        // public IEnumerable<StyleSheet> FindSkinStyleSheets(string name)
        // {
        //     var foundSkins = new HashSet<UITKSkin>();
        //     foreach (var styleSheet in FindStyleSheets(skin))
        //     {
        //         yield return styleSheet;
        //     }
        //
        //     IEnumerable<StyleSheet> FindStyleSheets(UITKSkin skin)
        //     {
        //         foundSkins.Add(skin);
        //         foreach (var otherSkin in skin.otherSkins)
        //         {
        //             if (foundSkins.Contains(skin))
        //             {
        //                 continue;
        //             }
        //             foreach (var styleSheet in FindStyleSheets(otherSkin))
        //             {
        //                 yield return styleSheet;
        //             }
        //         }
        //
        //         yield return skin.styleSheets.Find(x => x.name == name)?.styleSheet;
        //     }
        // }

        public virtual void Clear()
        {
            OnGraphViewUndirty();
            
            ToolbarLeft.Clear();
            ToolbarCenter.Clear();
            ToolbarRight.Clear();
            GraphViewContainer.Clear();

            Graph = null;
            GraphView = null;
            GraphAsset = null;
            GraphOwner = null;
            CommandDispatcher = null;
        }

        // 重新加载Graph
        public virtual void Reload()
        {
            if (GraphOwner is IGraphAssetOwner graphAssetOwner && graphAssetOwner.GraphAsset != null)
            {
                ForceLoad(graphAssetOwner);
            }
            else if (GraphOwner is IGraphOwner graphOwner)
            {
                ForceLoad(graphOwner);
            }
            else if (GraphAsset is IGraphAsset graphAsset)
            {
                ForceLoad(graphAsset);
            }
            else if (Graph is BaseGraphVM graphVM)
            {
                ForceLoad(graphVM);
            }
        }

        // 从GraphOwner加载
        public void ForceLoad(IGraphOwner graphOwner, object argument = null)
        {
            Clear();
            Load(graphOwner.Graph, graphOwner, (UnityObject)graphOwner, argument);
        }

        // 从GraphAssetOwner加载
        public void ForceLoad(IGraphAssetOwner graphAssetOwner, object argument = null)
        {
            Clear();
            Load(graphAssetOwner.Graph, graphAssetOwner, graphAssetOwner.GraphAsset, argument);
        }

        // 从Graph资源加载
        public void ForceLoad(IGraphAsset graphAsset, object argument = null)
        {
            Clear();
            Load(ViewModelFactory.CreateViewModel(graphAsset.DeserializeGraph()) as BaseGraphVM, null, graphAsset as UnityObject, argument);
        }

        // 直接加载GraphVM对象
        public void ForceLoad(BaseGraphVM graph, object argument = null)
        {
            Clear();
            Load(graph, null, null, argument);
        }

        // 直接加载Graph对象
        public void ForceLoad(BaseGraph graph, object argument = null)
        {
            Clear();
            Load(ViewModelFactory.CreateViewModel(ViewModelFactory.CreateViewModel(graph) as BaseGraphVM) as BaseGraphVM, null, null, argument);
        }

        #endregion

        #region Callbacks

        void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    Reload();
                    break;
            }
        }

        void OnGraphViewDirty()
        {
            if (!titleContent.text.EndsWith(" *"))
                titleContent.text += " *";
            if (GraphAsset != null)
                EditorUtility.SetDirty(GraphAsset);
            if (GraphOwner is UnityObject uobj && uobj != null)
                EditorUtility.SetDirty(uobj);
        }

        void OnGraphViewUndirty()
        {
            if (titleContent.text.EndsWith(" *"))
                titleContent.text = titleContent.text.Replace(" *", "");
        }

        #endregion

        #region Overrides

        protected virtual BaseGraphView NewGraphView(object argument)
        {
            return new DefaultGraphView(Graph, this, new CommandDispatcher());
        }

        protected virtual void BuildToolBar()
        {
            ToolbarButton btnOverview = new ToolbarButton()
            {
                name = "btnOverview",
                text = "Overview",
                tooltip = "查看所有节点"
            };
            btnOverview.clicked += () => { GraphView.FrameAll(); };
            ToolbarLeft.Add(btnOverview);

            IMGUIContainer drawName = new IMGUIContainer(() =>
            {
                GUILayout.BeginHorizontal();
                if (GraphAsset != null && GUILayout.Button(GraphAsset.name, EditorStyles.toolbarButton))
                    EditorGUIUtility.PingObject(GraphAsset);
                GUILayout.EndHorizontal();
            });
            drawName.style.flexGrow = 1;
            ToolbarCenter.Add(drawName);

            ToolbarButton btnReload = new ToolbarButton()
            {
                name = "btnReload",
                text = "Reload",
                tooltip = "重新加载",
            };
            btnReload.clicked += Reload;
            ToolbarRight.Add(btnReload);
        }

        #endregion

        #region Static

        /// <summary> 从Graph类型获取对应的GraphWindow </summary>
        public static BaseGraphWindow GetGraphWindow(Type graphType)
        {
            var windowType = GraphProcessorEditorUtil.GetViewType(graphType);
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
            var window = GetGraphWindow(graphOwner.Graph.ModelType);
            window.ForceLoad(graphOwner);
            return window;
        }

        /// <summary> 从GraphAssetOwner打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAssetOwner graphAssetOwner)
        {
            var window = GetGraphWindow(graphAssetOwner.Graph.ModelType);
            window.ForceLoad(graphAssetOwner);
            return window;
        }

        /// <summary> 从GraphAsset打开Graph </summary>
        public static BaseGraphWindow Open(IGraphAsset graphAsset)
        {
            var window = GetGraphWindow(graphAsset.GraphType);
            window.ForceLoad(graphAsset);
            return window;
        }

        /// <summary> 打开Graph </summary>
        public static BaseGraphWindow Open(BaseGraphVM graph)
        {
            var window = GetGraphWindow(graph.ModelType);
            window.ForceLoad(graph);
            return window;
        }

        /// <summary> 打开Graph </summary>
        public static BaseGraphWindow Open(BaseGraph graph)
        {
            var window = GetGraphWindow(graph.GetType());
            window.ForceLoad(graph);
            return window;
        }

        /// <summary> 双击资源 </summary>
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            UnityObject go = EditorUtility.InstanceIDToObject(instanceID);
            if (go == null) 
                return false;
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