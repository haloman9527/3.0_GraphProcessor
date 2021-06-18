#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class GraphViewParentElement : VisualElement
    {
        public ToolbarView Toolbar { get; private set; }
        public VisualElement GraphViewElement { get; private set; }

        public GraphViewParentElement()
        {
            name = "GraphViewParent";

            Toolbar = new ToolbarView();
            Toolbar.style.height = 25;
            Toolbar.style.flexGrow = 1;
            Toolbar.StretchToParentWidth();
            Add(Toolbar);

            GraphViewElement = new VisualElement();
            GraphViewElement.name = "GraphView";
            GraphViewElement.StretchToParentSize();
            GraphViewElement.style.top = Toolbar.style.height;
            Add(GraphViewElement);
        }

        public void SetUp(BaseGraphView _graphView)
        {
            GraphViewElement.Add(_graphView);
        }
    }
}
