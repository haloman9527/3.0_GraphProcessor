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

namespace Atom.GraphProcessor
{
    public interface IGraphAsset
    {
        Type GraphType { get; }
        
        void SaveGraph(BaseGraph graph);
        
        BaseGraph LoadGraph();
    }
}