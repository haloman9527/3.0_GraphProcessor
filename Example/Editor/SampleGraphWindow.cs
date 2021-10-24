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

[CustomGraphWindow(typeof(SampleGraph))]
public class SampleGraphWindow : BaseGraphWindow
{
    protected override BaseGraphView NewGraphView(BaseGraph graph, CommandDispatcher commandDispatcher)
    {
        return new SampleGraphView(graph, this, commandDispatcher);
    }
}
