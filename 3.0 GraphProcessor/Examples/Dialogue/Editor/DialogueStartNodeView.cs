using GraphProcessor.Editors;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor.Dialogue.Editors
{
    [CustomNodeEditor(typeof(DialogueStartNode))]
    public class DialogueStartNodeView : BaseNodeView
    {
        protected override void OnInitialized()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Dialogue/Styles/StartOrEndNodeStyle"));

            titleContainer.Remove(titleContainer.Q("title-button-container"));
            topContainer.parent.Remove(topContainer);
            titleContainer.Add(topContainer);
            SetSelectable(false);
            SetMovable(false);
        }
    }
}
