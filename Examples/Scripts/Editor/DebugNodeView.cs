using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(DebugNode))]
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
            Add(new IMGUIContainer(UpdateLabel));
            MarkDirtyRepaint();
        }

        void UpdateLabel()
        {
            if (!debugNode.TryGetPort(nameof(debugNode.input), out NodePort port) || !port.IsConnected)
            {
                label.text = debugNode.input;
                return;
            }

            object value = port.GetConnectValue();
            if (value != null)
            {
                if (value.Equals(null))
                    label.text = "NULL";
                else
                    label.text = value.ToString();
            }
        }
    }
}
