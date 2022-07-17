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
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphAssetOwner<TGraphAsset, TGraph> : InternalGraphAssetOwner, IGraphSerialization
        where TGraphAsset : UnityObject, IGraphAsset
        where TGraph : BaseGraphVM
    {
        #region Fields
        [SerializeField] TGraphAsset graphAsset;
        [SerializeField] TGraph graph;
        #endregion

        #region Properties
        public override UnityObject GraphAsset
        {
            get { return graphAsset; }
        }

        public override BaseGraphVM Graph
        {
            get { return T_Graph; }
        }

        public TGraphAsset T_GraphAsset
        {
            get { return graphAsset; }
        }

        public virtual TGraph T_Graph
        {
            get
            {
                if (graph == null && graphAsset != null)
                {
                    var graphData = DeserializeGraph();
                    if (graphData != null)
                    {
                        graph = GraphProcessorUtil.CreateViewModel(graphData) as TGraph;
                    }
                }
                return graph;
            }
            set { graph = value; }
        }
        #endregion

        #region Serialize
        public abstract void SaveGraph(BaseGraph graph);

        public abstract BaseGraph DeserializeGraph();

        public TGraph DeserializeTGraph()
        {
            return DeserializeGraph() as TGraph;
        }
        #endregion
    }
}
