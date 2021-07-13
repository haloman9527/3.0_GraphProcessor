using CZToolKit.Core;
using CZToolKit.MVVM;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public interface IHasSettingsView
    {
        void CloseSettings();
    }

    public abstract class HasSettingNodeView<M> : BaseNodeView<M>, IHasSettingsView where M : BaseNode
    {
        bool settingsExpanded = false;
        NodeSettingsView settingsContainer;
        VisualElement settings;
        Button settingButton;

        public HasSettingNodeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.SettingsNodeViewStyle);

            settingButton = new Button(ToggleSettings) { name = "settings-button" };
            settingButton.Add(new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit });
            titleContainer.Add(settingButton);

            settingsContainer = new NodeSettingsView();
            settingsContainer.visible = false;
            settings = new VisualElement();
            settings.Add(new Label("Settings") { name = "header" });
            settingsContainer.Add(settings);
            Add(settingsContainer);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            InitializeSettings();
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnGeometryChanged(null);
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

        void InitializeSettings()
        {
            foreach (var fieldInfo in Model.GetNodeFieldInfos())
            {
                if (Utility_Attribute.TryGetFieldInfoAttribute(fieldInfo, out ShowInSettingAttribute settingAttribute))
                    AddSettingField(fieldInfo);
            }
        }

        protected void AddSettingField(FieldInfo _fieldInfo)
        {
            var label = Utility_Attribute.TryGetFieldInfoAttribute(_fieldInfo, out InspectorNameAttribute inspectorNameAttribute)
                ? inspectorNameAttribute.displayName : _fieldInfo.Name;
            VisualElement fieldDrawer = CreateControlField(_fieldInfo, label);
            if (fieldDrawer == null) return;
            settingsContainer.Add(fieldDrawer);
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
    }
}