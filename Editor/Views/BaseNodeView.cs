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

namespace GraphProcessor.Editors
{
    public class BaseNodeView : NodeView
    {
        #region Static
        const string BaseNodeViewStyle = "GraphProcessorStyles/BaseNodeView";
        #endregion

        VisualElement settings;
        VisualElement topPortContainer;
        VisualElement bottomPortContainer;
        NodeSettingsView settingsContainer;
        Label titleLabel;
        Button settingButton;

        Dictionary<string, PortView> portViews = new Dictionary<string, PortView>();
        bool settingsExpanded = false;

        [NonSerialized] List<IconBadge> badges = new List<IconBadge>();

        public VisualElement controlsContainer { get; private set; }
        protected VisualElement inputContainerElement { get; set; }
        public Label TitleLabel
        {
            get
            {
                if (titleLabel == null)
                    titleLabel = titleContainer.Q<Label>("title-label");
                return titleLabel;
            }
        }
        public Dictionary<string, PortView> PortViews { get { return portViews; } }

        public bool Initialized { get; private set; }
        public BaseGraphView Owner { get; private set; }
        public BaseNode NodeData { get; private set; }
        public Type NodeDataType { get; private set; }
        public bool Lockable { get; private set; }
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
        protected virtual bool HasSettings { get; set; }

        #region  Initialization
        public void Initialize(BaseGraphView _owner, BaseNode _nodeData)
        {
            styleSheets.Add(Resources.Load<StyleSheet>(BaseNodeViewStyle));

            Owner = _owner;
            NodeData = _nodeData;
            NodeDataType = _nodeData.GetType();
            Lockable = AttributeCache.TryGetTypeAttribute(NodeDataType, out LockableAttribute lockableAttribute);
            if (AttributeCache.TryGetTypeAttribute(NodeDataType, out NodeTooltipAttribute nodeTooltipAttribute))
                tooltip = nodeTooltipAttribute.Tooltip;
            if (AttributeCache.TryGetTypeAttribute(NodeDataType, out NodeTitleTintAttribute nodeTitleTintAttribute))
            {
                titleContainer.style.backgroundColor = nodeTitleTintAttribute.BackgroundColor;
                TitleLabel.style.color = nodeTitleTintAttribute.BackgroundColor.GetLuminance() > 0.5f && nodeTitleTintAttribute.BackgroundColor.a > 0.5f ? Color.black : Color.white * 0.9f;
            }

            InitializeView();
            InitializePorts();
            InitializeSettings();
            RefreshPorts();
            RefreshExpandedState();
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnGeometryChanged(null);

            OnInitialized();
            MarkDirtyRepaint();
        }

        protected virtual void OnInitialized()
        {
#if !ODIN_INSPECTOR
            DrawDefaultInspector();
#endif
        }

        void InitializeView()
        {
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
            mainContainer.parent.Add(inputContainerElement);
            inputContainerElement.SendToBack();
            inputContainerElement.pickingMode = PickingMode.Ignore;

            base.expanded = NodeData.Expanded;
            title = NodeEditorUtility.GetNodeDisplayName(NodeDataType);
            SetPosition(NodeData.position);

            if (AttributeCache.TryGetTypeAttribute(NodeData.GetType(), out TooltipAttribute tooltipAttrib))
                tooltip = tooltipAttrib.tooltip;

            bool showControlOnHover = AttributeCache.TryGetTypeAttribute(NodeData.GetType(), out ShowControlOnHoverAttribute showControlOnHoverAttrib);
            if (showControlOnHover)
            {
                bool mouseOverControls = false;
                controlsContainer.style.display = DisplayStyle.None;
                RegisterCallback<MouseOverEvent>(e =>
                {
                    controlsContainer.style.display = DisplayStyle.Flex;
                    mouseOverControls = true;
                });
                RegisterCallback<MouseOutEvent>(e =>
                {
                    var rect = GetPosition();
                    var graphMousePosition = Owner.contentViewContainer.WorldToLocal(e.mousePosition);
                    if (rect.Contains(graphMousePosition) || !showControlOnHover)
                        return;
                    mouseOverControls = false;
                    schedule.Execute(_ =>
                    {
                        if (!mouseOverControls)
                            controlsContainer.style.display = DisplayStyle.None;
                    }).ExecuteLater(500);
                });
            }

            //Undo.undoRedoPerformed += UpdateFieldValues;

            Initialized = true;
        }

        protected virtual PortView CustomCreatePortView(Orientation _orientation, Direction _direction, NodePort _nodePort, BaseEdgeConnectorListener _listener)
        {
            return null;
        }

