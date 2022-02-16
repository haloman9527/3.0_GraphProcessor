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
    public abstract class InternalBaseGraphAsset : ScriptableObject, IGraphAsset
    {
        public abstract Type GraphType { get; }
        public abstract void SaveGraph(BaseGraph graph);
        public abstract BaseGraph DeserializeGraph();
    }
}