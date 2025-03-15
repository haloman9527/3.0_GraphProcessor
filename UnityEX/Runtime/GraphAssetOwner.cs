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
 *  Blog: https://www.haloman.net/
 *
 */
#endregion

using System;
using Atom;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Atom.GraphProcessor
{
    public abstract class GraphAssetOwner<TGraphAsset, TGraph> : MonoBehaviour, IGraphAssetOwner
        where TGraphAsset : UnityObject, IGraphAsset
        where TGraph : BaseGraphProcessor
    {
        #region Fields
        private TGraph graph = null;
        [SerializeField]
        private TGraphAsset graphAsset = null;
        #endregion

        #region Properties
        
        public IGraphAsset GraphAsset
        {
            get { return graphAsset; }
        }

        public BaseGraphProcessor Graph
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
                    var graphData = graphAsset.LoadGraph();
                    graph = ViewModelFactory.ProduceViewModel(graphData) as TGraph;
                }
                
                return graph;
            }
            set { graph = value; }
        }
        #endregion
    }
}
