using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace CZToolKit.GraphProcessor.Editors
{
    public class GroupView : Group, IGroupView
    {
        public BaseGraphView Owner { get; private set; }
        public BaseGroup GroupData { get { return userData as BaseGroup; } }
        public Label titleLabel { get; private set; }
        public ColorField colorField { get; private set; }


        public GroupView()
        {
            styleSheets.Add(GraphProcessorStyles.GroupViewStyle);
        }

        public void SetUp(IGraphElement _graphElement, CommandDispatcher _commandDispatcher, IGraphView _graphView)
        {
            userData = _graphElement;
            Owner = _graphView as BaseGraphView;

            BaseGroup groupData = _graphElement as BaseGroup;
            title = groupData.title;
            base.SetPosition(groupData.position);

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
                if (!Owner.Graph.NodesGUIDMapping.ContainsKey(nodeGUID)) continue;

                BaseNodeView nodeView = Owner.NodeViews[nodeGUID];
                AddElement(nodeView);
            }

            foreach (var stackGUID in GroupData.innerStackGUIDs.ToList())
            {
                if (!Owner.Graph.StackNodesGUIDMapping.ContainsKey(stackGUID)) continue;

                var stackView = Owner.StackNodeViews[stackGUID];
                AddElement(stackView);
            }
        }

        public void UpdateGroupColor(Color _newColor)
        {
            headerContainer.style.backgroundColor = _newColor;
            // 当明度大于0.5f,且透明度大于0.5f，文字颜色为黑色，否则为白色
            titleLabel.style.color = _newColor.GetLuminance() > 0.5f && _newColor.a > 0.5f ? Color.black : Color.white * 0.9f;
            GroupData.color = _newColor;
            Owner.SetDirty();
        }

        protected override void OnGroupRenamed(string _oldName, string _newName)
        {
            if (string.IsNullOrEmpty(_newName) || _oldName.Equals(_newName)) return;
            base.OnGroupRenamed(_oldName, _newName);
            GroupData.title = _newName;
            Owner.SetDirty();
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

        public override void SetPosition(Rect _newPos)
        {
            base.SetPosition(_newPos);
            if (Owner.Initialized)
                GroupData.position = _newPos;
        }
    }
}