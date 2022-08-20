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
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using CZToolKit.GraphProcessor;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

[CreateAssetMenu]
public class SampleGraphAsset : ScriptableObject, IGraphAsset, IGraphAsset<SampleGraph>
{
    [HideInInspector]
    [SerializeField]
    byte[] serializedGraph;
    [HideInInspector]
    [SerializeField]
    List<UnityObject> graphUnityReferences = new List<UnityObject>();

    public Type GraphType => typeof(SampleGraph);

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
}
