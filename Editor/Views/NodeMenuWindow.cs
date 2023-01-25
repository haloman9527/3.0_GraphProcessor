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
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class NodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        private BaseGraphView graphView;
        private List<BaseGraphView.NodeEntry> nodeEntries;
        private List<SearchTreeEntry> tree;

        public void Initialize(string treeName, BaseGraphView graphView, List<BaseGraphView.NodeEntry> nodeEntries)
        {
            this.graphView = graphView;
            this.nodeEntries = nodeEntries;
            this.tree = new List<SearchTreeEntry>(nodeEntries.Count + 1);
            this.tree.Add(new SearchTreeGroupEntry(new GUIContent(treeName)));
            CreateSearchTree(tree);
        }

        private void CreateSearchTree(List<SearchTreeEntry> tree)
        {
            HashSet<string> groups = new HashSet<string>();
            foreach (var entry in nodeEntries)
            {
                if (entry.hidden)
                    continue;

                var nodeName = entry.menu[entry.menu.Length - 1];

                if (entry.menu.Length > 1)
                {
                    var groupPath = "";
                    for (int i = 0; i < entry.menu.Length - 1; i++)
                    {
                        var title = entry.menu[i];
                        groupPath += title;
                        if (!groups.Contains(groupPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title))
                            {
                                level = i + 1
                            });
                            groups.Add(groupPath);
                        }
                    }
                }

                tree.Add(new SearchTreeEntry(new GUIContent(nodeName))
                {
                    level = entry.menu.Length,
                    userData = entry.nodeType
                });
            }
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = graphView.GraphWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - graphView.GraphWindow.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            graphView.CommandDispatcher.Do(new AddNodeCommand(graphView.ViewModel, searchTreeEntry.userData as Type, graphMousePosition.ToInternalVector2()));
            graphView.GraphWindow.Focus();
            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return tree;
        }
    }
}
#endif