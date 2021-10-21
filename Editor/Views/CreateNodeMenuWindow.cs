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
        IEnumerable<Type> nodeTypes;
        List<SearchTreeEntry> tree;

        public void Initialize(BaseGraphView _graphView, IEnumerable<Type> nodeTypes)
        {
            graphView = _graphView;
            this.nodeTypes = nodeTypes;
            tree = CreateSearchTree();
        }

        private List<SearchTreeEntry> CreateSearchTree()
        {
            List<SearchTreeEntry> tempTree = new List<SearchTreeEntry>()
                {new SearchTreeGroupEntry(new GUIContent("Create Elements"))};

            foreach (Type type in nodeTypes)
            {
                if (Util_Attribute.TryGetTypeAttribute(type, out NodeMenuItemAttribute attribute))
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

            graphView.CommandDispacter.Do(new AddNodeCommand(graphView.Model, graphView.Model.NewNode(searchTreeEntry.userData as Type, graphMousePosition)));
            graphView.GraphWindow.Focus();
            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return tree;
        }
    }
}