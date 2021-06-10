using CZToolKit.GraphProcessor.Editors;
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor.Examples
{

    public class MathGraphView : BaseGraphView
    {
        public MathGraphView(IGraph _graph, CommandDispatcher _commandDispatcher, BaseGraphWindow _window) : base(_graph, _commandDispatcher, _window)
        {
        }

        protected override IEnumerable<Type> GetNodeTypes()
        {
            yield return typeof(StringNode);
            yield return typeof(SampleNode);
            yield return typeof(FloatNode);
            yield return typeof(AddNode);
            yield return typeof(DebugNode);
        }
    }
}
