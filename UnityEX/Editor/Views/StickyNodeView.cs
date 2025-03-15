using UnityEditor;
using UnityEngine.UIElements;

#if UNITY_EDITOR

namespace Atom.GraphProcessor.Editors
{
    [CustomView(typeof(StickyNode))]
    public sealed class StickyNodeView : BaseNodeView
    {
        private TextField title;
        private Label titleLabel;
        private TextField contents;
        private Label contentsLabel;

        public StickyNodeView()
        {
            styleSheets.Add(GraphProcessorEditorStyles.DefaultStyles.StickyNodeStyle);

            title = new TextField() { name = "title" };
            titleLabel = new Label() { name = "titleLabel" };

            contents = new TextField() { name = "contents", multiline = true };
            contentsLabel = new Label() { name = "contentsLabel" };

            this.controls.Add(titleLabel);
            this.controls.Add(title);

            this.controls.Add(contentsLabel);
            this.controls.Add(contents);

            titleLabel.RegisterCallback<PointerDownEvent>(OnTitleClicked);
            title.RegisterCallback<FocusOutEvent>(OnTitleFocusOut);
            // title.RegisterValueChangedCallback(OnTitleInputChanged);

            contentsLabel.RegisterCallback<PointerDownEvent>(OnContentsClicked);
            contents.RegisterCallback<FocusOutEvent>(OnContentsFocusOut);
            // contents.RegisterValueChangedCallback(OnContentsInputChanged);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            title.SetValueWithoutNotify(ViewModel.Title);
            titleLabel.text = ViewModel.Title;
            contents.SetValueWithoutNotify((ViewModel as StickyNodeProcessor).Contents);
            contentsLabel.text = (ViewModel as StickyNodeProcessor).Contents;
        }

        protected override void OnBindingProperties()
        {
            base.OnBindingProperties();
            ViewModel.RegisterValueChanged<InternalVector2Int>(nameof(StickyNode.size), OnSizeChanged);
            ViewModel.RegisterValueChanged<string>(nameof(StickyNode.title), OnTitleChanged);
            ViewModel.RegisterValueChanged<string>(nameof(StickyNode.contents), OnContentsChanged);
        }

        protected override void OnUnBindingProperties()
        {
            base.OnUnBindingProperties();
            ViewModel.UnregisterValueChanged<InternalVector2Int>(nameof(StickyNode.size), OnSizeChanged);
            ViewModel.UnregisterValueChanged<string>(nameof(StickyNode.title), OnTitleChanged);
            ViewModel.UnregisterValueChanged<string>(nameof(StickyNode.contents), OnContentsChanged);
        }

        private void OnTitleClicked(PointerDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                schedule.Execute(() =>
                {
                    title.style.display = DisplayStyle.Flex;
                    titleLabel.style.display = DisplayStyle.None;
                    title.Focus();
                    title.SelectNone();
                }).ExecuteLater(20);
            }
        }

        private void OnContentsClicked(PointerDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                schedule.Execute(() =>
                {
                    contents.style.display = DisplayStyle.Flex;
                    contentsLabel.style.display = DisplayStyle.None;
                    contents.Focus();
                    contents.SelectNone();
                }).ExecuteLater(20);
            }
        }

        private void OnTitleFocusOut(FocusOutEvent evt)
        {
            var oldValue = (ViewModel as StickyNodeProcessor).Title;
            var newValue = title.value;
            Owner.CommandDispatcher.Do(() => { (ViewModel as StickyNodeProcessor).Title = newValue; }, () => { (ViewModel as StickyNodeProcessor).Title = oldValue; });
            title.style.display = DisplayStyle.None;
            titleLabel.style.display = DisplayStyle.Flex;
        }

        private void OnContentsFocusOut(FocusOutEvent evt)
        {
            var oldValue = (ViewModel as StickyNodeProcessor).Contents;
            var newValue = contents.value;
            Owner.CommandDispatcher.Do(() => { (ViewModel as StickyNodeProcessor).Contents = newValue; }, () => { (ViewModel as StickyNodeProcessor).Contents = oldValue; });
            contents.style.display = DisplayStyle.None;
            contentsLabel.style.display = DisplayStyle.Flex;
        }

        private void OnSizeChanged(ViewModel.ValueChangedArg<InternalVector2Int> obj)
        {
        }

        private void OnTitleChanged(ViewModel.ValueChangedArg<string> obj)
        {
            title.SetValueWithoutNotify(obj.newValue);
            titleLabel.text = obj.newValue;
        }

        private void OnContentsChanged(ViewModel.ValueChangedArg<string> obj)
        {
            contents.SetValueWithoutNotify(obj.newValue);
            contentsLabel.text = obj.newValue;
        }
    }
}
#endif