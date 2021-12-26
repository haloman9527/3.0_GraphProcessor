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
using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Internal
{
    public abstract class InternalBaseGraphAsset : ScriptableObject, IGraphAsset, ICloneable
    {
        #region Properties
        public abstract BaseGraph Graph
        {
            get;
        }
        #endregion

        public abstract void SaveGraph();

        public abstract void CheckGraphSerialization();

        public virtual object Clone() { return Instantiate(this); }
    }
}