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
using System.Linq;
using UnityEditor;
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
        private List<SearchTreeEntry> tree;

        public void Initialize(string treeName, BaseGraphView graphView)
        {
            this.treeName = treeName;
            this.graphView = graphView;
            this.nodeEntries = graphView.GetNodeEntries().OrderBy(entry => entry.path).ToList();
            this.tree = new List<SearchTreeEntry>() { new SearchTreeGroupEntry(new GUIContent(treeName)) };
            CreateSearchTree(tree);
        }

        private void CreateSearchTree(List<SearchTreeEntry> tree)
        {
            HashSet<string> groups = new HashSet<string>();
            foreach (BaseGraphView.NodeEntry entry in nodeEntries)
            {
                if (entry.hidden)
                    continue;

                var nodePath = entry.path;
                var nodeName = entry.menu[entry.menu.Length - 1];
                var level = 0;

                if (entry.menu.Length > 1)
                {
                    level++;
                    var groupPath = "";
                    for (int i = 0; i < entry.menu.Length - 1; i++)
                    {
                        var title = entry.menu[i];
                        groupPath += title;
                        if (!groups.Contains(groupPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title))
                            {
                                level = level
                            });
                            groups.Add(groupPath);
                        }
                    }
                }

                tree.Add(new SearchTreeEntry(new GUIContent(nodeName))
                {
                    level = level + 1,
                    userData = entry.nodeType
                });

                // var menuAttribute = GraphProcessorEditorUtil.GetNodeMenu(type);
                // if (menuAttribute != null)
                // {
                //     if (entry.titles.Length > 1)
                //     {
                //         SearchTreeGroupEntry groupTemp = null;
                //         for (int i = 1; i < entry.menu.Length; i++)
                //         {
                //             SearchTreeGroupEntry group = tempTree.Find(item =>
                //                     (item.content.text == menuAttribute.titles[i - 1] && item.level == i)) as
                //                 SearchTreeGroupEntry;
                //             if (group == null)
                //             {
                //                 group = new SearchTreeGroupEntry(new GUIContent(menuAttribute.titles[i - 1]), i);
                //                 int index = groupTemp == null ? 0 : tempTree.IndexOf(groupTemp);
                //                 tempTree.Insert(index + 1, group);
                //             }
                //
                //             groupTemp = group;
                //         }
                //
                //         tempTree.Insert(tempTree.IndexOf(groupTemp) + 1,
                //             new SearchTreeEntry(new GUIContent(menuAttribute.titles.Last()))
                //                 { userData = type, level = menuAttribute.titles.Length });
                //     }
                //     else
                //     {
                //         tempTree.Add(new SearchTreeEntry(new GUIContent(menuAttribute.titles.Last()))
                //             { userData = type, level = 1 });
                //     }
                // }
                // else
                // {
                //     GUIContent content = new GUIContent(type.Name);
                //     tempTree.Add(new SearchTreeEntry(content) { userData = type, level = 1 });
                // }
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