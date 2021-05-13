using CZToolKit.GraphProcessor.Editors;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Examples
{
    [CustomGraphWindow(typeof(MathGraph))]
    public class MathGraphWindow : BaseGraphWindow
    {
        protected override void OnInitializedWindow()
        {
            titleContent = new GUIContent("Examples.Math");
        }

        protected override BaseGraphView InitializeGraphView(BaseGraph _graphData)
        {
            MathGraphView graphView = new MathGraphView();
            graphView.Initialize(this, _graphData);
            return graphView;
        }
    }
}
