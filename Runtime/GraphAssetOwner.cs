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
    public abstract class GraphAssetOwner<TGraphAsset, TGraph> : InternalGraphAssetOwner
        where TGraphAsset : BaseGraphAsset<TGraph>
        where TGraph : BaseGraph, new()
    {
        #region 字段
        [NonSerialized] TGraph graph;
        [SerializeField] TGraphAsset graphAsset;
        #endregion

        #region 属性
        public override InternalBaseGraphAsset GraphAsset
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

        public override Type GraphAssetType { get { return typeof(TGraphAsset); } }

        public override Type GraphType { get { return typeof(TGraph); } }

        public TGraphAsset T_GraphAsset { get { return graphAsset; } }

        public override BaseGraph Graph { get { return T_Graph; } }

        public TGraph T_Graph
        {
            get
            {
                if (graph == null)
                {
                    if (graphAsset != null)
                        graph = graphAsset.DeserializeTGraph();
                    else
                        graph = DeserializeTGraph();
                }
                return graph;
            }
        }
        #endregion

        #region Serialize
        [HideInInspector]
        [SerializeField]
        string serializedGraph = "";
        [HideInInspector]
        [SerializeField]
        List<UnityObject> graphUnityReferences;

        public override void SaveGraph(BaseGraph graph)
        {
            if (GraphAsset != null)
            {
                graphAsset.SaveGraph(graph);
            }
            else
            {
                serializedGraph = GraphSerializer.SerializeValue(graph, out graphUnityReferences);
            }
        }

        public override BaseGraph DeserializeGraph()
        {
            var graph = GraphSerializer.DeserializeValue<TGraph>(serializedGraph, graphUnityReferences);
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
