using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class SimpleNodeView : BaseNodeView
    {
        const string SimpleNodeViewStyleFile = "GraphProcessor/Styles/SimpleNodeView";

        static StyleSheet simpleNodeViewStyle;
        public static StyleSheet SimpleNodeViewStyle
        {
            get
            {
                if (simpleNodeViewStyle == null)
                    simpleNodeViewStyle = Resources.Load<StyleSheet>(SimpleNodeViewStyleFile);
                return simpleNodeViewStyle;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            styleSheets.Add(SimpleNodeViewStyle);

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
