using Jiange.GraphProcessor;
using Jiange;

public abstract class FlowNode : BaseNode
{
    
}

[ViewModel(typeof(FlowNode))]
public abstract class FlowNodeProcessor : BaseNodeProcessor
{
    public FlowNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new BasePortProcessor(ConstValues.FLOW_IN_PORT_NAME, BasePort.Orientation.Horizontal, BasePort.Direction.Left, BasePort.Capacity.Multi));
        AddPort(new BasePortProcessor(ConstValues.FLOW_OUT_PORT_NAME, BasePort.Orientation.Horizontal, BasePort.Direction.Right, BasePort.Capacity.Single));
    }

    protected abstract void Execute();

    public void FlowNext()
    {
        FlowTo(ConstValues.FLOW_OUT_PORT_NAME);
    }

    public void FlowTo(string port)
    {
        foreach (FlowNodeProcessor item in GetConnections(ConstValues.FLOW_OUT_PORT_NAME))
        {
            if (item == null)
                continue;
            
            item.Execute();
        }
    }
}
