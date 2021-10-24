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
using CZToolKit.Core;
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using System;
using System.Collections.Generic;

public class SampleGraphView : BaseGraphView
{
    public SampleGraphView(BaseGraph graph, BaseGraphWindow window, CommandDispatcher commandDispacter) : base(graph, window, commandDispacter)
    {
    }

    protected override IEnumerable<Type> GetNodeTypes()
    {
        yield return typeof(SampleNode);
    }
}
