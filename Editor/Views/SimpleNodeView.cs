using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class SimpleNodeView : BaseNodeView
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
            titleButtonContainer.style.display = DisplayStyle.None;

            inputContainer.style.alignItems = Align.Center;
            outputContainer.style.alignItems = Align.Center;

            inputContainer.style.flexDirection = FlexDirection.Row;
            outputContainer.style.flexDirection = FlexDirection.Row;

            titleContainer.Insert(0, inputContainer);
            titleContainer.Add(outputContainer);

            titleContainer.Add(topContainer);
            titleContainer.style.alignItems = Align.Stretch;
            TitleLabel.style.alignSelf = Align.Center;
        }
    }
}
