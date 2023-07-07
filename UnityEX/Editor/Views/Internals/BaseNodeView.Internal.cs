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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseNodeView : NodeView, IGraphElementView<BaseNodeVM>
    {
        #region 字段
        
        public readonly Label nodeLabel;
        public readonly Image nodeIcon;
        public readonly VisualElement contents;
        public readonly VisualElement controls;
        public readonly VisualElement nodeBorder;
        public readonly VisualElement topPortContainer;
        public readonly VisualElement bottomPortContainer;
        public readonly VisualElement horizontalDivider;
        public readonly VisualElement verticalDivider;

        List<IconBadge> badges = new List<IconBadge>();
        Dictionary<string, BasePortView> portViews = new Dictionary<string, BasePortView>();
        #endregion

        #region 属性
        
        public Label NodeLabel
        {
            get
            {
                return nodeLabel;
            }
        }
        
        public Image NodeIcon
        {
            get
            {
                return nodeIcon;
            }
        }

        public BaseGraphView Owner
        {
            get;
            private set;
        }
        public BaseNodeVM ViewModel
        {
            get;
            protected set;
        }
        public IReadOnlyDictionary<string, BasePortView> PortViews
        {
            get { return portViews; }
        }
        #endregion

        public BaseNodeView()
        {
            styleSheets.Add(GraphProcessorStyles.BaseNodeViewStyle);
            
            contents = mainContainer.Q("contents");
            
            nodeBorder = this.Q(name: "node-border");
            nodeLabel = titleContainer.Q<Label>("title-label");
            horizontalDivider = this.Q(name: "divider", className: "horizontal");
            verticalDivider = topContainer.Q(name: "divider", className: "vertical");

            nodeIcon = new Image() { name = "title-icon" };
            titleContainer.Insert(0, nodeIcon);

            controls = new VisualElement { name = "controls" };
            contents.Add(controls);

            topPortContainer = new VisualElement { name = "top-input" };
            nodeBorder.Insert(0, topPortContainer);

            bottomPortContainer = new VisualElement { name = "bottom-input" };
            nodeBorder.Add(bottomPortContainer);
        }

        #region Initialize
        public void SetUp(BaseNodeVM node, BaseGraphView graphView)
        {
            ViewModel = node;
            Owner = graphView;

            // 初始化
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            title = ViewModel.Title;
            tooltip = ViewModel.Tooltip;
            if (ViewModel.ContainsKey(BaseNodeVM.TITLE_COLOR_NAME))
            {
                var color = ViewModel.TitleColor.ToColor();
                var lum = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
                NodeLabel.style.color = lum > 0.5f && ViewModel.TitleColor.a > 0.5f ? Color.black : Color.white * 0.9f;
                titleContainer.style.backgroundColor = color;
            }

            foreach (var port in ViewModel.InPorts)
            {
                BasePortView portView = NewPortView(port);
                portView.SetUp(port, Owner);
                portViews[port.Name] = portView;
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

            foreach (var port in ViewModel.OutPorts)
            {
                BasePortView portView = NewPortView(port);
                portView.SetUp(port, Owner);
                portViews[port.Name] = portView;
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
            
            RefreshPorts();
            RefreshContentsHorizontalDivider();
            RefreshPortContainer();
            OnInitialized();
        }

        public void OnInitialize()
        {
            ViewModel.BindingProperty<InternalVector2Int>(nameof(BaseNode.position), OnPositionChanged);
            ViewModel.BindingProperty<string>(BaseNodeVM.TITLE_NAME, OnTitleChanged);
            if (ViewModel.ContainsKey(BaseNodeVM.TITLE_COLOR_NAME))
                ViewModel.BindingProperty<InternalColor>(BaseNodeVM.TITLE_COLOR_NAME, OnTitleColorChanged);
            ViewModel.BindingProperty<string>(BaseNodeVM.TOOLTIP_NAME, OnTooltipChanged);

            ViewModel.onPortAdded += OnPortAdded;
            ViewModel.onPortRemoved += OnPortRemoved;

            foreach (var portView in portViews.Values)
            {
                portView.OnInitialize();
            }

            OnBindingProperties();
        }

        public void OnDestroy()
        {
            ViewModel.UnBindingProperty<string>(BaseNodeVM.TITLE_NAME, OnTitleChanged);
            if (ViewModel.ContainsKey(BaseNodeVM.TITLE_COLOR_NAME))
                ViewModel.UnBindingProperty<InternalColor>(BaseNodeVM.TITLE_COLOR_NAME, OnTitleColorChanged);
            ViewModel.UnBindingProperty<string>(BaseNodeVM.TOOLTIP_NAME, OnTooltipChanged);
            ViewModel.UnBindingProperty<InternalVector2Int>(nameof(BaseNode.position), OnPositionChanged);

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
        void OnPortAdded(BasePortVM port)
        {
            AddPortView(port);
            RefreshPorts();
            RefreshContentsHorizontalDivider();
            RefreshPortContainer();
        }

        void OnPortRemoved(BasePortVM port)
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

        void AddPortView(BasePortVM port)
        {
            BasePortView portView = NewPortView(port);
            portView.SetUp(port, Owner);
            portView.OnInitialize();
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

        void RemovePortView(BasePortVM port)
        {
            portViews[port.Name].RemoveFromHierarchy();
            portViews[port.Name].OnDestroy();
            portViews.Remove(port.Name);
        }

        void RefreshContentsHorizontalDivider()
        {
            if (portViews.Values.FirstOrDefault(port => port.orientation == Orientation.Horizontal) != null)
                horizontalDivider.RemoveFromClassList("hidden");
            else
                horizontalDivider.AddToClassList("hidden");
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
        }
    }
}
#endif