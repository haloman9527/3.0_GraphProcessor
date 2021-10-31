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
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseConnectionView : Edge, IBindableView<BaseConnection>
    {
        public Label IndexLabel { get; private set; }
        public BaseConnection Model { get; private set; }
        protected BaseGraphView Owner { get; private set; }

        public BaseConnectionView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);
            IndexLabel = new Label();
            IndexLabel.style.display = DisplayStyle.None;
            edgeControl.Add(IndexLabel);
        }

        public void SetUp(BaseConnection connection, BaseGraphView graphView)
        {
            Model = connection;
            Owner = graphView;
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public virtual void UnBindingProperties()
        {

        }

        void OnMouseDown(MouseDownEvent e)
        {
            if (e.clickCount == 2)
            {
                var position = e.mousePosition;
                position += new Vector2(-20 * Owner.scale, -30 * Owner.scale);
                Vector2 mousePos = Owner.GraphWindow.rootVisualElement.ChangeCoordinatesTo(Owner.contentViewContainer, position);
            }
        }

        public void ShowIndex(int index)
        {
            IndexLabel.text = index.ToString();
            IndexLabel.style.display = DisplayStyle.Flex;
        }

        public void HideIndex()
        {
            IndexLabel.text = "";
            IndexLabel.style.display = DisplayStyle.None;
        }
    }
}
#endif