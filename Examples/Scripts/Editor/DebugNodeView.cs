using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class PV : Port, IBasePortView
    {
        public static PV Create(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type)
        {
            PV pv = new PV(portOrientation, portDirection, portCapacity, type);
            return pv;
        }

        protected PV(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }

        public Port Self { get { return this; } }

        public BaseNodeView Owner { get; private set; }

        public PortTypeConstraint TypeConstraint { get { return PortTypeConstraint.Inherited; } }

        public Type DisplayType { get { return portType; } }

        public string FieldName { get { return portName; } }

        public void Initialize(BaseNodeView _owner)
        {
            Owner = _owner;
        }
    }

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
            if (!debugNode.TryGetPort("input", out NodePort port) || !port.IsConnected)
            {
                label.text = debugNode.input;
                return;
            }

            object value = null;
            if (port.TryGetConnectValue(ref value))
            {
                if (value == null || value.Equals(null))
                    label.text = "NULL";
                else
                    label.text = value.ToString();
            }
        }
    }
}
