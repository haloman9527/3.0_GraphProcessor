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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

#if UNITY_EDITOR
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace Atom.GraphProcessor.Editors
{
    public abstract partial class BaseNodeView : NodeView, IGraphElementView<BaseNodeProcessor>
    {
        #region 字段

        public readonly Label nodeLabel;
        public readonly Image nodeIcon;
        public readonly VisualElement controls;
        public readonly VisualElement nodeBorder;
        public readonly VisualElement topPortContainer;
        public readonly VisualElement bottomPortContainer;
        public readonly VisualElement titleInputPortContainer;
        public readonly VisualElement titleOutputPortContainer;
        public readonly VisualElement horizontalDivider;
        public readonly VisualElement verticalDivider;

        private List<IconBadge> badges = new List<IconBadge>();
        private Dictionary<string, BasePortView> portViews = new Dictionary<string, BasePortView>();

        #endregion

        #region 属性

        public bool Selectable
        {
            get => (capabilities & Capabilities.Selectable) == Capabilities.Selectable;
            set => capabilities = value ? (capabilities | Capabilities.Selectable) : (capabilities & ~Capabilities.Selectable);
        }

        public bool Deletable
        {
            get => (capabilities & Capabilities.Deletable) == Capabilities.Deletable;
            set => capabilities = value ? (capabilities | Capabilities.Deletable) : (capabilities & ~Capabilities.Deletable);
        }

        public bool Movable
        {
            get => (capabilities & Capabilities.Movable) == Capabilities.Movable;
            set => capabilities = value ? (capabilities | Capabilities.Movable) : (capabilities & ~Capabilities.Movable);
        }

        public Label NodeLabel => nodeLabel;

        public Image NodeIcon => nodeIcon;

        public BaseGraphView Owner { get; private set; }
        public BaseNodeProcessor ViewModel { get; protected set; }

        public SerializedProperty BindingProperty { get; private set; }
        public IGraphElementProcessor V => ViewModel;

        public IReadOnlyDictionary<string, BasePortView> PortViews => portViews;

        #endregion

        public BaseNodeView()
        {
            styleSheets.Add(GraphProcessorEditorStyles.DefaultStyles.BaseNodeViewStyle);

            var contents = mainContainer.Q("contents");

            nodeBorder = this.Q(name: "node-border");
            nodeLabel = titleContainer.Q<Label>("title-label");
            horizontalDivider = this.Q(name: "divider", className: "horizontal");
            verticalDivider = topContainer.Q(name: "divider", className: "vertical");

            nodeIcon = new Image() { name = "title-icon" };
            titleContainer.Insert(0, nodeIcon);

            controls = new BaseVisualElement() { name = "controls" };
            contents.Add(controls);

            topPortContainer = new VisualElement { name = "top-input" };
            nodeBorder.Insert(0, topPortContainer);

            bottomPortContainer = new VisualElement { name = "bottom-input" };
            nodeBorder.Add(bottomPortContainer);

            outputContainer.style.alignItems = Align.FlexEnd;

            titleInputPortContainer = new VisualElement { name = "title-input" };
            titleContainer.Add(titleInputPortContainer);
            titleInputPortContainer.SendToBack();

            titleOutputPortContainer = new VisualElement { name = "title-output" };
            titleContainer.Add(titleOutputPortContainer);
            titleOutputPortContainer.BringToFront();

            controls.RegisterCallback<BaseVisualElement.ChildChangedEvent>(OnChildChanged);
            this.RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        #region Initialize

        public void SetUp(BaseNodeProcessor node, BaseGraphView graphView)
        {
            Owner = graphView;
            ViewModel = node;
        }
        
        public void Init()
        {
            // 初始化
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            this.title = ViewModel.Title;
            this.tooltip = ViewModel.Tooltip;

            var color = ViewModel.TitleColor.ToColor();
            var lum = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
            this.nodeLabel.style.color = lum > 0.5f && ViewModel.TitleColor.a > 0.5f ? Color.black : Color.white * 0.9f;
            this.titleContainer.style.backgroundColor = color;

            OnIndexChanged(-1, ViewModel.Index);
            
            foreach (var port in ViewModel.InPorts)
            {
                AddPortView(port);
            }

            foreach (var port in ViewModel.OutPorts)
            {
                AddPortView(port);
            }

            foreach (var portView in portViews.Values)
            {
                portView.Init();
            }

            RefreshPorts();
            RefreshPortContainer();
            RefreshControls();
            RefreshContentsHorizontalDivider();
            
            ViewModel.PropertyChanged += OnViewModelChanged;
            ViewModel.onPortAdded += OnPortAdded;
            ViewModel.onPortRemoved += OnPortRemoved;
            ViewModel.onIndexChanged += OnIndexChanged;
            
            this.DoInit();
        }

        public void UnInit()
        {
            foreach (var portView in portViews.Values)
            {
                portView.UnInit();
            }
            
            ViewModel.PropertyChanged -= OnViewModelChanged;
            ViewModel.onPortAdded -= OnPortAdded;
            ViewModel.onPortRemoved -= OnPortRemoved;
            this.DoUnInit();
        }

        #endregion

        #region Callbacks

        private void OnChildChanged(BaseVisualElement.ChildChangedEvent evt)
        {
            RefreshControls();
            RefreshContentsHorizontalDivider();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.shiftKey)
            { 
                var hashSet = new HashSet<BaseNodeView>();
                var queue = new Queue<BaseNodeView>();
                queue.Enqueue(this);
                while (queue.Count > 0)
                {
                    var n = queue.Dequeue();
                    if (hashSet.Contains(n))
                    {
                        continue;
                    }

                    hashSet.Add(n);
                    foreach (var p in n.ViewModel.OutPorts)
                    {
                        foreach (var c in p.Connections)
                        {
                            if (Owner.NodeViews.TryGetValue(c.ToNodeID, out var nv))
                            {
                                queue.Enqueue(nv);
                            }
                        }
                    }
                }

                Owner.AddToSelection(hashSet.Where(n => n.Selectable));
                evt.StopPropagation();
            }
        }

        private void OnIndexChanged(int oldIndex, int newIndex)
        {
            if (this.Owner.Context.graphWindow.UnityGraphAssetSO != null)
            {
                this.Owner.Context.graphWindow.UnityGraphAssetSO.Update();
                this.BindingProperty = this.Owner.Context.graphWindow.UnityGraphAssetSO.FindProperty($"data.nodes.Array.data[{newIndex}]");
            }
        }

        private void OnPortAdded(PortProcessor port)
        {
            AddPortView(port);
            RefreshPorts();
            RefreshContentsHorizontalDivider();
            RefreshPortContainer();
        }

        private void OnPortRemoved(PortProcessor port)
        {
            RemovePortView(port);
            RefreshPorts();
            RefreshContentsHorizontalDivider();
            RefreshPortContainer();
        }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as BaseNodeProcessor;
            switch (e.PropertyName)
            {
                case nameof(BaseNode.position):
                {
                    base.SetPosition(new Rect(node.Position.ToVector2(), GetPosition().size));
                    Owner.SetDirty();
                    break;
                }
                case ConstValues.NODE_TITLE_NAME:
                {
                    base.title = node.Title;
                    break;
                }
                case ConstValues.NODE_TITLE_COLOR_NAME:
                {
                    titleContainer.style.backgroundColor = node.TitleColor.ToColor();
                    var lum = 0.299f * node.TitleColor.r + 0.587f * node.TitleColor.g + 0.114f * node.TitleColor.b;
                    NodeLabel.style.color = lum > 0.5f && node.TitleColor.a > 0.5f ? Color.black : Color.white * 0.9f;
                    break;
                }
                case ConstValues.NODE_TOOLTIP_NAME:
                {
                    this.tooltip = node.Tooltip;
                    break;
                }
            }
        }

        #endregion
        
        protected void PortChanged()
        {
            RefreshPorts();
            RefreshPortContainer();
            RefreshControls();
            RefreshContentsHorizontalDivider();
        }

        private void AddPortView(PortProcessor port)
        {
            var portView = NewPortView(port);
            if (port.Name == ConstValues.FLOW_IN_PORT_NAME)
            {
                titleInputPortContainer.Add(portView);
            }
            else if (port.Name == ConstValues.FLOW_OUT_PORT_NAME)
            {
                titleOutputPortContainer.Add(portView);
            }
            else
            {
                switch (port.Direction)
                {
                    case BasePort.Direction.Left:
                    {
                        inputContainer.Add(portView);
                        break;
                    }
                    case BasePort.Direction.Top:
                    {
                        topPortContainer.Add(portView);
                        break;
                    }
                    case BasePort.Direction.Right:
                    {
                        outputContainer.Add(portView);
                        break;
                    }
                    case BasePort.Direction.Bottom:
                    {
                        bottomPortContainer.Add(portView);
                        break;
                    }
                }
            }
            portView.SetUp(port, Owner);
            portViews[port.Name] = portView;
        }

        private void RemovePortView(PortProcessor port)
        {
            portViews[port.Name].UnInit();
            portViews[port.Name].RemoveFromHierarchy();
            portViews.Remove(port.Name);
        }

        private void RefreshContentsHorizontalDivider()
        {
            if (inputContainer.childCount > 0 || outputContainer.childCount > 0 || CheckDrawControls())
                horizontalDivider.RemoveFromClassList("hidden");
            else
                horizontalDivider.AddToClassList("hidden");

            if (inputContainer.childCount > 0 || outputContainer.childCount > 0)
                verticalDivider.RemoveFromClassList("hidden");
            else
                verticalDivider.AddToClassList("hidden");
        }

        private void RefreshPortContainer()
        {
            if (topPortContainer.childCount > 0)
                topPortContainer.RemoveFromClassList("hidden");
            else
                topPortContainer.AddToClassList("hidden");

            if (bottomPortContainer.childCount > 0)
                bottomPortContainer.RemoveFromClassList("hidden");
            else
                bottomPortContainer.AddToClassList("hidden");

            if (titleInputPortContainer.childCount > 0)
                titleInputPortContainer.RemoveFromClassList("hidden");
            else
                titleInputPortContainer.AddToClassList("hidden");

            if (titleOutputPortContainer.childCount > 0)
                titleOutputPortContainer.RemoveFromClassList("hidden");
            else
                titleOutputPortContainer.AddToClassList("hidden");
        }

        private void RefreshControls()
        {
            if (CheckDrawControls())
                controls.RemoveFromClassList("hidden");
            else
                controls.AddToClassList("hidden");
        }
    }
}
#endif