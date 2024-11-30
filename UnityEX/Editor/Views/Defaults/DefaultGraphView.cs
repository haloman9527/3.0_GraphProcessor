
#if UNITY_EDITOR
namespace Moyo.GraphProcessor.Editors
{
    [CustomView(typeof(BaseGraph))]
    public class DefaultGraphView : BaseGraphView
    {
        public DefaultGraphView(BaseGraphProcessor graph, BaseGraphWindow window, CommandDispatcher commandDispatcher) : base(graph, window, commandDispatcher)
        {
        }
    }
}
#endif