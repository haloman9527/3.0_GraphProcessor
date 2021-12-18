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
using UnityEngine;


namespace CZToolKit.GraphProcessor
{
    public abstract class GraphAssetOwner<TGraphAsset, TGraph> : InternalGraphAssetOwner
        where TGraphAsset : BaseGraphAsset<TGraph>
        where TGraph : BaseGraph, new()
    {
        #region 字段
        [SerializeField]
        TGraphAsset graphAsset;
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

        public TGraphAsset T_GraphAsset { get { return graphAsset; } }

        public override BaseGraph Graph { get { return T_Graph; } }

        public TGraph T_Graph { get { return graphAsset?.T_Graph; } }

        #endregion
    }
}
