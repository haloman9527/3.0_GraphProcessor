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
using CZToolKit.Common.ViewModel;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphAssetOwner<TGraphAsset, TGraph> : MonoBehaviour, IGraphAssetOwner, IGraphSerialization
        where TGraphAsset : UnityObject, IGraphAsset
        where TGraph : BaseGraphVM
    {
        #region Fields
        [SerializeField] TGraphAsset graphAsset = null;
        [SerializeField] TGraph graph = null;
        #endregion

        #region Properties
        public UnityObject GraphAsset
        {
            get { return graphAsset; }
        }

        public BaseGraphVM Graph
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
                        graph = ViewModelFactory.CreateViewModel(graphData) as TGraph;
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
