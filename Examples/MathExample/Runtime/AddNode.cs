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
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using CZToolKit.VM;
using CZToolKit.GraphProcessor;

[NodeTooltip("加法节点")]
[NodeMenu("Add")]
public class AddNode : BaseNode { }

[ViewModel(typeof(AddNode))]
public class AddNodeVM : BaseNodeVM, IGetPortValue, IGetPortValue<float>
{
    public AddNodeVM(BaseNode model) : base(model)
    {
        AddPort(new BasePortVM("InputA", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePortVM("InputB", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePortVM("Result", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(float))
        {
            HideLabel = true
        });
        AddPort(new BasePortVM("FlowIn", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Multi, typeof(object)));
        AddPort(new BasePortVM("FlowOut", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(object)));
    }

    public object GetValue(string port)
    {
        var inputAValue = Ports["InputA"].GetConnectionValue<float>();
        var inputBValue = Ports["InputB"].GetConnectionValue<float>();
        return inputAValue + inputBValue;
    }

    float IGetPortValue<float>.GetValue(string port)
    {
        var inputAValue = Ports["InputA"].GetConnectionValue<float>();
        var inputBValue = Ports["InputB"].GetConnectionValue<float>();
        return inputAValue + inputBValue;
    }
}
