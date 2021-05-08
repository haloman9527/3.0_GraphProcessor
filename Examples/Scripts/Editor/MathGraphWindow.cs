using CZToolKit.GraphProcessor.Editors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Examples
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
            GraphView = new MathGraphView();
            GraphView.Initialize(this, GraphData);
        }
    }
}
