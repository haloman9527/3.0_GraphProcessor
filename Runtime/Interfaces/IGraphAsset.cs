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

namespace CZToolKit.GraphProcessor
{
    public interface IGraphAsset
    {
        Type GraphType { get; }
        void SaveGraph(IGraph graph);
        BaseGraph DeserializeGraph();
    }

    public interface IGraphAsset<T> : IGraphAsset where T : BaseGraph, IGraph, new()
    {
        T DeserializeTGraph();
    }
}