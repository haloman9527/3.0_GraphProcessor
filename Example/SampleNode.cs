#region 注 释
/***
 *
 *  Title:
 *      一个样例节点
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using CZToolKit.GraphProcessor;
using System.Collections.Generic;

public class SampleNode : BaseNode
{
    public int value;

    protected override void BindProperties()
    {
        base.BindProperties();
        this[nameof(value)] = new BindableProperty<int>(0, v => value = v);
    }

    protected override IEnumerable<BasePort> GetPorts()
    {
        yield return new BasePort("A", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Multi, typeof(int));
        yield return new BasePort("B", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(int));
    }
}
