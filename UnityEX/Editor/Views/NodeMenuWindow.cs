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
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    public class NodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        #region Define
        
        public interface INodeEntry
        {
            string Path { get; }

            string[] Menu { get; }

            Type NodeType { get; }

            BaseNodeProcessor CreateNode(BaseGraphProcessor graph, InternalVector2Int position);
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

            public Type NodeType
            {
                get { return nodeType; }
            }

            public NodeEntry(string path, string[] menu, Type nodeType)
            {
                this.path = path;
                this.menu = menu;
                this.nodeType = nodeType;
            }

            public BaseNodeProcessor CreateNode(BaseGraphProcessor graph, InternalVector2Int position)
            {
                if (graph == null)
                    return null;
                return graph.NewNode(nodeType, position);
            }
        }
        #endregion

        private sealed class EntryComparer : IComparer<INodeEntry>
        {
            private readonly Dictionary<string, int> usage;

            public EntryComparer(Dictionary<string, int> usage)
            {
                this.usage = usage;
            }

            public int Compare(INodeEntry x, INodeEntry y)
            {
                usage.TryGetValue(x.Path, out var ux);
                usage.TryGetValue(y.Path, out var uy);
                if (ux != uy)
                    return uy.CompareTo(ux);
                return string.Compare(x.Path, y.Path, StringComparison.Ordinal);
            }
        }
        
        private string treeName;
        private BaseGraphView graphView;
        public List<INodeEntry> entries = new List<INodeEntry>(256);

        private Func<INodeEntry, bool> filter;
        private Func<BaseNodeProcessor, bool> onNodeCreated;
        private static readonly Dictionary<string, int> s_UsageTick = new Dictionary<string, int>(256);
        private static int s_Tick;

        public void Initialize(string treeName, BaseGraphView graphView)
        {
            this.treeName = treeName;
            this.graphView = graphView;
            this.filter = null;
            this.onNodeCreated = null;
        }

        public void SetFilter(Func<INodeEntry, bool> filter)
        {
            this.filter = filter;
        }

        public void SetNodeCreatedHandler(Func<BaseNodeProcessor, bool> onNodeCreated)
        {
            this.onNodeCreated = onNodeCreated;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = graphView.Context.graphWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - graphView.Context.graphWindow.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            var nodeEntry = searchTreeEntry.userData as INodeEntry;
            if (nodeEntry == null)
                return false;

            var node = nodeEntry.CreateNode(graphView.ViewModel, graphMousePosition.ToInternalVector2Int());
            if (node == null)
                return false;

            var handled = onNodeCreated != null && onNodeCreated(node);
            if (!handled)
                graphView.Context.Do(new AddNodeCommand(graphView.ViewModel, node));

            s_UsageTick[nodeEntry.Path] = ++s_Tick;
            
            graphView.Context.graphWindow.Focus();
            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>(entries.Count + 1);
            tree.Add(new SearchTreeGroupEntry(new GUIContent(treeName)));

            var visibleEntries = new List<INodeEntry>(entries.Count);
            foreach (var nodeEntry in entries)
            {
                if (filter != null && !filter(nodeEntry))
                    continue;
                visibleEntries.Add(nodeEntry);
            }

            visibleEntries.Sort(new EntryComparer(s_UsageTick));
            
            var groups = new HashSet<string>();
            var pathBuilder = new System.Text.StringBuilder();
            foreach (var nodeEntry in visibleEntries)
            {
                var nodeName = nodeEntry.Menu[nodeEntry.Menu.Length - 1];

                if (nodeEntry.Menu.Length > 1)
                {
                    pathBuilder.Clear();
                    for (int i = 0; i < nodeEntry.Menu.Length - 1; i++)
                    {
                        var title = nodeEntry.Menu[i];
                        if (pathBuilder.Length > 0)
                            pathBuilder.Append('/');
                        pathBuilder.Append(title);
                        var groupPath = pathBuilder.ToString();
                        if (groups.Add(groupPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title))
                            {
                                level = i + 1
                            });
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
