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


        public BaseGraphView Owner { get; private set; }
        public BaseGroup GroupData { get; private set; }
        public Label titleLabel { get; private set; }
        public ColorField colorField { get; private set; }

        public void Initialize(BaseGraphView _owner, BaseGroup _groupData)
        {
            styleSheets.Add(GroupViewStyle);

            Owner = _owner;
            GroupData = _groupData;
            title = _groupData.title;
            base.SetPosition(_groupData.position);

            titleLabel = headerContainer.Q("titleLabel") as Label;
            colorField = new ColorField { value = GroupData.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(GroupData.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
        }

        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in GroupData.innerNodeGUIDs.ToList())
            {
                if (!Owner.GraphData.Nodes.ContainsKey(nodeGUID)) continue;

                BaseNodeView nodeView = Owner.NodeViews[nodeGUID];
                AddElement(nodeView);
            }

            foreach (var stackGUID in GroupData.innerStackGUIDs.ToList())
            {
                if (!Owner.GraphData.StackNodes.ContainsKey(stackGUID)) continue;

                var stackView = Owner.StackNodeViews[stackGUID];
                AddElement(stackView);
            }
        }

        public void UpdateGroupColor(Color newColor)
        {
            GroupData.color = newColor;
            headerContainer.style.backgroundColor = newColor;
            // 当明度大于0.5f,且透明度大于0.5f，文字颜色为黑色，否则为白色
            titleLabel.style.color = newColor.GetLuminance() > 0.5f && newColor.a > 0.5f ? Color.black : Color.white * 0.9f;
        }

        protected override void OnGroupRenamed(string _oldName, string _newName)
        {
            if (string.IsNullOrEmpty(_newName) || _oldName.Equals(_newName)) return;
            base.OnGroupRenamed(_oldName, _newName);
            GroupData.title = _newName;
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
                    if (!GroupData.innerNodeGUIDs.Contains(nodeView.NodeData.GUID))
                        GroupData.innerNodeGUIDs.Add(nodeView.NodeData.GUID);
                    continue;
                }
                BaseStackNodeView stackNodeView = element as BaseStackNodeView;
                if (stackNodeView != null)
                {
                    if (!GroupData.innerStackGUIDs.Contains(stackNodeView.stackNode.GUID))
                        GroupData.innerStackGUIDs.Add(stackNodeView.stackNode.GUID);
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
                        GroupData.innerNodeGUIDs.Remove(nodeView.NodeData.GUID);
                    else if (element is BaseStackNodeView stackNodeView)
                        GroupData.innerNodeGUIDs.Remove(stackNodeView.stackNode.GUID);
                }
            }

            base.OnElementsRemoved(elements);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (Owner.Initialized)
                GroupData.position = newPos;
        }
    }
}