#if UNITY_EDITOR
namespace Atom.GraphProcessor.Editors
{
    public class GraphViewContext
    {
        public BaseGraphWindow window;

        public CommandDispatcher commandDispatcher;
    }
}
#endif