        void InitializePorts()
        {
            BaseEdgeConnectorListener listener = Owner.connectorListener;

            foreach (var nodePort in NodeData.Ports)
            {
                Direction direction = nodePort.Value.Direction == PortDirection.Input ? Direction.Input : Direction.Output;
                Orientation orientation =
                    AttributeCache.TryGetFieldAttribute(NodeDataType, nodePort.Value.FieldName, out VerticalAttribute vertical) ?
                    Orientation.Vertical : Orientation.Horizontal;

                PortView portView = CustomCreatePortView(orientation, direction, nodePort.Value, listener);
                if (portView == null)
                    portView = PortView.CreatePV(orientation, direction, nodePort.Value, listener);
                portView.Initialize(this);
                PortViews[nodePort.Key] = portView;
            }
        }

        void InitializeSettings()
        {
            if (HasSettings)
            {
                CreateSettingButton();
                settingsContainer = new NodeSettingsView();
                settingsContainer.visible = false;
                settings = new VisualElement();
                // Add Node type specific settings
                settings.Add(CreateSettingsView());
                settingsContainer.Add(settings);
                Add(settingsContainer);

                var fields = NodeData.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var field in fields)
                    if (field.GetCustomAttribute(typeof(SettingAttribute)) != null)
                        AddSettingField(field);
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (settingButton != null)
            {
                var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
                settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
                settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
            }
        }

        void CreateSettingButton()
        {
            settingButton = new Button(ToggleSettings) { name = "settings-button" };
            settingButton.Add(new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit });

            titleContainer.Add(settingButton);
        }

        void ToggleSettings()
        {
            settingsExpanded = !settingsExpanded;
            if (settingsExpanded)
                OpenSettings();
            else
                CloseSettings();
        }

        public void OpenSettings()
        {
            if (settingsContainer != null)
            {
                Owner.ClearSelection();
                Owner.AddToSelection(this);

                settingButton.AddToClassList("clicked");
                settingsContainer.visible = true;
                settingsExpanded = true;
            }
        }

        public void CloseSettings()
        {
            if (settingsContainer != null)
            {
                settingButton.RemoveFromClassList("clicked");
                settingsContainer.visible = false;
                settingsExpanded = false;
            }
        }

        #endregion

        #region API

        public void HightlightOn()
        {

        }

