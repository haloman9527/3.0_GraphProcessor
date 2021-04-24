using CZToolKit.Core;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor.Editors
{
    public interface IHasSettingNodeView
    {
        void CloseSettings();
    }

    public class HasSettingNodeView : BaseNodeView, IHasSettingNodeView
    {
        bool settingsExpanded = false;
        NodeSettingsView settingsContainer;
        VisualElement settings;
        Button settingButton;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessor/Styles/SettingsNodeView"));
            InitializeSettings();
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnGeometryChanged(null);
        }

        void InitializeSettings()
        {
            CreateSettingButton();
            settingsContainer = new NodeSettingsView();
            settingsContainer.visible = false;
            settings = new VisualElement();
            settings.Add(CreateSettingsView());
            settingsContainer.Add(settings);
            Add(settingsContainer);

            foreach (var field in NodeDataTypeFieldInfos)
            {
                if (AttributeCache.TryGetFieldInfoAttribute(NodeDataType, field, out SettingAttribute settingAttribute))
                    AddSettingField(field);
            }
        }

        protected void AddSettingField(FieldInfo _fieldInfo)
        {
            if (_fieldInfo == null)
                return;


            var label = AttributeCache.TryGetFieldInfoAttribute(NodeDataType, _fieldInfo, out DisplayNameAttribute displayNameAttribute)
                ? displayNameAttribute.DisplayName : NodeEditorUtility.GetDisplayName(_fieldInfo.Name);

            var element = FieldFactory.CreateField(_fieldInfo.FieldType, _fieldInfo.GetValue(NodeData), (newValue) =>
            {
                Owner.RegisterCompleteObjectUndo("Updated " + newValue);
                _fieldInfo.SetValue(NodeData, newValue);
            }, label);

            if (element != null)
            {
                settingsContainer.Add(element);
                element.name = _fieldInfo.Name;
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

        protected virtual VisualElement CreateSettingsView() { return new Label("Settings") { name = "header" }; }

    }
}