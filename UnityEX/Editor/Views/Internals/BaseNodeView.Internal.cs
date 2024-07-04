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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace CZToolKit.GraphProcessor.Editors
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

        List<IconBadge> badges = new List<IconBadge>();
        Dictionary<string, BasePortView> portViews = new Dictionary<string, BasePortView>();

        #endregion

        #region 属性

        public Label NodeLabel
        {
            get { return nodeLabel; }
        }

        public Image NodeIcon
        {
            get { return nodeIcon; }
        }

        public BaseGraphView Owner { get; private set; }
        public BaseNodeProcessor ViewModel { get; protected set; }

        public IReadOnlyDictionary<string, BasePortView> PortViews
        {
            get { return portViews; }
        }

        #endregion

        public BaseNodeView()
        {
            styleSheets.Add(GraphProcessorStyles.BaseNodeViewStyle);

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
        }

        #region Initialize

        public void SetUp(BaseNodeProcessor node, BaseGraphView graphView)
        {
            ViewModel = node;
            Owner = graphView;

            // 初始化
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            title = ViewModel.Title;
            tooltip = ViewModel.Tooltip;
            if (ViewModel.Contains(BaseNodeProcessor.TITLE_COLOR_NAME))
            {
                var color = ViewModel.TitleColor.ToColor();
                var lum = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
                NodeLabel.style.color = lum > 0.5f && ViewModel.TitleColor.a > 0.5f ? Color.black : Color.white * 0.9f;
                titleContainer.style.backgroundColor = color;
            }

            foreach (var port in ViewModel.LeftPorts)
            {
                var portView = NewPortView(port);
                portView.SetUp(port, Owner);
                portViews[port.Name] = portView;
                if (port.Name.StartsWith("Flow-"))
                {
                    titleInputPortContainer.Add(portView);
                }
                else
                {
                    switch (port.Orientation)
                    {
                        case BasePort.Orientation.Horizontal:
                        {
                            inputContainer.Add(portView);
                            break;
                        }
                        case BasePort.Orientation.Vertical:
                        {
                            topPortContainer.Add(portView);
                            break;
                        }
                    }
                }
            }

            foreach (var port in ViewModel.RightPorts)
            {
                BasePortView portView = NewPortView(port);
                portView.SetUp(port, Owner);
                portViews[port.Name] = portView;

                if (port.Name.StartsWith("Flow-"))
                {
                    titleOutputPortContainer.Add(portView);
                }
                else
                {
                    switch (port.Orientation)
                    {
                        case BasePort.Orientation.Horizontal:
                        {
                            outputContainer.Add(portView);
                            break;
                        }
                        case BasePort.Orientation.Vertical:
                        {
                            bottomPortContainer.Add(portView);
                            break;
                        }
                    }
                }
            }

            OnInitialized();
            
            RefreshPorts();
            RefreshPortContainer();
            RefreshControls();
            RefreshContentsHorizontalDivider();
        }

        private void OnChildChanged(BaseVisualElement.ChildChangedEvent evt)
        {
            RefreshControls();
            RefreshContentsHorizontalDivider();
        }

        public void OnCreate()
        {
            ViewModel.BindProperty<InternalVector2Int>(nameof(BaseNode.position), OnPositionChanged);
            ViewModel.BindProperty<string>(BaseNodeProcessor.TITLE_NAME, OnTitleChanged);
            if (ViewModel.Contains(BaseNodeProcessor.TITLE_COLOR_NAME))
                ViewModel.BindProperty<InternalColor>(BaseNodeProcessor.TITLE_COLOR_NAME, OnTitleColorChanged);
            ViewModel.BindProperty<string>(BaseNodeProcessor.TOOLTIP_NAME, OnTooltipChanged);

            ViewModel.onPortAdded += OnPortAdded;
            ViewModel.onPortRemoved += OnPortRemoved;

            foreach (var portView in portViews.Values)
            {
                portView.OnCreate();
            }

            OnBindingProperties();
        }

        public void OnDestroy()
        {
            ViewModel.UnBindProperty<string>(BaseNodeProcessor.TITLE_NAME, OnTitleChanged);
            if (ViewModel.Contains(BaseNodeProcessor.TITLE_COLOR_NAME))
                ViewModel.UnBindProperty<InternalColor>(BaseNodeProcessor.TITLE_COLOR_NAME, OnTitleColorChanged);
            ViewModel.UnBindProperty<string>(BaseNodeProcessor.TOOLTIP_NAME, OnTooltipChanged);
            ViewModel.UnBindProperty<InternalVector2Int>(nameof(BaseNode.position), OnPositionChanged);

            ViewModel.onPortAdded -= OnPortAdded;
            ViewModel.onPortRemoved -= OnPortRemoved;

            foreach (var portView in portViews.Values)
            {
                portView.OnDestroy();
            }

            OnUnBindingProperties();
        }

        #endregion

        #region Callbacks

        void OnPortAdded(BasePortProcessor port)
        {
            AddPortView(port);
            RefreshPorts();
            RefreshContentsHorizontalDivider();
            RefreshPortContainer();
        }

        void OnPortRemoved(BasePortProcessor port)
        {
            RemovePortView(port);
            RefreshPorts();
            RefreshContentsHorizontalDivider();
            RefreshPortContainer();
        }

        void OnTitleChanged(string oldTitle, string newTitle)
        {
            base.title = newTitle;
        }

        void OnTooltipChanged(string oldTooltip, string newTooltip)
        {
            this.tooltip = newTooltip;
        }

        void OnPositionChanged(InternalVector2Int oldPosition, InternalVector2Int newPosition)
        {
            base.SetPosition(new Rect(newPosition.ToVector2(), GetPosition().size));
            Owner.SetDirty();
        }

        void OnTitleColorChanged(InternalColor oldColor, InternalColor color)
        {
            titleContainer.style.backgroundColor = color.ToColor();
            var lum = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
            NodeLabel.style.color = lum > 0.5f && color.a > 0.5f ? Color.black : Color.white * 0.9f;
        }

        #endregion

        public void SetDeletable(bool deletable)
        {
            if (deletable)
                capabilities |= Capabilities.Deletable;
            else
                capabilities &= ~Capabilities.Deletable;
        }

        public void SetMovable(bool movable)
        {
            if (movable)
                capabilities = capabilities | Capabilities.Movable;
            else
                capabilities = capabilities & (~Capabilities.Movable);
        }

        public void SetSelectable(bool selectable)
        {
            if (selectable)
                capabilities |= Capabilities.Selectable;
            else
                capabilities &= ~Capabilities.Selectable;
        }

        protected void PortChanged()
        {
            RefreshPorts();
            RefreshPortContainer();
            RefreshContentsHorizontalDivider();
        }

        void AddPortView(BasePortProcessor port)
        {
            BasePortView portView = NewPortView(port);
            portView.SetUp(port, Owner);
            portView.OnCreate();
            portViews[port.Name] = portView;

            if (portView.orientation == Orientation.Horizontal)
            {
                if (portView.direction == Direction.Input)
                    inputContainer.Add(portView);
                else
                    outputContainer.Add(portView);
            }
            else
            {
                if (portView.direction == Direction.Input)
                    topPortContainer.Add(portView);
                else
                    bottomPortContainer.Add(portView);
            }
        }

        void RemovePortView(BasePortProcessor port)
        {
            portViews[port.Name].RemoveFromHierarchy();
            portViews[port.Name].OnDestroy();
            portViews.Remove(port.Name);
        }

        void RefreshContentsHorizontalDivider()
        {
            if (inputContainer.childCount > 0 || outputContainer.childCount > 0 || controls.childCount > 0)
                horizontalDivider.RemoveFromClassList("hidden");
            else
                horizontalDivider.AddToClassList("hidden");
            
            if (inputContainer.childCount > 0 || outputContainer.childCount > 0)
                verticalDivider.RemoveFromClassList("hidden");
            else
                verticalDivider.AddToClassList("hidden");
        }

        void RefreshPortContainer()
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
        
        void RefreshControls()
        {
            if (controls.childCount > 0)
                controls.RemoveFromClassList("hidden");
            else
                controls.AddToClassList("hidden");
        }
    }
}
#endif