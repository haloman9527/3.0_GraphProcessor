using Atom.GraphProcessor;
using Atom;

[NodeMenu("行为开始节点")]
public class BTStartNode : BTBaseNode
{    
    public float CD;
}

[ViewModel(typeof(BTStartNode))]
public class BTStartNodeProcessor : BTBaseNodeProcessor
{
    public BTStartNodeProcessor(BTStartNode model) : base(model)
    {
    }

    protected override void Execute()
    {
        FlowNext();
    }
}