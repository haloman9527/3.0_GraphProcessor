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
#if UNITY_EDITOR
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
            Toolbar.style.height = 20;
            Toolbar.style.flexGrow = 1;
            Toolbar.StretchToParentWidth();
            Add(Toolbar);

            GraphViewElement = new VisualElement();
            GraphViewElement.name = "GraphView";
            GraphViewElement.StretchToParentSize();
            GraphViewElement.style.top = Toolbar.style.height;
            Add(GraphViewElement);
        }
    }
}
#endif