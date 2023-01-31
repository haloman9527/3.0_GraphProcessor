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
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using System;
using System.Collections.Generic;
using CZToolKit.Common;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SampleGraphView : BaseGraphView
{
    public SampleGraphView(BaseGraphVM graph, BaseGraphWindow window, CommandDispatcher commandDispatcher) : base(graph, window, commandDispatcher)
    {
    }

    protected override void NodeCreationRequest(NodeCreationContext c)
    {
        var multiLayereEntryCount = 0;
        var entries = new List<NodeEntry>(16);
        foreach (var nodeType in GetNodeTypes())
        {
            var path = nodeType.Name;
            var menu = (string[])null;
            var hidden = false;
            var menuAttribute = GraphProcessorEditorUtil.GetNodeMenu(nodeType);
            if (menuAttribute != null)
            {
                path = menuAttribute.path;
                menu = menuAttribute.menu;
                hidden = menuAttribute.hidden;
            }
            else
            {
                menu = new string[] { nodeType.Name };
            }

            if (menu.Length > 1)
                multiLayereEntryCount++;
            entries.Add(new NodeEntry(nodeType, path, menu, hidden));
        }

        entries.QuickSort((a, b) => -(a.menu.Length.CompareTo(b.menu.Length)));
        entries.QuickSort(0, multiLayereEntryCount - 1, (a, b) => String.Compare(a.path, b.path, StringComparison.Ordinal));
        entries.QuickSort(multiLayereEntryCount, entries.Count - 1, (a, b) => String.Compare(a.path, b.path, StringComparison.Ordinal));

        var nodeMenu = ScriptableObject.CreateInstance<NodeMenuWindow>();
        nodeMenu.Initialize("Nodes", this, entries);
        SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), nodeMenu);
    }

    private IEnumerable<Type> GetNodeTypes()
    {
        yield return typeof(FloatNode);
        yield return typeof(AddNode);
        yield return typeof(SubNode);
        yield return typeof(LogNode);
    }

    protected override BaseConnectionView NewConnectionView(BaseConnectionVM connection)
    {
        return new SampleConnectionView();
    }
}
#endif