namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BaseGraph))]
    public class DefaultGraphView : BaseGraphView
    {
        public DefaultGraphView(BaseGraphVM graph, BaseGraphWindow window, CommandDispatcher commandDispatcher) : base(graph, window, commandDispatcher)
        {
        }
    }
}