using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(DebugNode))]
    [NodeTooltip("123123124123124123")]
    public class DebugNodeView : BaseNodeView
    {
        DebugNode debugNode;
        Label label = new Label();

        protected override void OnInitialized()
        {
            base.OnInitialized();
            debugNode = NodeData as DebugNode;
            controlsContainer.Add(label);
            UpdateLabel();
            Image icon = new Image() { image = EditorGUIUtility.FindTexture("editicon.sml") };
            icon.style.width = 25;
            icon.style.height = 25;
            icon.style.left = 5;
            icon.style.alignSelf = Align.Center;
            AddIcon(icon);
            Add(new IMGUIContainer(UpdateLabel));
        }

        void UpdateLabel()
        {
            if (!debugNode.TryGetPort(nameof(debugNode.input), out NodePort port) || !port.IsConnected)
            {
                label.text = debugNode.input;
                return;
            }

            object value = null;
            if (port.TryGetConnectValue(ref value))
            {
                if (value.Equals(null))
                    label.text = "NULL";
                else
                    label.text = value.ToString();
            }
        }
    }
}
