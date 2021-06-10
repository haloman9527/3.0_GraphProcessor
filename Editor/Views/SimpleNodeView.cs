
namespace CZToolKit.GraphProcessor.Editors
{
    public class SimpleNodeView : BaseNodeView
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
            styleSheets.Add(GraphProcessorStyles.SimpleNodeViewStyle);

            titleContainer.Insert(0, inputContainer);
            titleContainer.Add(outputContainer);
            titleContainer.Add(topContainer);
        }

        public override bool RefreshPorts()
        {
            bool result = base.RefreshPorts();
            titleContainer.Insert(0, inputContainer);
            titleContainer.Add(outputContainer);
            titleContainer.Add(topContainer);
            return result;
        }
    }
}
