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
    public interface IGraphAsset : IGraphSerialization
    {
        Type GraphType { get; }
    }

    public interface IGraphAsset<T> : IGraphSerialization<T> where T : BaseGraph, new()
    {
        Type GraphType { get; }
    }
}