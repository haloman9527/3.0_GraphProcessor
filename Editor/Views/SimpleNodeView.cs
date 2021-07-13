
namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class SimpleNodeView<M> : BaseNodeView<M> where M : BaseNode
    {
        protected SimpleNodeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.SimpleNodeViewStyle);
            titleContainer.Insert(0, inputContainer);
            titleContainer.Add(outputContainer);
            titleContainer.Add(topContainer);
        }
    }
}
