using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace CZToolKit.GraphProcessor.Editors
{
    public class GroupView : Group
    {
        const string groupStylePath = "GraphProcessor/Styles/GroupView";
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
        public BaseGroup groupData;
        Label titleLabel;
        ColorField colorField;

        public void Initialize(BaseGraphView _owner, BaseGroup _groupData)
        {
            styleSheets.Add(GroupViewStyle);

            owner = _owner;
            groupData = _groupData;
            title = _groupData.title;
            base.SetPosition(_groupData.position);

            titleLabel = headerContainer.Q("titleLabel") as Label;
            colorField = new ColorField { value = groupData.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(groupData.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
        }

        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in groupData.innerNodeGUIDs.ToList())
            {
                if (!owner.GraphData.Nodes.ContainsKey(nodeGUID)) continue;

                BaseNodeView nodeView = owner.NodeViews[nodeGUID];
                AddElement(nodeView);
            }

            foreach (var stackGUID in groupData.innerStackGUIDs.ToList())
            {
                if (!owner.GraphData.StackNodes.ContainsKey(stackGUID)) continue;

                var stackView = owner.StackNodeViews[stackGUID];
                AddElement(stackView);
            }
        }

        public void UpdateGroupColor(Color newColor)
        {
            groupData.color = newColor;
            headerContainer.style.backgroundColor = newColor;
            // 当明度大于0.5f,且透明度大于0.5f，文字颜色为黑色，否则为白色
            titleLabel.style.color = newColor.GetLuminance() > 0.5f && newColor.a > 0.5f ? Color.black : Color.white * 0.9f;
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            groupData.title = newName;
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
                    if (!groupData.innerNodeGUIDs.Contains(nodeView.NodeData.GUID))
                        groupData.innerNodeGUIDs.Add(nodeView.NodeData.GUID);
                    continue;
                }
                BaseStackNodeView stackNodeView = element as BaseStackNodeView;
                if (stackNodeView != null)
                {
                    if (!groupData.innerStackGUIDs.Contains(stackNodeView.stackNode.GUID))
                        groupData.innerStackGUIDs.Add(stackNodeView.stackNode.GUID);
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
                        groupData.innerNodeGUIDs.Remove(nodeView.NodeData.GUID);
                    else if (element is BaseStackNodeView stackNodeView)
                        groupData.innerNodeGUIDs.Remove(stackNodeView.stackNode.GUID);
                }
            }

            base.OnElementsRemoved(elements);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            groupData.position = newPos;
        }
    }
}