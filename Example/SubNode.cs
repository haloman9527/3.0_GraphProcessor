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

[NodeMenuItem("Sub")]
public class SubNode : BaseNode, IGetValue<float>
{
    protected override void OnEnabled()
    {
        base.OnEnabled();

        AddPort(new BasePort("InputA", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePort("InputB", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePort("Output", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(float)));
    }

    public override object GetValue(string port)
    {
        var inputAValue = Ports["InputA"].GetConnectionValue<float>();
        var inputBValue = Ports["InputB"].GetConnectionValue<float>();
        return inputAValue - inputBValue;
    }

    float IGetValue<float>.GetValue(string port)
    {
        var inputAValue = Ports["InputA"].GetConnectionValue<float>();
        var inputBValue = Ports["InputB"].GetConnectionValue<float>();
        return inputAValue - inputBValue;
    }
}
