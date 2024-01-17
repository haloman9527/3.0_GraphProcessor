using System.Collections.Generic;
using CZToolKit.GraphProcessor;
using CZToolKit;
using Sirenix.OdinInspector;

[NodeMenu("刷新Wwise信息")]
public class RefreshWwiseNode : FlowNode
{
    
}

[ViewModel(typeof(RefreshWwiseNode))]
public class RefreshWwiseNodeProcessor : FlowNodeProcessor
{
    public RefreshWwiseNodeProcessor(RefreshWwiseNode model) : base(model)
    {
        
    }

    protected override void Execute()
    {
        FlowNext();
    }
}