        public void HightlightOf()
        {

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

        void AddBadge(IconBadge badge)
        {
            Add(badge);
            badges.Add(badge);
            badge.AttachTo(topContainer, SpriteAlignment.TopRight);
        }

        void RemoveBadge(Func<IconBadge, bool> callback)
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
        void OpenNodeViewScript()
        {
            var script = NodeEditorUtility.FindScriptFromType(GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        void OpenNodeScript()
        {
            var script = NodeEditorUtility.FindScriptFromType(NodeData.GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }
        #endregion

        #region Callbacks & Overrides

        Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new Dictionary<string, List<(object value, VisualElement target)>>();
        Dictionary<string, VisualElement> hideElementIfConnected = new Dictionary<string, VisualElement>();
        Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new Dictionary<FieldInfo, List<VisualElement>>();


        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();
        }

        protected virtual void DrawDefaultInspector()
        {
            var fields = NodeData.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Type nodeDataType = NodeData.GetType();
            foreach (var field in fields)
            {
                if (AttributeCache.TryGetFieldInfoAttribute(nodeDataType, field, out SettingAttribute settingAttribute))
                {
                    HasSettings = true;
                    continue;
                }

                // 是否是一个接口
                bool isPort = AttributeCache.TryGetFieldInfoAttribute(nodeDataType, field, out PortAttribute portAttrib);
                // 是否是入方向的接口
                bool isInputPort = isPort && portAttrib.Direction == PortDirection.Input;
                // 是公开，或者有SerializeField特性
                bool isDisplay = field.IsPublic || AttributeCache.TryGetFieldInfoAttribute(nodeDataType, field, out SerializeField serializable);

                if (!isDisplay || (isPort && portAttrib.ShowBackValue == ShowBackingValue.Never))
                    continue;

                bool showAsDrawer = field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                if (isPort)
                    continue;

                //skip if marked with NonSerialized or HideInInspector
                if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
                    continue;

                // Hide the field if we want to display in in the inspector
                var showInInspector = field.GetCustomAttribute<ShowInInspector>();
                if (showInInspector != null && !showInInspector.showInNode)
                    continue;

                var showInputDrawer = isInputPort && isDisplay;
                showInputDrawer |= isInputPort && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                showInputDrawer &= !typeof(IList).IsAssignableFrom(field.FieldType);

                var elem = AddControlField(field, ObjectNames.NicifyVariableName(field.Name), showInputDrawer);
                if (isInputPort)
                {
                    hideElementIfConnected[field.Name] = elem;

                    // Hide the field right away if there is already a connection:
                    if (PortViews.TryGetValue(field.Name, out var pv))
                        if (pv.Edges.Count > 0)
                            elem.style.display = DisplayStyle.None;
                }
            }
        }

        void UpdateFieldVisibility(string fieldName, object newValue)
        {
            if (visibleConditions.TryGetValue(fieldName, out var list))
            {
                foreach (var elem in list)
                {
                    if (newValue.Equals(elem.value))
                        elem.target.style.display = DisplayStyle.Flex;
                    else
                        elem.target.style.display = DisplayStyle.None;
                }
            }
        }

        void UpdateOtherFieldValueSpecific<T>(FieldInfo field, object newValue)
        {
            foreach (var inputField in fieldControlsMap[field])
            {
                var notify = inputField as INotifyValueChanged<T>;
                if (notify != null)
                    notify.SetValueWithoutNotify((T)newValue);
            }
        }

        static MethodInfo specificUpdateOtherFieldValue = typeof(BaseNodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        void UpdateOtherFieldValue(FieldInfo info, object newValue)
        {
            // Warning: Keep in sync with FieldFactory CreateField
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);

            genericUpdate.Invoke(this, new object[] { info, newValue });
        }

        object GetInputFieldValueSpecific<T>(FieldInfo field)
        {
            if (fieldControlsMap.TryGetValue(field, out var list))
            {
                foreach (var inputField in list)
                {
                    if (inputField is INotifyValueChanged<T> notify)
                        return notify.value;
                }
            }
            return null;
        }

        static MethodInfo specificGetValue = typeof(BaseNodeView).GetMethod(nameof(GetInputFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        object GetInputFieldValue(FieldInfo info)
        {
            // Warning: Keep in sync with FieldFactory CreateField
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificGetValue.MakeGenericMethod(fieldType);

            return genericUpdate.Invoke(this, new object[] { info });
        }

        protected VisualElement AddControlField(string fieldName, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
            => AddControlField(NodeData.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), label, showInputDrawer, valueChangedCallback);

        protected VisualElement AddControlField(FieldInfo field, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
        {
            if (field == null)
                return null;

            var element = FieldFactory.CreateField(field.FieldType, field.GetValue(NodeData), (newValue) =>
            {
                Owner.RegisterCompleteObjectUndo("Updated " + newValue);
                field.SetValue(NodeData, newValue);
                NotifyNodeChanged();
                valueChangedCallback?.Invoke();
                UpdateFieldVisibility(field.Name, newValue);
                // When you have the node inspector, it's possible to have multiple input fields pointing to the same
                // property. We need to update those manually otherwise they still have the old value in the inspector.
                UpdateOtherFieldValue(field, newValue);
            }, showInputDrawer ? "" : label);

            if (!fieldControlsMap.TryGetValue(field, out var inputFieldList))
                inputFieldList = fieldControlsMap[field] = new List<VisualElement>();
            inputFieldList.Add(element);

            if (element != null)
            {
                if (showInputDrawer)
                {
                    var box = new VisualElement { name = field.Name };
                    box.AddToClassList("port-input-element");
                    box.Add(element);
                    inputContainerElement.Add(box);
                }
                else
                {
                    controlsContainer.Add(element);
                }
            }

            return element;
        }


        public void ForceUpdatePorts()
        {
            NodeDataCache.UpdateStaticPorts(NodeData);

            RefreshPorts();
        }

        void UpdateFieldValues()
        {
            foreach (var kp in fieldControlsMap)
                UpdateOtherFieldValue(kp.Key, kp.Key.GetValue(NodeData));
        }

        protected void AddSettingField(FieldInfo field)
        {
            if (field == null)
                return;

            var label = field.GetCustomAttribute<SettingAttribute>().name;

            var element = FieldFactory.CreateField(field.FieldType, field.GetValue(NodeData), (newValue) =>
            {
                Owner.RegisterCompleteObjectUndo("Updated " + newValue);
                field.SetValue(NodeData, newValue);
            }, label);

            if (element != null)
            {
                settingsContainer.Add(element);
                element.name = field.Name;
            }
        }

        internal void OnPortConnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.FieldName) != null)
                inputContainerElement.Q(port.FieldName).AddToClassList("empty");

            if (hideElementIfConnected.TryGetValue(port.FieldName, out var elem))
                elem.style.display = DisplayStyle.None;
        }

        internal void OnPortDisconnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.FieldName) != null)
                inputContainerElement.Q(port.FieldName).RemoveFromClassList("empty");

            if (hideElementIfConnected.TryGetValue(port.FieldName, out var elem))
                elem.style.display = DisplayStyle.Flex;
        }

        public override void SetPosition(Rect newPos)
        {
            if (Initialized || !NodeData.Locked)
            {
                Initialized = false;
                base.SetPosition(newPos);

                Owner.RegisterCompleteObjectUndo("Moved graph node");
                NodeData.position = newPos;
            }
        }

        public void ChangeLockStatus()
        {
            NodeData.Locked ^= true;
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
            if (NodeEditorUtility.FindScriptFromType(NodeData.GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        // 按钮状态
        Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (NodeEditorUtility.FindScriptFromType(GetType()) != null)
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

        protected virtual VisualElement CreateSettingsView() { return new Label("Settings") { name = "header" }; }

        /// <summary> Send an event to the graph telling that the content of this node have changed </summary>
        public void NotifyNodeChanged()
        {
            //owner.graphData.NotifyNodeChanged(nodeData);
        }

        #endregion
    }
}