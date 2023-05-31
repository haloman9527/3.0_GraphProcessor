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
        #region Define
        
        public interface INodeEntry
        {
            string Path { get; }

            string[] Menu { get; }

            void CreateNode(BaseGraphView graphView, InternalVector2Int position);
        }

        public class NodeEntry : INodeEntry
        {
            private readonly string path;
            private readonly string[] menu;
            public readonly Type nodeType;

            public string Path
            {
                get { return path; }
            }

            public string[] Menu
            {
                get { return menu; }
            }

            public NodeEntry(string path, string[] menu, Type nodeType)
            {
                this.path = path;
                this.menu = menu;
                this.nodeType = nodeType;
            }

            public void CreateNode(BaseGraphView graphView, InternalVector2Int position)
            {
                graphView.CommandDispatcher.Do(new AddNodeCommand(graphView.ViewModel, nodeType, position));
            }
        }
        #endregion
        
        private string treeName;
        private BaseGraphView graphView;
        public List<INodeEntry> entries = new List<INodeEntry>(256);

        public void Initialize(string treeName, BaseGraphView graphView)
        {
            this.treeName = treeName;
            this.graphView = graphView;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = graphView.GraphWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - graphView.GraphWindow.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            var nodeEntry = searchTreeEntry.userData as INodeEntry;
            nodeEntry.CreateNode(graphView, graphMousePosition.ToInternalVector2Int());
            
            graphView.GraphWindow.Focus();
            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>(entries.Count + 1);
            tree.Add(new SearchTreeGroupEntry(new GUIContent(treeName)));
            
            HashSet<string> groups = new HashSet<string>();
            foreach (var nodeEntry in entries)
            {
                var nodeName = nodeEntry.Menu[nodeEntry.Menu.Length - 1];

                if (nodeEntry.Menu.Length > 1)
                {
                    var groupPath = "";
                    for (int i = 0; i < nodeEntry.Menu.Length - 1; i++)
                    {
                        var title = nodeEntry.Menu[i];
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
                    level = nodeEntry.Menu.Length,
                    userData = nodeEntry
                });
            }
            
            return tree;
        }
    }
}
#endif