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

    protected override void OnEnabled()
    {
        base.OnEnabled();

        this[nameof(value)] = new BindableProperty<int>(() => value, v => value = v);

        AddPort(new BasePort("Input", BasePort.Orientation.Vertical, BasePort.Direction.Input, BasePort.Capacity.Multi, typeof(int)));
        AddPort(new BasePort("Output", BasePort.Orientation.Vertical, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(int)));
    }
}
