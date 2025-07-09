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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */
#endregion
using Atom;
using Atom.GraphProcessor;

[NodeTooltip("加法节点")]
[NodeMenu("Add")]
public class AddNode : BaseNode { }

[ViewModel(typeof(AddNode))]
public class AddNodeProcessor : BaseNodeProcessor, IGetPortValue, IGetPortValue<float>
{
    public AddNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new PortProcessor("InputA", BasePort.Direction.Left, BasePort.Capacity.Single, typeof(float)));
        AddPort(new PortProcessor("InputB", BasePort.Direction.Left, BasePort.Capacity.Single, typeof(float)));
        AddPort(new PortProcessor(ConstValues.FLOW_OUT_PORT_NAME, BasePort.Direction.Right, BasePort.Capacity.Multi, typeof(float))
        {
            HideLabel = true
        });
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
