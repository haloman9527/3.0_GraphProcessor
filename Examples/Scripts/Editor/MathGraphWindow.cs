using GraphProcessor.Editors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor.Examples
{
    [CustomGraphWindow(typeof(MathGraph))]
    public class MathGraphWindow : BaseGraphWindow
    {
        protected override void InitializeWindow(BaseGraph graph)
        {
            base.InitializeWindow(graph);
            titleContent = new GUIContent("Examples.Math");
        }

        protected override void InitializeGraphView()
        {
            graphView = new MathGraphView();
            graphView.Initialize(this, GraphData);
        }
    }
}
