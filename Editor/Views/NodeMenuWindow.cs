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
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class NodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        BaseGraphView graphView;
        IEnumerable<Type> nodeTypes;
        List<SearchTreeEntry> tree;

        public void Initialize(BaseGraphView graphView, IEnumerable<Type> nodeTypes)
        {
            this.graphView = graphView;
            this.nodeTypes = nodeTypes;
            tree = CreateSearchTree();
        }

        private List<SearchTreeEntry> CreateSearchTree()
        {
            List<SearchTreeEntry> tempTree = new List<SearchTreeEntry>()
                {new SearchTreeGroupEntry(new GUIContent("Create Elements"))};

            foreach (Type type in nodeTypes)
            {
                var menuAttribute = GraphProcessorEditorUtil.GetNodeMenu(type);
                if (menuAttribute != null)
                {
                    if (menuAttribute.showInList)
                    {
                        if (menuAttribute.titles.Length > 1)
                        {
                            SearchTreeGroupEntry groupTemp = null;
                            for (int i = 1; i < menuAttribute.titles.Length; i++)
                            {
                                SearchTreeGroupEntry group = tempTree.Find(item =>
                                        (item.content.text == menuAttribute.titles[i - 1] && item.level == i)) as
                                    SearchTreeGroupEntry;
                                if (group == null)
                                {
                                    group = new SearchTreeGroupEntry(new GUIContent(menuAttribute.titles[i - 1]), i);
                                    int index = groupTemp == null ? 0 : tempTree.IndexOf(groupTemp);
                                    tempTree.Insert(index + 1, group);
                                }

                                groupTemp = group;
                            }
                            tempTree.Insert(tempTree.IndexOf(groupTemp) + 1,
                                new SearchTreeEntry(new GUIContent(menuAttribute.titles.Last()))
                                { userData = type, level = menuAttribute.titles.Length });
                        }
                        else
                        {
                            tempTree.Add(new SearchTreeEntry(new GUIContent(menuAttribute.titles.Last()))
                            { userData = type, level = 1 });
                        }
                    }
                }
                else
                {
                    GUIContent content = new GUIContent(type.Name);
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

            graphView.CommandDispacter.Do(new AddNodeCommand(graphView.ViewModel, searchTreeEntry.userData as Type, graphMousePosition.ToInternalVector2()));
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