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
using CZToolKit.GraphProcessor.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public abstract class BaseGraphAsset<GraphClass> : InternalBaseGraphAsset where GraphClass : BaseGraph, new()
    {
        public override Type GraphType => typeof(GraphClass);

        #region Serialize
        [HideInInspector]
        [SerializeField]
        [TextArea(20, 20)]
        string serializedGraph = String.Empty;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> graphUnityReferences = new List<UnityObject>();

        public override void SaveGraph(BaseGraph graph)
        {
            serializedGraph = GraphSerializer.SerializeValue(graph, out graphUnityReferences);
        }

        public override sealed BaseGraph DeserializeGraph()
        {
            var graph = GraphSerializer.DeserializeValue<GraphClass>(serializedGraph, graphUnityReferences);
            if (graph == null)
                graph = new GraphClass();
            graph.Enable();
            return graph;
        }

        public GraphClass DeserializeTGraph()
        {
            return DeserializeGraph() as GraphClass;
        }
        #endregion
    }
}