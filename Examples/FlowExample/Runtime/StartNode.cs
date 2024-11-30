﻿using Moyo.GraphProcessor;
using Moyo;

[NodeMenu("Start", hidden = true)]
public class StartNode : FlowNode
{
}

[ViewModel(typeof(StartNode))]
public class StartNodeProcessor : FlowNodeProcessor
{
    public StartNodeProcessor(StartNode model) : base(model)
    {
    }

    protected override void Execute()
    {
        FlowNext();
    }
}