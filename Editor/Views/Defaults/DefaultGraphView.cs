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

namespace CZToolKit.GraphProcessor.Editors
{
    public sealed class DefaultGraphView : BaseGraphView<BaseGraph>
    {
        public DefaultGraphView(BaseGraph graph, BaseGraphWindow window, CommandDispatcher commandDispacter) : base(graph, window, commandDispacter) { }

        protected override BaseConnectionView NewConnectionView() { return new BaseConnectionView(); }
    }
}
