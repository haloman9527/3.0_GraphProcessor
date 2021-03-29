using GraphProcessor.Editors;
using UnityEngine;

namespace GraphProcessor.Dialogue
{
    [CustomGraphWindow(typeof(Dialogue))]
    public class DialogueGraphWindow : BaseGraphWindow
    {
        protected override void InitializeWindow(BaseGraph graph)
        {
            base.InitializeWindow(graph);

            titleContent = new GUIContent("Dialogue");
        }
    }
}
