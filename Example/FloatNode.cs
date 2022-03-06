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
using CZToolKit.Core.IntegratedViewModel;
using CZToolKit.GraphProcessor;

[NodeMenuItem("Float")]
public class FloatNode : BaseNode, IGetValue, IGetValue<float>
{
    public float num;

    protected override void OnEnabled()
    {
        base.OnEnabled();

        AddPort(new BasePort("Output", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(float)));

        this[nameof(num)] = new BindableProperty<float>(() => num, v => num = v);
    }

    public object GetValue(string port)
    {
        return num;
    }

    float IGetValue<float>.GetValue(string port)
    {
        return num;
    }
}
