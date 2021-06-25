using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(RelayNode))]
    public class RelayNodeView : BaseNodeView
    {
        protected override void OnInitialized()
        {
            styleSheets.Add(GraphProcessorStyles.RelayNodeViewStyle);

            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // 双击删除
            if (evt.clickCount == 2)
                Owner.RemoveRelayNode(this);
        }
    }
}