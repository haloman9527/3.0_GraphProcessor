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
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using NodeView = UnityEditor.Experimental.GraphView.Node;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class BaseNodeView : NodeView, IBindableView<BaseNode>
    {
        Label titleLabel;
        [NonSerialized]
        List<IconBadge> badges = new List<IconBadge>();

        public Label TitleLabel
        {
            get
            {
                if (titleLabel == null)
                    titleLabel = titleContainer.Q<Label>("title-label");
                return titleLabel;
            }
        }

        public Image icon { get; }
        public VisualElement nodeBorder { get; }
        public VisualElement topPortContainer { get; }
        public VisualElement bottomPortContainer { get; }
        public VisualElement controlsContainer { get; }
        public VisualElement inputContainerElement { get; }
        public VisualElement contentsHorizontalDivider { get; }
        public VisualElement portsVerticalDivider { get; }
        public VisualElement controlsHorizontalDivider { get; }

        public BaseGraphView Owner { get; private set; }
        public CommandDispatcher CommandDispatcher { get; private set; }
        public Dictionary<string, NodePortView> PortViews { get; } = new Dictionary<string, NodePortView>();

        public BaseNode Model { get; protected set; }

        #region Initialization

        public BaseNodeView()
        {
            styleSheets.Add(GraphProcessorStyles.BaseNodeViewStyle);
            styleSheets.Add(GraphProcessorStyles.PortViewTypesStyle);

            icon = new Image();
            icon.style.alignSelf = Align.Center;
            titleContainer.Insert(titleContainer.IndexOf(TitleLabel), icon);

            nodeBorder = this.Q(name: "node-border");

            contentsHorizontalDivider = contentContainer.Q(name: "divider", className: "horizontal");
            contentsHorizontalDivider.AddToClassList("contents-horizontal-divider");
            contentsHorizontalDivider.style.backgroundColor = Color.green;

            portsVerticalDivider = topContainer.Q(name: "divider", className: "vertical");
            portsVerticalDivider.AddToClassList("ports-vertical-divider");
            portsVerticalDivider.style.backgroundColor = Color.red;

            controlsContainer = new VisualElement { name = "controls" };
            controlsContainer.AddToClassList("node-controls");
            controlsContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
            mainContainer.Add(controlsContainer);

            controlsHorizontalDivider = new VisualElement() { name = "divider" };
            controlsHorizontalDivider.style.height = 1;
            controlsHorizontalDivider.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
            controlsHorizontalDivider.StretchToParentWidth();
            controlsHorizontalDivider.AddToClassList("horizontal");
            controlsHorizontalDivider.AddToClassList("controls-horizontal-divider");
            controlsHorizontalDivider.style.backgroundColor = Color.blue;
            controlsContainer.Add(controlsHorizontalDivider);

            topPortContainer = new VisualElement { name = "top-port-container" };
            topPortContainer.style.justifyContent = Justify.Center;
            topPortContainer.style.alignItems = Align.Center;
            topPortContainer.style.flexDirection = FlexDirection.Row;
            Insert(0, topPortContainer);

            bottomPortContainer = new VisualElement { name = "bottom-port-container" };
            bottomPortContainer.style.justifyContent = Justify.Center;
            bottomPortContainer.style.alignItems = Align.Center;
            bottomPortContainer.style.flexDirection = FlexDirection.Row;
            Add(bottomPortContainer);

            inputContainerElement = new VisualElement { name = "input-container" };
            inputContainerElement.pickingMode = PickingMode.Ignore;
            inputContainerElement.SendToBack();
            Add(inputContainerElement);

            TitleLabel.style.flexWrap = Wrap.Wrap;
        }

        public void SetUp(BaseNode _nodeViewModel, CommandDispatcher _commandDispatcher, BaseGraphView _graphView)
        {
            Model = _nodeViewModel;
            CommandDispatcher = _commandDispatcher;
            Owner = _graphView;

            // 绑定
            BindingProperties();

            InitializePorts();
            RefreshPorts();

            foreach (var fieldInfo in Model.GetNodeFieldInfos())
            {
                // 如果不是接口，跳过
                if (!PortViews.TryGetValue(fieldInfo.Name, out NodePortView portView)) continue;
                if (portView.direction != Direction.Input) continue;
                if (portView.orientation != Orientation.Horizontal) continue;

                var box = new VisualElement { name = fieldInfo.Name };
                box.AddToClassList("port-input-element");
                if (Utility_Attribute.TryGetFieldInfoAttribute(fieldInfo, out ShowAsDrawer showAsDrawer))
                {
                    BindableElement fieldDrawer = UIElementsFactory.CreateField(String.Empty, fieldInfo.FieldType, Model.GetFieldInfoValue(fieldInfo), (newValue) =>
                    {
                        IBindableProperty property;
                        if (!string.IsNullOrEmpty(showAsDrawer.targetBindablePropertyName) && (property = Model[showAsDrawer.targetBindablePropertyName]) != null)
                        {
                            property.ValueBoxed = newValue;
                            Owner.SetDirty();
                        }
                    });
                    if (fieldDrawer != null)
                    {
                        box.Add(fieldDrawer);
                        box.visible = !portView.Model.IsConnected;
                        portView.onConnected += () => { box.visible = false; };
                        portView.onDisconnected += () => { box.visible = !portView.connected; };
                    }
                }
                else
                {
                    box.visible = false;
                    box.style.height = portView.style.height;
                }
                inputContainerElement.Add(box);
            }
        }

        #region 数据监听回调
        void OnExpandedChanged(bool _expanded)
        {
            expanded = _expanded;
            inputContainerElement.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
            Owner.SetDirty();
        }
        void OnTitleChanged(string _title)
        {
            title = _title;
        }
        void OnIconChanged(Texture _icon)
        {
            if (_icon != null)
            {
                icon.style.display = DisplayStyle.Flex;
                icon.image = _icon;
                icon.style.width = Model.IconSize.x;
                icon.style.height = Model.IconSize.y;
            }
            else
            {
                icon.style.display = DisplayStyle.None;
            }
        }
        void OnIconSizeChanged(Vector2 _size)
        {
            icon.style.width = _size.x;
            icon.style.height = _size.y;
        }
        void OnTooltipChanged(string _tooltip)
        {
            tooltip = _tooltip;
        }
        void OnPositionChanged(Vector2 _position)
        {
            base.SetPosition(new Rect(_position, GetPosition().size));
            Owner.SetDirty();
        }
        void OnTitleColorChanged(Color _color)
        {
            titleContainer.style.backgroundColor = _color;
            TitleLabel.style.color = _color.GetLuminance() > 0.5f && _color.a > 0.5f ? Color.black : Color.white * 0.9f;
        }
        protected virtual void BindingProperties()
        {
            // 初始化
            base.expanded = Model.Expanded;
            title = Model.Title;
            OnIconChanged(Model.Icon);
            tooltip = Model.Tooltip; base.SetPosition(new Rect(Model.Position, GetPosition().size));
            titleContainer.style.backgroundColor = Model.TitleColor;
            TitleLabel.style.color = Model.TitleColor.GetLuminance() > 0.5f && Model.TitleColor.a > 0.5f ? Color.black : Color.white * 0.9f;


            Model.BindingProperty<bool>(nameof(Model.Expanded), OnExpandedChanged);
            Model.BindingProperty<string>(nameof(Model.Title), OnTitleChanged);
            Model.Title = GraphProcessorEditorUtility.GetNodeDisplayName(Model.GetType());
            Model.BindingProperty<Texture>(nameof(Model.Icon), OnIconChanged);
            Model.BindingProperty<Vector2>(nameof(Model.IconSize), OnIconSizeChanged);
            Model.BindingProperty<string>(nameof(Model.Tooltip), OnTooltipChanged);
            Model.BindingProperty<Vector2>(nameof(Model.Position), OnPositionChanged);
            Model.BindingProperty<Color>(nameof(Model.TitleColor), OnTitleColorChanged);
        }

        public virtual void UnBindingProperties()
        {
            foreach (var portView in PortViews.Values)
            {
                portView.UnBindingProperties();
            }
            Model.UnBindingProperty<bool>(nameof(Model.Expanded), OnExpandedChanged);
            Model.UnBindingProperty<string>(nameof(Model.Title), OnTitleChanged);
            Model.UnBindingProperty<Texture>(nameof(Model.Icon), OnIconChanged);
            Model.UnBindingProperty<Vector2>(nameof(Model.IconSize), OnIconSizeChanged);
            Model.UnBindingProperty<string>(nameof(Model.Tooltip), OnTooltipChanged);
            Model.UnBindingProperty<Vector2>(nameof(Model.Position), OnPositionChanged);
            Model.UnBindingProperty<Color>(nameof(Model.TitleColor), OnTitleColorChanged);
        }
        #endregion

        void InitializePorts()
        {
            foreach (var nodePort in Model.Ports)
            {
                Direction direction = nodePort.Value.Direction == PortDirection.Input ? Direction.Input : Direction.Output;
                Orientation orientation =
                    Utility_Attribute.TryGetFieldAttribute(Model.GetType(), nodePort.Value.FieldName, out VerticalAttribute vertical) ?
                    Orientation.Vertical : Orientation.Horizontal;

                NodePortView portView = CustomCreatePortView(orientation, direction, nodePort.Value);
                if (portView == null)
                    portView = NodePortView.CreatePV(orientation, direction, nodePort.Value);
                portView.SetUp(nodePort.Value, CommandDispatcher, Owner);
                PortViews[nodePort.Key] = portView;
            }
        }

        public void Initialized()
        {
            base.expanded = Model.Expanded;
            //foreach (var item in ViewModel.BindableProperties)
            //{
            //    BindableElement element = UIElementsFactory.CreateField(item.Key, item.Value.ValueType, item.Value.ValueBoxed, newValue =>
            //    {
            //        item.Value.ValueBoxed = newValue;
            //    });
            //    controlsContainer.Add(element);
            //}
            //foreach (var fieldInfo in Utility_Reflection.GetFieldInfos(ViewModel.ModelType))
            //{
            //    if (!EditorGUILayoutExtension.CanDraw(fieldInfo)) continue;
            //    if (PortViews.TryGetValue(fieldInfo.Name, out BasePortView portView) && portView.direction == Direction.Input) continue;
            //    if (fieldInfo.FieldType != typeof(string) && !fieldInfo.FieldType.IsValueType && fieldInfo.GetValue(NodeData) == null)
            //        fieldInfo.SetValue(NodeData, Activator.CreateInstance(fieldInfo.FieldType));

            //    string label = NodeEditorUtility.GetDisplayName(fieldInfo.Name);
            //    if (Utility_Attribute.TryGetFieldInfoAttribute(fieldInfo, out InspectorNameAttribute displayName))
            //        label = displayName.displayName;
            //    BindableElement element = UIElementsFactory.CreateField(label, fieldInfo.FieldType, fieldInfo.GetValue(NodeData), newValue =>
            //    {
            //        fieldInfo.SetValue(NodeData, newValue);
            //    });
            //    element.MarkDirtyRepaint();
            //    controlsContainer.Add(element);
            //}
            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }
        #endregion

        #region API
        public void HighlightOn()
        {
            nodeBorder.AddToClassList("highlight");
        }
        public void HighlightOff()
        {
            nodeBorder.RemoveFromClassList("highlight");
        }

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
                capabilities |= Capabilities.Movable;
            else
                capabilities &= ~Capabilities.Movable;
        }

        public void SetSelectable(bool selectable)
        {
            if (selectable)
                capabilities |= Capabilities.Selectable;
            else
                capabilities &= ~Capabilities.Selectable;
        }

        public void AddBadge(IconBadge badge)
        {
            Add(badge);
            badges.Add(badge);
            badge.AttachTo(topContainer, SpriteAlignment.RightCenter);
        }

        public void RemoveBadge(Func<IconBadge, bool> callback)
        {
            badges.RemoveAll(b =>
            {
                if (callback(b))
                {
                    b.Detach();
                    b.RemoveFromHierarchy();
                    return true;
                }
                return false;
            });
        }

        #endregion

        #region ContextMenu
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (var script in EditorUtilityExtension.FindAllScriptFromType(Model.GetType()))
            {
                evt.menu.AppendAction($"Open Script/" + script.name, _ => { AssetDatabase.OpenAsset(script); });
            }
            //evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
            evt.menu.AppendAction(Model.Locked ? "Unlock" : "Lock", (e) => ChangeLockStatus(), Status.Normal);
            evt.menu.AppendSeparator();
        }

        void OpenNodeViewScript()
        {
            var script = EditorUtilityExtension.FindScriptFromType(GetType(), s => s.name.Contains("View"));

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (EditorUtilityExtension.FindScriptFromType(GetType(), s => s.name.Contains("View")) != null)
                return Status.Normal;
            return Status.Disabled;
        }
        #endregion

        #region Callbacks & Overrides
        protected virtual NodePortView CustomCreatePortView(Orientation _orientation, Direction _direction, NodePort _nodePort)
        {
            return null;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();
        }

        protected override void ToggleCollapse()
        {
            Model.Expanded = !expanded;
        }

        protected VisualElement CreateControlField(FieldInfo _fieldInfo, string _label = null, Action<object> _valueChangedCallback = null)
        {
            if (_fieldInfo == null)
                return null;

            var fieldDrawer = UIElementsFactory.CreateField(_label, _fieldInfo.FieldType, Model.GetFieldInfoValue(_fieldInfo), (newValue) =>
             {
                 Model.SetFieldInfoValue(_fieldInfo, newValue);
                 _valueChangedCallback?.Invoke(newValue);
                 Owner.SetDirty();
             });

            return fieldDrawer;
        }

        public override void SetPosition(Rect newPos)
        {
            Model.Position = newPos.position;
        }

        public void ChangeLockStatus()
        {
            Model.Locked ^= true;
            SetMovable(!Model.Locked);
        }

        public virtual new bool RefreshPorts()
        {
            foreach (var ipv in PortViews.Values)
            {
                if (!(ipv is NodePortView portView))
                    continue;
                switch (portView.direction)
                {
                    case Direction.Input:
                        if (portView.orientation == Orientation.Horizontal)
                            inputContainer.Add(portView);
                        else
                            topPortContainer.Add(portView);
                        break;
                    case Direction.Output:
                        if (portView.orientation == Orientation.Horizontal)
                            outputContainer.Add(portView);
                        else
                            bottomPortContainer.Add(portView);
                        break;
                    default:
                        break;
                }
            }
            return base.RefreshPorts();
        }
        #endregion
    }

    public abstract class BaseNodeView<M> : BaseNodeView where M : BaseNode
    {
        public M T_Model { get { return Model as M; } }
    }
}