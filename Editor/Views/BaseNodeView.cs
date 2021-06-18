using CZToolKit.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
using NodeView = UnityEditor.Experimental.GraphView.Node;
using CZToolKit.Core.Editors;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseNodeView : NodeView, INodeView
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

        public VisualElement topPortContainer { get; }
        public VisualElement bottomPortContainer { get; }
        public VisualElement controlsContainer { get; }
        public VisualElement inputContainerElement { get; }
        public VisualElement contentHorizontalDivider { get; }
        public VisualElement portsVerticalDivider { get; }
        public VisualElement controlsHorizontalDivider { get; }

        public bool Initialized { get; private set; }
        public override bool expanded
        {
            get { return base.expanded; }
            set
            {
                base.expanded = value;
                if (Initialized)
                    NodeData.Expanded = value;
            }
        }
        public bool Lockable { get; private set; }
        public BaseGraphView Owner { get; private set; }
        public CommandDispatcher CommandDispatcher { get; private set; }
        public BaseNode NodeData { get; private set; }
        public Type NodeDataType { get; private set; }
        public Dictionary<string, PortView> PortViews { get; private set; } = new Dictionary<string, PortView>();
        protected List<FieldInfo> NodeDataTypeFieldInfos
        {
            get { return Utility_Refelection.GetFieldInfos(NodeDataType); }
        }

        #region  Initialization

        public BaseNodeView()
        {
            styleSheets.Add(GraphProcessorStyles.BaseNodeViewStyle);
            styleSheets.Add(GraphProcessorStyles.PortViewTypesStyle);

            contentHorizontalDivider = contentContainer.Q(name: "divider", className: "horizontal");
            portsVerticalDivider = topContainer.Q(name: "divider", className: "vertical");

            controlsContainer = new VisualElement { name = "Controls" };
            controlsContainer.AddToClassList("NodeControls");
            controlsContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
            mainContainer.Add(controlsContainer);

            topPortContainer = new VisualElement { name = "TopPortContainer" };
            topPortContainer.style.justifyContent = Justify.Center;
            topPortContainer.style.alignItems = Align.Center;
            topPortContainer.style.flexDirection = FlexDirection.Row;
            Insert(0, topPortContainer);

            bottomPortContainer = new VisualElement { name = "BottomPortContainer" };
            bottomPortContainer.style.justifyContent = Justify.Center;
            bottomPortContainer.style.alignItems = Align.Center;
            bottomPortContainer.style.flexDirection = FlexDirection.Row;
            Add(bottomPortContainer);

            inputContainerElement = new VisualElement { name = "input-container" };
            inputContainerElement.pickingMode = PickingMode.Ignore;
            inputContainerElement.SendToBack();
            Add(inputContainerElement);

            controlsHorizontalDivider = new VisualElement() { name = "divider" };
            controlsHorizontalDivider.AddToClassList("horizontal");
            controlsHorizontalDivider.style.height = 1;
            controlsHorizontalDivider.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
            controlsHorizontalDivider.StretchToParentWidth();
            controlsContainer.Add(controlsHorizontalDivider);


            contentHorizontalDivider.style.backgroundColor = Color.green;
            portsVerticalDivider.style.backgroundColor = Color.red;
            controlsHorizontalDivider.style.backgroundColor = Color.blue;
        }

        public void SetUp(IGraphElement _graphElement, CommandDispatcher _commandDispatcher, IGraphView _graphView)
        {
            if (Initialized) return;

            NodeData = _graphElement as BaseNode;
            NodeDataType = NodeData.GetType();
            CommandDispatcher = _commandDispatcher;
            Owner = _graphView as BaseGraphView;

            InitializeView();
            InitializePorts();
            RefreshPorts();
            RefreshExpandedState();

            foreach (var fieldInfo in NodeDataTypeFieldInfos)
            {
                // 如果是接口
                if (PortViews.TryGetValue(fieldInfo.Name, out PortView portView)
                && portView.orientation == Orientation.Horizontal
                && portView.direction == Direction.Input)
                {
                    var box = new VisualElement { name = fieldInfo.Name };
                    box.AddToClassList("port-input-element");
                    VisualElement fieldDrawer;
                    if (Utility_Attribute.TryGetFieldInfoAttribute(fieldInfo, out ShowAsDrawer showAsDrawer)
                        && (fieldDrawer = CreateControlField(fieldInfo, string.Empty, _ => { Owner.SetDirty(); })) != null)
                    {
                        box.Add(fieldDrawer);
                        box.visible = !portView.PortData.IsConnected;
                        portView.onConnected += () => { box.visible = false; };
                        portView.onDisconnected += () => { if (!portView.connected) box.visible = true; };
                    }
                    else
                    {
                        box.visible = false;
                        box.style.height = portView.style.height;
                    }
                    inputContainerElement.Add(box);
                }
            }

            if (!Owner.Initialized)
                Owner.onInitializeCompleted += OnInitialized;
            else
                OnInitialized();

            //expanded = NodeData.Expanded;
            //inputContainerElement.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

            Initialized = true;

            this.MarkDirtyRepaint();
        }

        void InitializeView()
        {
            title = NodeEditorUtility.GetNodeDisplayName(NodeDataType);
            TitleLabel.style.flexWrap = Wrap.Wrap;
            SetPosition(NodeData.position);
            Lockable = Utility_Attribute.TryGetTypeAttribute(NodeDataType, out LockableAttribute lockableAttribute);

            if (Utility_Attribute.TryGetTypeAttribute(NodeDataType, out NodeIconAttribute iconAttribute))
            {
                Texture icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconAttribute.iconPath);
                if (icon != null)
                    AddIcon(new Image() { image = icon, style = { width = iconAttribute.width, height = iconAttribute.height } });
            }

            if (Utility_Attribute.TryGetTypeAttribute(NodeDataType, out NodeTooltipAttribute nodeTooltipAttribute))
                tooltip = nodeTooltipAttribute.Tooltip;

            if (Utility_Attribute.TryGetTypeAttribute(NodeDataType, out NodeTitleTintAttribute nodeTitleTintAttribute))
            {
                titleContainer.style.backgroundColor = nodeTitleTintAttribute.BackgroundColor;
                TitleLabel.style.color = nodeTitleTintAttribute.BackgroundColor.GetLuminance() > 0.5f && nodeTitleTintAttribute.BackgroundColor.a > 0.5f ? Color.black : Color.white * 0.9f;
            }

            //bool showControlOnHover = Utility_Attribute.TryGetTypeAttribute(NodeDataType, out ShowControlOnHoverAttribute showControlOnHoverAttrib);
            //if (showControlOnHover)
            //{
            //    bool mouseOverControls = false;
            //    controlsContainer.style.display = DisplayStyle.None;
            //    RegisterCallback<MouseOverEvent>(e =>
            //    {
            //        controlsContainer.style.display = DisplayStyle.Flex;
            //        mouseOverControls = true;
            //    });
            //    RegisterCallback<MouseOutEvent>(e =>
            //    {
            //        var rect = GetPosition();
            //        var graphMousePosition = Owner.contentViewContainer.WorldToLocal(e.mousePosition);
            //        if (rect.Contains(graphMousePosition) || !showControlOnHover)
            //            return;
            //        mouseOverControls = false;
            //        schedule.Execute(_ =>
            //        {
            //            if (!mouseOverControls)
            //                controlsContainer.style.display = DisplayStyle.None;
            //        }).ExecuteLater(500);
            //    });
            //}

            //Undo.undoRedoPerformed += UpdateFieldValues;
        }

        void InitializePorts()
        {
            foreach (var nodePort in NodeData.Ports)
            {
                Direction direction = nodePort.Value.Direction == PortDirection.Input ? Direction.Input : Direction.Output;
                Orientation orientation =
                    Utility_Attribute.TryGetFieldAttribute(NodeDataType, nodePort.Value.FieldName, out VerticalAttribute vertical) ?
                    Orientation.Vertical : Orientation.Horizontal;

                PortView portView = CustomCreatePortView(orientation, direction, nodePort.Value);
                if (portView == null)
                    portView = PortView.CreatePV(orientation, direction, nodePort.Value);
                portView.SetUp(nodePort.Value, CommandDispatcher, Owner);
                PortViews[nodePort.Key] = portView;
            }
        }

        protected virtual void OnInitialized()
        {
            //foreach (var fieldInfo in Utility_Refelection.GetFieldInfos(NodeDataType))
            //{
            //    if (!EditorGUILayoutExtension.CanDraw(fieldInfo)) continue;
            //    if (PortViews.TryGetValue(fieldInfo.Name, out PortView portView) && portView.direction == Direction.Input) continue;
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
        }
        #endregion

        #region API

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

        public void AddIcon(Image _icon)
        {
            _icon.style.alignSelf = Align.Center;
            titleContainer.Insert(titleContainer.IndexOf(TitleLabel), _icon);
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

        #region Private
        void OpenNodeScript()
        {
            var script = EditorUtilityExtension.FindScriptFromType(NodeDataType);

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        void OpenNodeViewScript()
        {
            var script = EditorUtilityExtension.FindScriptFromType(GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }
        #endregion

        #region Callbacks & Overrides

        Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new Dictionary<string, List<(object value, VisualElement target)>>();
        Dictionary<string, VisualElement> hideElementIfConnected = new Dictionary<string, VisualElement>();
        Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new Dictionary<FieldInfo, List<VisualElement>>();

        protected virtual PortView CustomCreatePortView(Orientation _orientation, Direction _direction, NodePort _nodePort)
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
            base.ToggleCollapse();
            inputContainerElement.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected VisualElement CreateControlField(FieldInfo _fieldInfo, string _label = null, Action<object> _valueChangedCallback = null)
        {
            if (_fieldInfo == null)
                return null;

            var fieldDrawer = UIElementsFactory.CreateField(_label, _fieldInfo.FieldType, _fieldInfo.GetValue(NodeData), (newValue) =>
             {
                 Owner.RegisterCompleteObjectUndo("Updated " + newValue);
                 _fieldInfo.SetValue(NodeData, newValue);
                 _valueChangedCallback?.Invoke(newValue);
                 Owner.SetDirty();
             });

            return fieldDrawer;
        }

        public virtual void OnPortConnected(PortView _portView, PortView _targetPortView)
        {
            if (_portView.direction == Direction.Input && inputContainerElement?.Q(_portView.FieldName) != null)
                inputContainerElement.Q(_portView.FieldName).AddToClassList("empty");

            if (hideElementIfConnected.TryGetValue(_portView.FieldName, out var elem))
                elem.style.display = DisplayStyle.None;
        }

        public virtual void OnPortDisconnected(PortView _portView, PortView _targetPortView)
        {
            if (_portView.direction == Direction.Input && inputContainerElement?.Q(_portView.FieldName) != null)
                inputContainerElement.Q(_portView.FieldName).RemoveFromClassList("empty");

            if (hideElementIfConnected.TryGetValue(_portView.FieldName, out var elem))
                elem.style.display = DisplayStyle.Flex;
        }

        public override void SetPosition(Rect newPos)
        {
            if (NodeData.Locked) return;

            base.SetPosition(newPos);
            if (Initialized)
            {
                Owner.RegisterCompleteObjectUndo("Moved graph node");
                NodeData.position = newPos;
                Owner.SetDirty();
            }
        }

        public void ChangeLockStatus()
        {
            NodeData.Locked ^= true;
            SetMovable(!NodeData.Locked);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
            evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
            if (Lockable)
                evt.menu.AppendAction(NodeData.Locked ? "Unlock" : "Lock", (e) => ChangeLockStatus(), Status.Normal);
        }

        // 按钮状态
        Status OpenNodeScriptStatus(DropdownMenuAction action)
        {
            if (EditorUtilityExtension.FindScriptFromType(NodeData.GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        // 按钮状态
        Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (EditorUtilityExtension.FindScriptFromType(GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        public virtual new bool RefreshPorts()
        {
            foreach (var portView in PortViews.Values)
            {
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
}