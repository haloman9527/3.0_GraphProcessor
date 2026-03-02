using Atom.GraphProcessor;
using Atom;
using Sirenix.OdinInspector;
using System;

[Serializable]
public abstract class BTBaseNode : BaseNode
{
    [LabelText("节点注释")]
    public string comment;
}

[ViewModel(typeof(BTBaseNode))]
public abstract class BTBaseNodeProcessor : BaseNodeProcessor
{
    public BTBaseNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new PortProcessor(ConstValues.FLOW_IN_PORT_NAME, BasePort.Direction.Left, BasePort.Capacity.Multi));
        AddPort(new PortProcessor(ConstValues.FLOW_OUT_PORT_NAME, BasePort.Direction.Right, BasePort.Capacity.Multi));
    }

    protected abstract void Execute();

    public void FlowNext()
    {
        FlowTo(ConstValues.FLOW_OUT_PORT_NAME);
    }

    public void FlowTo(string port)
    {
        foreach (BTBaseNodeProcessor item in GetPortConnections(ConstValues.FLOW_OUT_PORT_NAME))
        {
            if (item == null)
                continue;
            
            item.Execute();
        }
    }
}
