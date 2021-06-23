using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public class ToolbarView : Toolbar
    {
        public new class UxmlFactory : UxmlFactory<ToolbarView, GraphView.UxmlTraits> { }

        public VisualElement Left { get; }
        public VisualElement Right { get; }

        public ToolbarView()
        {
            name = "ToolbarView";
            style.height = 20;
            style.backgroundColor = new Color(0, 0, 0, 0);

            Left = new VisualElement() { name = "left", style = { flexDirection = FlexDirection.Row } };
            Right = new VisualElement() { name = "right", style = { flexDirection = FlexDirection.Row } };

            Add(Left);
            Add(new VisualElement() { style = { flexGrow = 1 } });
            Add(Right);
        }

        public void AddToLeft(VisualElement _element)
        {
            _element.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            Left.Add(_element);
        }

        public void AddToRight(VisualElement _element)
        {
            _element.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            _element.style.left = 0;
            _element.style.borderRightWidth = 0;
            Right.Add(_element);
        }

        public void AddButtonToLeft(ToolbarButton _button)
        {
            _button.style.alignSelf = Align.Center;
            _button.style.width = 60;
            _button.style.unityTextAlign = TextAnchor.MiddleCenter;
            _button.style.color = Color.black;
            AddToLeft(_button);
        }

        public void AddButtonToRight(ToolbarButton _button)
        {
            _button.style.alignSelf = Align.Center;
            _button.style.width = 60;
            _button.style.unityTextAlign = TextAnchor.MiddleCenter;
            _button.style.color = Color.black;
            AddToRight(_button);
        }

        public void AddToggleToLeft(ToolbarToggle _toggle)
        {
            _toggle.Q(className: "unity-toggle__input").style.justifyContent = Justify.Center;
            _toggle.Q(className: "unity-toggle__input").StretchToParentSize();
            _toggle.Q(className: "unity-toggle__input").style.marginBottom = 0;
            _toggle.Q(className: "unity-toggle__text").style.color = new Color(0, 0, 0, 1);
            AddToLeft(_toggle);
        }

        public void AddToggleToRight(ToolbarToggle _toggle)
        {
            _toggle.Q(className: "unity-toggle__input").style.justifyContent = Justify.Center;
            _toggle.Q(className: "unity-toggle__input").StretchToParentSize();
            _toggle.Q(className: "unity-toggle__input").style.marginBottom = 0;
            _toggle.Q(className: "unity-toggle__text").style.color = new Color(0, 0, 0, 1);
            AddToLeft(_toggle);
        }
    }
}
