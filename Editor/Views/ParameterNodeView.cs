using UnityEngine.UIElements;

namespace GraphProcessor.Editors
{
    [CustomNodeView(typeof(ParameterNode))]
    public class ParameterNodeView : BaseNodeView
    {
        ParameterNode parameterNode;

        protected override void OnInitialized()
        {
            parameterNode = NodeData as ParameterNode;

            titleContainer.Remove(titleContainer.Q("title-button-container"));
            topContainer.parent.Remove(topContainer);
            titleContainer.Add(topContainer);

            title = parameterNode.Parameter?.Name;
        }
    }
}
