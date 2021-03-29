using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor.Editors
{
    [CustomNodeEditor(typeof(RelayNode))]
    public class RelayNodeView : BaseNodeView
    {
        protected override void OnInitialized()
        {
            titleContainer.RemoveFromHierarchy();
            this.Q("divider").RemoveFromHierarchy();

            styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/RelayNode"));
            foreach (var item in PortViews)
            {
                item.Value.Q("type").style.display = DisplayStyle.None;
            }
        }
    }
}