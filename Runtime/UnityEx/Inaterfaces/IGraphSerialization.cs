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

namespace CZToolKit.GraphProcessor
{
    public interface IGraphSerialization
    {
        void SaveGraph(IGraph graph);
        BaseGraph DeserializeGraph();
    }

    public interface IGraphSerialization<T> : IGraphSerialization where T : BaseGraph, IGraph, new()
    {
        T DeserializeTGraph();
    }
}
