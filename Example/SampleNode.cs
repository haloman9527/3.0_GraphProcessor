#region 注 释
/***
 *
 *  Title:
 *  
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
    protected override void InitializeBindableProperties()
    {
        base.InitializeBindableProperties();
        this["Port-A"] = new BindableProperty<BasePort>(new BasePort("A", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Multi));
        this["Port-B"] = new BindableProperty<BasePort>(new BasePort("B", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi));
    }

    public override IEnumerable<BasePort> GetPorts()
    {
        yield return this["Port-A"].AsBindableProperty<BasePort>().Value;
        yield return this["Port-B"].AsBindableProperty<BasePort>().Value;
    }
}
