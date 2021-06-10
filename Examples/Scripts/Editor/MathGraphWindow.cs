using CZToolKit.GraphProcessor.Editors;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Examples
{
    [CustomGraphWindow(typeof(MathGraph))]
    public class MathGraphWindow : BaseGraphWindow
    {
        protected override void OnLoadedGraph()
        {
            titleContent = new GUIContent("Examples.Math");
        }

        protected override BaseGraphView CreateGraphView(IGraph _graph)
        {
            MathGraphView graphView = new MathGraphView(_graph, CommandDispatcher, this);
            return graphView;
        }
    }
}
