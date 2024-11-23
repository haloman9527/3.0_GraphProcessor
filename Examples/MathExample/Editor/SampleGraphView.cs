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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

#if UNITY_EDITOR
using Jiange;
using Jiange.GraphProcessor;
using Jiange.GraphProcessor.Editors;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SampleGraphView : BaseGraphView
{
    public SampleGraphView(BaseGraphProcessor graph, BaseGraphWindow window, CommandDispatcher commandDispatcher) : base(graph, window, commandDispatcher)
    {
    }

    protected override void BuildNodeMenu(NodeMenuWindow nodeMenu)
    {
        foreach (var nodeType in GetNodeTypes())
        {
            if (nodeType.IsAbstract) 
                continue;
            var nodeStaticInfo = GraphProcessorUtil.NodeStaticInfos[nodeType];
            if (nodeStaticInfo.hidden)
                continue;
            
            var path = nodeStaticInfo.path;
            var menu = nodeStaticInfo.menu;
            nodeMenu.entries.Add(new NodeMenuWindow.NodeEntry(path, menu, nodeType));
        }
    }
    private IEnumerable<Type> GetNodeTypes()
    {
        yield return typeof(FloatNode);
        yield return typeof(AddNode);
        yield return typeof(SubNode);
        yield return typeof(LogNode);
    }

    protected override BaseConnectionView NewConnectionView(BaseConnectionProcessor connection)
    {
        return new SampleConnectionView();
    }
}
#endif