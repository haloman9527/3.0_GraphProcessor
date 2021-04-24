using GraphProcessor.Editors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor.Examples
{

    public class MathGraphView : BaseGraphView
    {
        protected override IEnumerable<Type> GetNodeTypes()
        {
            yield return typeof(StringNode);
            yield return typeof(FloatNode);
            yield return typeof(AddNode);
            yield return typeof(DebugNode);
        }
    }
}
