using CZToolKit.GraphProcessor;
using CZToolKit;

public abstract class FlowNode : BaseNode
{
    
}

[ViewModel(typeof(FlowNode))]
public abstract class FlowNodeProcessor : BaseNodeProcessor
{
    public FlowNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new BasePortProcessor("FlowIn", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Multi));
        AddPort(new BasePortProcessor("FlowOut", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Single));
    }

    protected abstract void Execute();

    public void FlowNext()
    {
        FlowTo("FlowOut");
    }

    public void FlowTo(string port)
    {
        foreach (FlowNodeProcessor item in GetConnections("FlowOut"))
        {
            if (item == null)
                continue;
            
            item.Execute();
        }
    }
}
