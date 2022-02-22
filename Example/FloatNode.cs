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
using CZToolKit.Core.BindableProperty;
using CZToolKit.GraphProcessor;

public class FloatNode : BaseNode
{
    public float num;

    protected override void OnEnabled()
    {
        base.OnEnabled();

        AddPort(new BasePort("Output", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(float)));

        this[nameof(num)] = new BindableProperty<float>(() => num, v => num = v);
    }

    public override object GetValue(string port)
    {
        return num;
    }
}
