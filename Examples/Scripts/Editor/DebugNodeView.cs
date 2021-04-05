using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor.Editors
{
    [CustomNodeEditor(typeof(DebugNode))]
    public class PrintNodeView : BaseNodeView
    {
        DebugNode debugNode; 
        Label label = new Label();

        protected override void OnInitialized()
        {
            debugNode = NodeData as DebugNode;
            controlsContainer.Add(label);
            contentContainer.Add(new IMGUIContainer(() =>
            {
                UpdateLabel();
            }));
        }

        private void UpdateLabel()
        {
            if(debugNode.TryGetInputValue("input", out object value))
            {
                if (value == null)
                    label.text = "NULL";
                else
                    label.text = value.ToString();
            }
            else
            {
                label.text = debugNode.text;
            }

        }
    }
}
