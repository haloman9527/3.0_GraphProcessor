using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using CZToolKit.Core;

namespace CZToolKit.GraphProcessor.Editors
{
    public class CreateNodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        BaseGraphView graphView;
        NodePort port;
        IEnumerable<Type> nodeTypes;
        List<SearchTreeEntry> tree;
        Action<BaseNode> onNodeCreated;

        public void Initialize(BaseGraphView _graphView, IEnumerable<Type> _nodeTypes)
        {
            graphView = _graphView;
            nodeTypes = _nodeTypes;
            tree = CreateSearchTree();
            onNodeCreated = null;
        }

        //public void Initialize(BaseGraphView _graphView, IEnumerable<Type> _nodeTypes, NodePort _port, Action<BaseNode> _onNodeCreated)
        //{
        //    graphView = _graphView;
        //    port = _port;
        //    nodeTypes = _nodeTypes;
        //    tree = foreach (var item in GraphProcessorCache.PortCache)
        //    {
        //        foreach (var cachedPort in item.Value)
        //        {
        //            if (NodePort.IsCompatible(port, cachedPort))
        //                yield return item.Key;
        //        }
        //    }
        //    onNodeCreated = _onNodeCreated;
        //}

        private List<SearchTreeEntry> CreateSearchTree()
        {
            List<SearchTreeEntry> tempTree = new List<SearchTreeEntry>()
                {new SearchTreeGroupEntry(new GUIContent("Create Elements"))};

            foreach (Type type in nodeTypes)
            {
                if (Utility_Attribute.TryGetTypeAttribute(type, out NodeMenuItemAttribute attribute))
                {
                    if (attribute.showInList)
                    {
                        if (attribute.titles.Length > 1)
                        {
                            SearchTreeGroupEntry groupTemp = null;
                            for (int i = 1; i < attribute.titles.Length; i++)
                            {
                                SearchTreeGroupEntry group = tempTree.Find(item =>
                                        (item.content.text == attribute.titles[i - 1] && item.level == i)) as
                                    SearchTreeGroupEntry;
                                if (group == null)
                                {
                                    group = new SearchTreeGroupEntry(new GUIContent(attribute.titles[i - 1]), i);
                                    int index = groupTemp == null ? 0 : tempTree.IndexOf(groupTemp);
                                    tempTree.Insert(index + 1, group);
                                }

                                groupTemp = group;
                            }
                            tempTree.Insert(tempTree.IndexOf(groupTemp) + 1,
                                new SearchTreeEntry(new GUIContent(attribute.titles.Last()))
                                { userData = type, level = attribute.titles.Length });
                        }
                        else
                        {
                            tempTree.Add(new SearchTreeEntry(new GUIContent(attribute.titles.Last()))
                            { userData = type, level = 1 });
                        }
                    }
                }
                else
                {
                    GUIContent content = new GUIContent(GraphProcessorEditorUtility.GetNodeDisplayName(type));
                    tempTree.Add(new SearchTreeEntry(content) { userData = type, level = 1 });
                }
            }
            return tempTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = graphView.GraphWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - graphView.GraphWindow.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            graphView.Model.AddNode(BaseNode.CreateNew(searchTreeEntry.userData as Type, graphMousePosition));
            graphView.GraphWindow.Focus();

            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return tree;
        }


        //void CreateEdgeNodeMenu(List<SearchTreeEntry> tree)
        //{
        //    var entries = NodeProvider.GetEdgeCreationNodeMenuEntry((edgeFilter.input ?? edgeFilter.output) as PortView, graphView.graphData);

        //    var titlePaths = new HashSet<string>();

        //    var nodePaths = NodeProvider.GetNodeMenuEntries(graphView.graphData);

        //    tree.Add(new SearchTreeEntry(new GUIContent($"Relay", icon))
        //    {
        //        level = 1,
        //        userData = new NodeProvider.PortDescription
        //        {
        //            nodeType = typeof(RelayNode),
        //            portType = typeof(System.Object),
        //            isInput = inputPortView != null,
        //            portFieldName = inputPortView != null ? nameof(RelayNode.output) : nameof(RelayNode.input),
        //            portIdentifier = "0",
        //        }
        //    });

        //    var sortedMenuItems = entries.Select(port => (port, nodePaths.FirstOrDefault(kp => kp.type == port.nodeType).path)).OrderBy(e => e.path);

        //    // Sort menu by alphabetical order and submenus
        //    foreach (var nodeMenuItem in sortedMenuItems)
        //    {
        //        var nodePath = nodePaths.FirstOrDefault(kp => kp.type == nodeMenuItem.port.nodeType).path;

        //        // Ignore the node if it's not in the create menu
        //        if (String.IsNullOrEmpty(nodePath))
        //            continue;

        //        var nodeName = nodePath;
        //        int level = 0;
        //        string parts = nodePath.Split('/');

        //        if (parts.Length > 1)
        //        {
        //            level++;
        //            nodeName = parts[parts.Length - 1];
        //            var fullTitleAsPath = "";

        //            for (var i = 0; i < parts.Length - 1; i++)
        //            {
        //                var title = parts[i];
        //                fullTitleAsPath += title;
        //                level = i + 1;

        //                // Add section title if the node is in subcategory
        //                if (!titlePaths.Contains(fullTitleAsPath))
        //                {
        //                    tree.Add(new SearchTreeGroupEntry(new GUIContent(title))
        //                    {
        //                        level = level
        //                    });
        //                    titlePaths.Add(fullTitleAsPath);
        //                }
        //            }
        //        }

        //        tree.Add(new SearchTreeEntry(new GUIContent($"{nodeName}", icon))
        //        {
        //            level = level + 1,
        //            userData = nodeMenuItem.port
        //        });
        //    }
        //}
    }
}