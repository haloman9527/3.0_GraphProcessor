using System;
using System.Collections;
using System.Collections.Generic;
using CZToolKit.GraphProcessor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu]
public class FlowGraphAsset : ScriptableObject, IGraphAsset, IGraphAsset<FlowGraph>
{
    [HideInInspector]
    public byte[] serializedGraph;
    [HideInInspector]
    public List<Object> graphUnityReferences = new List<Object>();

    public Type GraphType => typeof(FlowGraph);

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
        {
            graph = new FlowGraph();
        }
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
    
}

public class FlowGraphVM : BaseGraphVM
{
    public FlowGraphVM(BaseGraph model) : base(model)
    {
        
    }
}