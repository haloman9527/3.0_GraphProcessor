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

        public void AddToLeft(VisualElement element)
        {
            element.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            Left.Add(element);
        }

        public void AddToRight(VisualElement element)
        {
            element.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            element.style.left = 0;
            element.style.borderRightWidth = 0;
            Right.Add(element);
        }

        public void AddButtonToLeft(ToolbarButton button)
        {
            button.style.alignSelf = Align.Center;
            button.style.width = 60;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            AddToLeft(button);
        }

        public void AddButtonToRight(ToolbarButton button)
        {
            button.style.alignSelf = Align.Center;
            button.style.width = 60;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            AddToRight(button);
        }

        public void AddToggleToLeft(ToolbarToggle toggle, float width)
        {
            toggle.Q(className: "unity-toggle__input").style.justifyContent = Justify.Center;
            toggle.Q(className: "unity-toggle__input").StretchToParentSize();
            toggle.Q(className: "unity-toggle__input").style.marginBottom = 0;
            toggle.Q(className: "unity-toggle__text").style.color = new Color(0, 0, 0, 1);
            toggle.style.alignSelf = Align.Center;
            toggle.style.width = width;
            toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
            AddToLeft(toggle);
        }

        public void AddToggleToRight(ToolbarToggle toggle, float width)
        {
            toggle.Q(className: "unity-toggle__input").style.justifyContent = Justify.Center;
            toggle.Q(className: "unity-toggle__input").StretchToParentSize();
            toggle.Q(className: "unity-toggle__input").style.marginBottom = 0;
            toggle.Q(className: "unity-toggle__text").style.color = new Color(0, 0, 0, 1);
            toggle.style.alignSelf = Align.Center;
            toggle.style.width = width;
            toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
            AddToLeft(toggle);
        }

        public void RemoveFromLeft(int index)
        {
            Left.RemoveAt(index);
        }

        public void RemoveFromRight(int index)
        {
            Right.RemoveAt(index);
        }
    }
}
#endif