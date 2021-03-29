using GraphProcessor.Editors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor.Dialogue.Editors
{
    [CustomNodeEditor(typeof(DialogueNode))]
    public class DialogueNodeView : BaseNodeView
    {
        static Texture icon;
        static Texture Icon
        {
            get
            {
                if (icon == null)
                    icon = Resources.Load<Texture>("Dialogue/Textures/untitled");
                return icon;
            }
        }


        DialogueNode dialogueNode;
        protected override void OnInitialized()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Dialogue/Styles/DialogueNodeStyle"));

            Image img = new Image();
            img.image = Icon;
            img.style.height = topContainer.style.height;
            topContainer.Q("divider","vertical").Add(img);

            titleContainer.Remove(titleContainer.Q("title-button-container"));
            titleContainer.Remove(titleContainer.Q("title-label"));
            topContainer.parent.Remove(topContainer);
            titleContainer.Add(topContainer);

            style.minWidth = style.maxWidth = 100;

            //titleContainer.style.display = DisplayStyle.None;
            foreach (var item in PortViews)
            {
                item.Value.portName = "";
                item.Value.style.maxHeight = 20;
            }

            dialogueNode = NodeData as DialogueNode;
            Label label = new Label() { text = dialogueNode.text };
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.maxWidth = 150;


            controlsContainer.Add(label);
            contentContainer.Add(new IMGUIContainer(() =>
            {
                label.text = dialogueNode.text;
                tooltip = dialogueNode.text;
            }));
        }
    }
}
