using UnityEngine.UIElements;

namespace GraphProcessor.Editors
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
            contentContainer.Add(new IMGUIContainer(() =>
            {
                UpdateLabel();
            }));
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (!debugNode.TryGetPort("input", out NodePort port) || !port.IsConnected)
            {
                label.text = debugNode.text;
                return;
            }

            object value = null;
            if (port.TryGetConnectValue(ref value))
            {
                if (value == null)
                    label.text = "NULL";
                else
                    label.text = value.ToString();
            }
        }
    }
}
