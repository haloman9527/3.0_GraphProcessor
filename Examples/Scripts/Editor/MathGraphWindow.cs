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
using CZToolKit.GraphProcessor.Editors;
using System;
using System.Collections.Generic;
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

        protected override BaseGraphView GenerateGraphView(BaseGraph _graph)
        {
            return new MathGraphView(_graph,CommandDispatcher,this);
        }
    }

    public class MathGraphView : BaseGraphView
    {
        public MathGraphView(BaseGraph _graph, CommandDispatcher _commandDispatcher, BaseGraphWindow _window) : base(_graph, _commandDispatcher, _window) { }

        protected override IEnumerable<Type> GetNodeTypes()
        {
            yield return typeof(StringNode);
            yield return typeof(SampleNode);
            yield return typeof(FloatNode);
            yield return typeof(AddNode);
            yield return typeof(DebugNode);
            yield return typeof(SharedVariableNode);
        }
    }
}
