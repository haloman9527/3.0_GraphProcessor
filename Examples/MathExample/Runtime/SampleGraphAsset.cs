#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using CZToolKit.GraphProcessor;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

using UnityObject = UnityEngine.Object;

[CreateAssetMenu]
public class SampleGraphAsset : ScriptableObject, IGraphAsset, IGraphAsset<SampleGraph>
{
    [HideInInspector]
    public byte[] serializedGraph;
    [HideInInspector]
    public List<UnityObject> graphUnityReferences = new List<UnityObject>();

    public Type GraphType => typeof(SampleGraph);
    
    public UnityObject UnityAsset => this;

    public void SaveGraph(BaseGraph graph)
    {
        serializedGraph = SerializationUtility.SerializeValue(graph, DataFormat.Binary, out graphUnityReferences);
    }

    public BaseGraph DeserializeGraph()
    {
        return DeserializeTGraph();
    }

    public SampleGraph DeserializeTGraph()
    {
        SampleGraph graph = null;
        if (serializedGraph != null && serializedGraph.Length > 0)
            graph = SerializationUtility.DeserializeValue<SampleGraph>(serializedGraph, DataFormat.Binary, graphUnityReferences);
        if (graph == null)
        {
            graph = new SampleGraph();
        }
        return graph;
    }

    [Button]
    public void Reset()
    {
        SaveGraph(new SampleGraph());
    }
}
