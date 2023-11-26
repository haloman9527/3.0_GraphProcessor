using CZToolKit.GraphProcessor;
using CZToolKit.VM;
using UnityEngine;

[NodeMenu("Start")]
public class StartNode : FlowNode
{
    
}

[ViewModel(typeof(StartNode))]
public class StartNodeVM : FlowNodeVM
{
    public StartNodeVM(BaseNode model) : base(model)
    {
        
    }

    protected override void Execute()
    {
        Debug.Log("Start");
        FlowNext();
    }
}
