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

using System.Collections.Generic;
using Atom;
using Atom.GraphProcessor;

[NodeMenu("Sub")]
public class SubNode : BaseNode
{
    public List<string> ports = new List<string>();
}

[ViewModel(typeof(SubNode))]
public class SubNodeProcessor : BaseNodeProcessor, IGetPortValue, IGetPortValue<float>
{
    public SubNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new BasePortProcessor("InputA", BasePort.Direction.Left, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePortProcessor("InputB", BasePort.Direction.Left, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePortProcessor(ConstValues.FLOW_OUT_PORT_NAME, BasePort.Direction.Right, BasePort.Capacity.Multi, typeof(float))
        {
            HideLabel = true
        });
    }

    public object GetValue(string port)
    {
        var inputAValue = Ports["InputA"].GetConnectionValue<float>();
        var inputBValue = Ports["InputB"].GetConnectionValue<float>();
        return inputAValue - inputBValue;
    }

    float IGetPortValue<float>.GetValue(string port)
    {
        var inputAValue = Ports["InputA"].GetConnectionValue<float>();
        var inputBValue = Ports["InputB"].GetConnectionValue<float>();
        return inputAValue - inputBValue;
    }
}
