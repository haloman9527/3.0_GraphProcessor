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
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class InternalBaseConnectionView : Edge, IBindableView<BaseConnection>
    {
        public BaseConnection Model { get; private set; }
        protected InternalBaseGraphView Owner { get; private set; }

        public InternalBaseConnectionView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);
        }

        public void SetUp(BaseConnection connection, InternalBaseGraphView graphView)
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
    }
}