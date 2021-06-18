using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(RelayNode))]
    public class RelayNodeView : BaseNodeView
    {
        protected override void OnInitialized()
        {
            titleContainer.RemoveFromHierarchy();
            this.Q("divider").RemoveFromHierarchy();

            styleSheets.Add(GraphProcessorStyles.RelayNodeViewStyle);
            foreach (var item in PortViews)
            {
                item.Value.Q("type").style.display = DisplayStyle.None;
            }

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