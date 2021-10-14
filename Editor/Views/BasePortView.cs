#region ×¢ ÊÍ
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: °ëÖ»ÁúÏºÈË
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class BasePortView<M> : InternalBasePortView where M : BaseSlot
    {
        BasePortView(Orientation orientation, Direction direction, Capacity capacity, Type type) : base(orientation, direction, capacity, type)
        {
            m_EdgeConnector = new EdgeConnector<BaseConnectionView>(new EdgeConnectorListener());
            this.AddManipulator(m_EdgeConnector);
        }

        public BasePortView(BaseSlot slot, Type portType) : this(
            orientation: slot.orientation == BaseSlot.Orientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical,
            direction: slot.direction == BaseSlot.Direction.Input ? Direction.Input : Direction.Output,
            capacity: slot.capacity == BaseSlot.Capacity.Single ? Capacity.Single : Capacity.Multi,
            portType)
        {

        }
    }

    /// <summary> Ä¬ÈÏ </summary>
    public sealed class BasePortView : BasePortView<BaseSlot>
    {
        public BasePortView(BaseSlot slot, Type portType) : base(slot, portType) { }
    }
}