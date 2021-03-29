using GraphProcessor.Editors;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor.Dialogue.Editors
{
    [CustomNodeEditor(typeof(DialogueEndNode))]
    public class DialogueEndNodeView : BaseNodeView
    {
        DialogueEndNode endNode;
        protected override void OnInitialized()
        {
            endNode = NodeData as DialogueEndNode;

            styleSheets.Add(Resources.Load<StyleSheet>("Dialogue/Styles/StartOrEndNodeStyle"));

            titleContainer.Remove(titleContainer.Q("title-button-container"));
            topContainer.parent.Remove(topContainer);
            titleContainer.Insert(0,topContainer);

            title = "End";
            PortViews[nameof(endNode.enter)].portColor = Color.red;
        }
    }
}
