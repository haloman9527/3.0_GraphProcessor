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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class NodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        private string treeName;
        private BaseGraphView graphView;
        private List<BaseGraphView.NodeEntry> nodeEntries;

        public void Initialize(string treeName, BaseGraphView graphView, List<BaseGraphView.NodeEntry> nodeEntries)
        {
            this.treeName = treeName;
            this.graphView = graphView;
            this.nodeEntries = nodeEntries;
        }

        private void CreateSearchTree(List<SearchTreeEntry> tree)
        {
            HashSet<string> groups = new HashSet<string>();
            foreach (var nodeEntry in nodeEntries)
            {
                if (nodeEntry.hidden)
                    continue;

                var nodeName = nodeEntry.menu[nodeEntry.menu.Length - 1];

                if (nodeEntry.menu.Length > 1)
                {
                    var groupPath = "";
                    for (int i = 0; i < nodeEntry.menu.Length - 1; i++)
                    {
                        var title = nodeEntry.menu[i];
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
                    level = nodeEntry.menu.Length,
                    userData = nodeEntry
                });
            }
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = graphView.GraphWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - graphView.GraphWindow.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            var nodeEntry = searchTreeEntry.userData as BaseGraphView.NodeEntry;
            graphView.CommandDispatcher.Do(new AddNodeCommand(graphView.ViewModel, nodeEntry.nodeType, graphMousePosition.ToInternalVector2()));
            graphView.GraphWindow.Focus();
            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>(nodeEntries.Count + 1);
            tree.Add(new SearchTreeGroupEntry(new GUIContent(treeName)));
            CreateSearchTree(tree);
            return tree;
        }
    }
}
#endif