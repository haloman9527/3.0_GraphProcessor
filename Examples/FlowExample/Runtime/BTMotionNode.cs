using System.Collections.Generic;
using Atom.GraphProcessor;
using Atom;
using Sirenix.OdinInspector;
using System;

[Serializable]
[NodeMenu("动画状态")]
public class BTMotionNode : BTBaseNode
{
    [LabelText("动画名称")]
    public string configmotionName;

    [ReadOnly]
    public string resPath;
}

[ViewModel(typeof(BTMotionNode))]
public class BTMotionNodeProcessor : BTBaseNodeProcessor
{
    public BTMotionNodeProcessor(BTMotionNode model) : base(model)
    {
        
    }

    protected override void Execute()
    {
        FlowNext();
    }
}
