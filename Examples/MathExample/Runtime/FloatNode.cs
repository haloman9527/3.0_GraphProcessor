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
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using CZToolKit;
using CZToolKit.GraphProcessor;

[NodeTooltip("浮点数节点")]
[NodeMenu("Float")]
public class FloatNode : BaseNode
{
    public float num;
}

[ViewModel(typeof(FloatNode))]
public class FloatNodeProcessor : BaseNodeProcessor, IGetPortValue, IGetPortValue<float>
{
    public FloatNode T_Model
    {
        get;
    }

    public float Value
    {
        get { return GetPropertyValue<float>(nameof(FloatNode.num)); }
        set { SetPropertyValue(nameof(FloatNode.num), value); }
    }

    public FloatNodeProcessor(FloatNode model) : base(model)
    {
        T_Model = model;
        this[nameof(FloatNode.num)] = new BindableProperty<float>(() => T_Model.num, v => T_Model.num = v);
        AddPort(new BasePortProcessor("Flow-Out", BasePort.Orientation.Horizontal, BasePort.Direction.Right, BasePort.Capacity.Multi, typeof(object)));
    }

    public object GetValue(string port)
    {
        return T_Model.num;
    }

    float IGetPortValue<float>.GetValue(string port)
    {
        return T_Model.num;
    }
}
