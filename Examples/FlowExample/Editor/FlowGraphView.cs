#if UNITY_EDITOR
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

using Atom;
using Atom.GraphProcessor;
using Atom.GraphProcessor.Editors;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FlowGraphView : BaseGraphView
{
    protected override void BuildNodeMenu(NodeMenuWindow nodeMenu)
    {
        foreach (var nodeInfo in GraphProcessorUtil.GetNodeStaticInfos())
        {
            if (nodeInfo.Hidden)
                continue;
            
            if (!typeof(FlowNode).IsAssignableFrom(nodeInfo.NodeType))
                continue;
            
            var path = nodeInfo.Path;
            var menu = nodeInfo.Menu;
            nodeMenu.entries.Add(new NodeMenuWindow.NodeEntry(path, menu, nodeInfo.NodeType));
        }
    }
    
    protected override BaseConnectionView NewConnectionView(BaseConnectionProcessor connection)
    {
        return new SampleConnectionView();
    }
}
#endif