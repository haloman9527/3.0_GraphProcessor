using CZToolKit.GraphProcessor;
using CZToolKit.VM;

[NodeMenu("Start")]
public class StartNode : FlowNode
{
    
}

[ViewModel(typeof(StartNode))]
public class StartNodeVM : FlowNodeVM
{
    public StartNodeVM(StartNode model) : base(model)
    {
        
    }

    protected override void Execute()
    {
        FlowNext();
    }
}
