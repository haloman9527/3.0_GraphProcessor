using System.Collections.Generic;
using CZToolKit.GraphProcessor;
using CZToolKit.VM;
using Sirenix.OdinInspector;

[NodeMenu("刷新Wwise信息")]
public class RefreshWwiseNode : FlowNode
{
    
}

[ViewModel(typeof(RefreshWwiseNode))]
public class RefreshWwiseNodeVM : FlowNodeVM
{
    public RefreshWwiseNodeVM(RefreshWwiseNode model) : base(model)
    {
        
    }

    protected override void Execute()
    {
        FlowNext();
    }
}
