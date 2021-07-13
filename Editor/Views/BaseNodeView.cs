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
    public abstract class BaseNodeView : NodeView
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
            CommandDispatcher = _commandDispatcher;
            Owner = _graphView;

            Model = _nodeViewModel;
            BindingProperties();
            Model.UpdateProperties();
            Model.RegisterValueChangedEvent<Color>(nameof(Model.TitleTint), v =>
            {
                titleContainer.style.backgroundColor = v;
                TitleLabel.style.color = v.GetLuminance() > 0.5f && v.a > 0.5f ? Color.black : Color.white * 0.9f;
            });

            InitializePorts();
            RefreshPorts();

            //foreach (var fieldInfo in ViewModel.GetNodeFieldInfos())
            //{
            //    // 如果不是接口，跳过
            //    if (!PortViews.TryGetValue(fieldInfo.Name, out NodePortView portView)) continue;
            //    if (portView.direction != Direction.Input) continue;
            //    if (portView.orientation != Orientation.Horizontal) continue;

            //    var box = new VisualElement { name = fieldInfo.Name };
            //    box.AddToClassList("port-input-element");
            //    if (Utility_Attribute.TryGetFieldInfoAttribute(fieldInfo, out ShowAsDrawer showAsDrawer))
            //    {
            //        VisualElement fieldDrawer = CreateControlField(fieldInfo, string.Empty, null);
            //        if (fieldDrawer != null)
            //        {
            //            box.Add(fieldDrawer);
            //            box.visible = !portView.ViewModel.IsConnected;
            //            portView.onConnected += () => { box.visible = false; };
            //            portView.onDisconnected += () => { box.visible = !portView.connected; };
            //        }
            //    }
            //    else
            //    {
            //        box.visible = false;
            //        box.style.height = portView.style.height;
            //    }
            //    inputContainerElement.Add(box);
            //}
        }


        protected virtual void BindingProperties()
        {
            Model.RegisterValueChangedEvent<bool>(nameof(Model.Expanded), v =>
            {
                expanded = v;
                inputContainerElement.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;

            });
            Model.RegisterValueChangedEvent<string>(nameof(Model.Title), v =>
            {
                title = v;
            });
            Model.Title = GraphProcessorEditorUtility.GetNodeDisplayName(Model.GetType());
            Model.RegisterValueChangedEvent<Texture>(nameof(Model.Icon), v =>
            {
                if (v != null)
                {
                    icon.style.display = DisplayStyle.Flex;
                    icon.image = v;
                    icon.style.width = Model.IconSize.x;
                    icon.style.height = Model.IconSize.y;
                }
                else
                {
                    icon.style.display = DisplayStyle.None;
                }
            });
            Model.RegisterValueChangedEvent<Vector2>(nameof(Model.IconSize), v =>
            {
                icon.style.width = v.x;
                icon.style.height = v.y;
            });
            Model.RegisterValueChangedEvent<string>(nameof(Model.Tooltip), v =>
            {
                tooltip = v;
            });
            Model.RegisterValueChangedEvent<Vector2>(nameof(Model.Position), v =>
            {
                base.SetPosition(new Rect(v, GetPosition().size));
            });
        }

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
            Model.UpdateExpanded();
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
            evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
            evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
            evt.menu.AppendAction(Model.Locked ? "Unlock" : "Lock", (e) => ChangeLockStatus(), Status.Normal);
            evt.menu.AppendSeparator();
        }

        void OpenNodeScript()
        {
            var script = EditorUtilityExtension.FindScriptFromType(Model.GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        void OpenNodeViewScript()
        {
            var script = EditorUtilityExtension.FindScriptFromType(GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        Status OpenNodeScriptStatus(DropdownMenuAction action)
        {
            if (EditorUtilityExtension.FindScriptFromType(Model.GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (EditorUtilityExtension.FindScriptFromType(GetType()) != null)
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

    public sealed class DefaultNodeView : BaseNodeView<BaseNode>
    {

    }
}