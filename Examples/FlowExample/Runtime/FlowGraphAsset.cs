using System;
using Moyo.GraphProcessor;
using Moyo;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class FlowGraphAsset : ScriptableObject, IGraphAsset
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
    public int startNodeID;
}

[ViewModel(typeof(FlowGraph))]
public class FlowGraphProcessor : BaseGraphProcessor
{
    private StartNodeProcessor StartNode { get; }

    public FlowGraphProcessor(FlowGraph model) : base(model)
    {
        if (Nodes.TryGetValue(model.startNodeID, out var _node) && _node is StartNodeProcessor)
        {
            StartNode = _node as StartNodeProcessor;
        }

        if (StartNode == null)
        {
            StartNode = AddNode(new StartNode() { position = new InternalVector2Int(100, 100) }) as StartNodeProcessor;
            model.startNodeID = StartNode.ID;
        }
    }
}