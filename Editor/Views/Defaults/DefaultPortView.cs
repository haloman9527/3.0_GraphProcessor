using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public sealed class DefaultPortView : PortView
{
    public static PortView CreatePV(Slot slot, Type portType)
    {
        Orientation o = slot.orientation == Slot.Orientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical;
        Direction d = slot.direction == Slot.Direction.Input ? Direction.Input : Direction.Output;
        Capacity c = slot.capacity == Slot.Capacity.Single ? Capacity.Single : Capacity.Multi;
        return new DefaultPortView(o, d, c, portType);
    }

    DefaultPortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
    {
        m_EdgeConnector = new EdgeConnector<BaseConnectionView>(new EdgeConnectorListener());
        this.AddManipulator(m_EdgeConnector);
    }
}
