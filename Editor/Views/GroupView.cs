using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace GraphProcessor.Editors
{
    public class GroupView : UnityEditor.Experimental.GraphView.Group
    {
        const string groupStylePath = "GraphProcessorStyles/GroupView";
        static StyleSheet groupViewStyle;
        public static StyleSheet GroupViewStyle
        {
            get
            {
                if (groupViewStyle == null)
                    groupViewStyle = Resources.Load<StyleSheet>(groupStylePath);
                return groupViewStyle;
            }
        }

        public BaseGraphView owner;
        public BaseGroup group;
        ColorField colorField;

        public void Initialize(BaseGraphView graphView, BaseGroup group)
        {
            styleSheets.Add(GroupViewStyle);

            this.group = group;
            owner = graphView;

            title = group.title;
            base.SetPosition(group.position);

            colorField = new ColorField { value = this.group.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(this.group.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
        }

        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in group.innerNodeGUIDs.ToList())
            {
                if (!owner.GraphData.Nodes.ContainsKey(nodeGUID)) continue;

                BaseNodeView nodeView = owner.NodeViews[nodeGUID];
                AddElement(nodeView);
            }

            foreach (var stackGUID in group.innerStackGUIDs.ToList())
            {
                if (!owner.GraphData.StackNodes.ContainsKey(stackGUID)) continue;

                var stackView = owner.StackNodeViews[stackGUID];
                AddElement(stackView);
            }
        }

        public void UpdateGroupColor(Color newColor)
        {
            group.color = newColor;
            style.backgroundColor = newColor;
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            group.title = newName;
            base.OnGroupRenamed(oldName, newName);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();

            foreach (GraphElement element in containedElements)
            {
                element.BringToFront();
            }
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                BaseNodeView nodeView = element as BaseNodeView;
                if (nodeView != null)
                {
                    if (!group.innerNodeGUIDs.Contains(nodeView.NodeData.GUID))
                        group.innerNodeGUIDs.Add(nodeView.NodeData.GUID);
                    continue;
                }
                BaseStackNodeView stackNodeView = element as BaseStackNodeView;
                if (stackNodeView != null)
                {
                    if (!group.innerStackGUIDs.Contains(stackNodeView.stackNode.GUID))
                        group.innerStackGUIDs.Add(stackNodeView.stackNode.GUID);
                }
            }
            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (parent != null)
            {
                foreach (VisualElement element in elements)
                {
                    if (element is BaseNodeView nodeView)
                        group.innerNodeGUIDs.Remove(nodeView.NodeData.GUID);
                    else if (element is BaseStackNodeView stackNodeView)
                        group.innerNodeGUIDs.Remove(stackNodeView.stackNode.GUID);
                }
            }

            base.OnElementsRemoved(elements);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            group.position = newPos;
        }
    }
}