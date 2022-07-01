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
using CZToolKit.Core.SharedVariable;
using CZToolKit.GraphProcessor.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphAssetOwner<TGraphAsset, TGraph> : InternalGraphAssetOwner, IGraphSerialization
        where TGraphAsset : UnityObject, IGraphAsset<TGraph>
        where TGraph : BaseGraph, IGraphForMono, new()
    {
        #region 字段
        [SerializeField] TGraphAsset graphAsset;
        [NonSerialized] TGraph graph;
        #endregion

        #region 属性
        public override UnityObject GraphAsset
        {
            get { return graphAsset; }
            set
            {
                if (graphAsset != value)
                {
                    graphAsset = value as TGraphAsset;
                    graph = null;
                    if (graphAsset != null)
                    {
                        foreach (var variable in T_Graph.Variables)
                        {
                            if (GetVariable(variable.GUID) == null)
                                SetVariable(variable.Clone() as SharedVariable);
                        }
                    }
                }
            }
        }

        public TGraphAsset T_GraphAsset
        {
            get { return graphAsset; }
        }

        public override IGraph Graph
        {
            get { return T_Graph; }
        }

        public TGraph T_Graph
        {
            get
            {
                if (graph == null)
                {
                    if (T_GraphAsset != null)
                        graph = T_GraphAsset.DeserializeTGraph();
                    else
                        graph = DeserializeTGraph();
                }
                return graph;
            }
        }

        public override Type GraphAssetType
        {
            get { return typeof(TGraphAsset); }
        }

        public override Type GraphType
        {
            get { return typeof(TGraph); }
        }
        #endregion

        #region Serialize
        [HideInInspector]
        [SerializeField]
        byte[] serializedGraph;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> graphUnityReferences;

        public void SaveGraph(IGraph graph)
        {
            if (GraphAsset != null)
                graphAsset.SaveGraph(graph);
            else
                serializedGraph = Sirenix.Serialization.SerializationUtility.SerializeValue(graph, Sirenix.Serialization.DataFormat.JSON, out graphUnityReferences);
        }

        public BaseGraph DeserializeGraph()
        {
            TGraph graph = null;
            if (serializedGraph != null && serializedGraph.Length > 0)
                graph = Sirenix.Serialization.SerializationUtility.DeserializeValue<TGraph>(serializedGraph, Sirenix.Serialization.DataFormat.JSON, graphUnityReferences);
            if (graph == null)
                graph = new TGraph();
            graph.Enable();
            return graph;
        }

        public TGraph DeserializeTGraph()
        {
            return DeserializeGraph() as TGraph;
        }
        #endregion
    }
}
