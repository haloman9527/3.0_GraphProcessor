using System;
using System.Collections.Generic;
using CZToolKit.GraphProcessor;
using CZToolKit.VM;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

using UnityObject = UnityEngine.Object;

[CreateAssetMenu]
public class FlowGraphAsset : ScriptableObject, IGraphAsset, IGraphAsset<FlowGraph>
{
    [HideInInspector] public byte[] serializedGraph;
    [HideInInspector] public List<UnityObject> graphUnityReferences = new List<UnityObject>();

    public Type GraphType => typeof(FlowGraph);
    public UnityObject UnityAsset => this;

    public void SaveGraph(BaseGraph graph)
    {
        serializedGraph = SerializationUtility.SerializeValue(graph, DataFormat.Binary, out graphUnityReferences);
    }

    public BaseGraph DeserializeGraph()
    {
        return DeserializeTGraph();
    }

    public FlowGraph DeserializeTGraph()
    {
        FlowGraph graph = null;
        if (serializedGraph != null && serializedGraph.Length > 0)
            graph = SerializationUtility.DeserializeValue<FlowGraph>(serializedGraph, DataFormat.Binary, graphUnityReferences);
        if (graph == null)
            graph = new FlowGraph();

        return graph;
    }

    [Button]
    public void Reset()
    {
        SaveGraph(new FlowGraph());
    }
}

public class FlowGraph : BaseGraph
{
    public int startNodeID;
}

[ViewModel(typeof(FlowGraph))]
public class FlowGraphVM : BaseGraphVM
{
    private StartNodeVM StartNode { get; }

    public FlowGraphVM(FlowGraph model) : base(model)
    {
        if (Nodes.TryGetValue(model.startNodeID, out var _node) && _node is StartNodeVM)
        {
            StartNode = _node as StartNodeVM;
        }

        if (StartNode == null)
        {
            StartNode = AddNode(new StartNode() { position = new InternalVector2Int(100, 100) }) as StartNodeVM;
            model.startNodeID = StartNode.ID;
        }
    }
}