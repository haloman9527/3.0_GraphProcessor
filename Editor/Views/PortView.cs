using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class PortView : Port
    {
        public Image Icon { get; }
        public BaseGraphView GraphView { get; private set; }
        public Slot Model { get; private set; }

        protected PortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            styleSheets.Add(GraphProcessorStyles.PortViewStyle);
            Icon = new Image();
            Icon.AddToClassList("port-icon");
            Insert(1, Icon);

            var portLabel = this.Q("type");
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;
                portLabel.style.flexGrow = 1;
            }
            bool vertical = portOrientation == Orientation.Vertical;

            if (vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            if (vertical)
                this.Q("connector").pickingMode = PickingMode.Position;

            if (portOrientation == Orientation.Vertical)
                AddToClassList("vertical");
        }

        public void SetUp(Slot slot, BaseGraphView graphView)
        {
            Model = slot;
            GraphView = graphView;

            tooltip = slot.name;
        }
    }
}
