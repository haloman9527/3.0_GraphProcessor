using System;
using Atom.GraphProcessor;
using Atom;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class BTFlowGraphAsset : ScriptableObject, IGraphAsset
{
    [SerializeField] 
    private FlowGraph data;

    public Type GraphType => typeof(FlowGraph);

    public void SaveGraph(BaseGraph graph) => this.data = (FlowGraph)graph;

    public BaseGraph LoadGraph() => data;

    [Button]
    public void Reset()
    {
        SaveGraph(new FlowGraph());
    }
}

[Serializable]
public class FlowGraph : BaseGraph
{
    public long startNodeID;
}

[ViewModel(typeof(FlowGraph))]
public class FlowGraphProcessor : BaseGraphProcessor
{
    private BTStartNodeProcessor StartNode { get; }

    public FlowGraphProcessor(FlowGraph model) : base(model)
    {
        if (Nodes.TryGetValue(model.startNodeID, out var _node) && _node is BTStartNodeProcessor)
        {
            StartNode = _node as BTStartNodeProcessor;
        }

        if (StartNode == null)
        {
            StartNode = AddNode(new BTStartNode() { position = new InternalVector2Int(100, 100) }) as BTStartNodeProcessor;
            model.startNodeID = StartNode.ID;
        }
    }
}