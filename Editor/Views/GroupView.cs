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
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using CZToolKit.Core;

namespace CZToolKit.GraphProcessor.Editors
{
    public sealed class GroupView : Group, IBindableView<GroupPanel>
    {
        public Label titleLabel { get; private set; }
        public VisualElement titleContainer { get; private set; }
        public VisualElement centralContainer { get; private set; }
        public ColorField colorField { get; private set; }
        public BaseGraphView Owner { get; private set; }
        public GroupPanel Model { get; private set; }

        public GroupView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.GroupViewStyle);

            headerContainer.style.flexDirection = FlexDirection.Row;
            titleContainer = headerContainer.Q("titleContainer");
            titleLabel = headerContainer.Q("titleLabel") as Label;
            centralContainer = this.Q("centralContainer");

            colorField = new ColorField { name = "headerColorPicker" };
            headerContainer.Insert(0, colorField);

            //ToolbarToggle toggle = new ToolbarToggle() { name = "expanded", text = "expanded" };
            //toggle.style.flexGrow = 0;
            //toggle.RegisterValueChangedCallback(_ =>
            //{
            //    foreach (var item in containedElements)
            //    {
            //        item.style.display = _.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            //    }
            //    centralContainer.style.display = _.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            //});
            //headerContainer.Add(toggle);
        }

        public void SetUp(GroupPanel _group, CommandDispatcher _commandDispatcher, BaseGraphView _graphView)
        {
            Model = _group;
            Owner = _graphView;

            // 绑定
            BindingProperties();

            InitializeInnerNodes();

            //NodePort nodeport = new NodePort() { direction = PortDirection.Output, fieldName = "output", multiple = true, typeConstraint = PortTypeConstraint.None, typeQualifiedName = typeof(object).AssemblyQualifiedName };
            //NodePortView n = NodePortView.CreatePV(Orientation.Horizontal, Direction.Output, nodeport);
            //n.SetUp(nodeport, Owner.CommandDispatcher, Owner);
            //n.style.marginLeft = 100;
            //this.Add(n);
        }

        #region 数据监听回调
        void OnTitleChanged(string _title)
        {
            title = _title;
            Owner.SetDirty();
        }
        void OnPositionChanged(Rect _position)
        {
            base.SetPosition(_position);
            Owner.SetDirty();
        }
        void OnColorChanged(Color _color)
        {
            headerContainer.style.backgroundColor = _color;
            // 当明度大于0.5f,且透明度大于0.5f，文字颜色为黑色，否则为白色
            titleLabel.style.color = _color.GetLuminance() > 0.5f && _color.a > 0.5f ? Color.black : Color.white * 0.9f;
            colorField.SetValueWithoutNotify(_color);
            Owner.SetDirty();
        }

        void BindingProperties()
        {
            // 初始化
            title = Model.Title;
            base.SetPosition(Model.Position);
            headerContainer.style.backgroundColor = Model.Color;
            // 当明度大于0.5f,且透明度大于0.5f，文字颜色为黑色，否则为白色
            titleLabel.style.color = Model.Color.GetLuminance() > 0.5f && Model.Color.a > 0.5f ? Color.black : Color.white * 0.9f;
            colorField.SetValueWithoutNotify(Model.Color);


            colorField.RegisterValueChangedCallback(e =>
            {
                Model.Color = e.newValue;
            });

            Model.BindingProperty<string>(nameof(Model.Title), OnTitleChanged);
            Model.BindingProperty<Rect>(nameof(Model.Position), OnPositionChanged);
            Model.BindingProperty<Color>(nameof(Model.Color), OnColorChanged);
        }

        public void UnBindingProperties()
        {
            Model.UnBindingProperty<string>(nameof(Model.Title), OnTitleChanged);
            Model.UnBindingProperty<Rect>(nameof(Model.Position), OnPositionChanged);
            Model.UnBindingProperty<Color>(nameof(Model.Color), OnColorChanged);
        }
        #endregion

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            return base.AcceptsElement(element, ref reasonWhyNotAccepted);
        }

        void InitializeInnerNodes()
        {
            foreach (var nodeGUID in Model.InnerNodeGUIDs)
            {
                if (!Owner.NodeViews.ContainsKey(nodeGUID)) continue;

                BaseNodeView nodeView = Owner.NodeViews[nodeGUID];
                AddElement(nodeView);
            }

            //foreach (var stackGUID in Model.InnerStackGUIDs)
            //{
            //    if (!Owner.Model.Stacks.ContainsKey(stackGUID)) continue;

            //    var stackView = Owner.StackViews[stackGUID];
            //    AddElement(stackView);
            //}
        }

        protected override void OnGroupRenamed(string _oldName, string _newName)
        {
            if (string.IsNullOrEmpty(_newName) || _oldName.Equals(_newName)) return;
            Model.Title = _newName;
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
                if (nodeView != null && !Model.InnerNodeGUIDs.Contains(nodeView.Model.GUID))
                    Model.AddNode(nodeView.Model.GUID);
                //StackView stackNodeView = element as StackView;
                //if (stackNodeView != null && !Model.InnerStackGUIDs.Contains(stackNodeView.Model.GUID))
                //    Model.AddStack(stackNodeView.Model.GUID);
            }
            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                BaseNodeView nodeView = element as BaseNodeView;
                if (nodeView != null)
                    Model.RemoveNode(nodeView.Model.GUID);
                //StackView stackNodeView = element as StackView;
                //if (stackNodeView != null)
                //    Model.RemoveNode(stackNodeView.Model.GUID);
            }
            base.OnElementsRemoved(elements);
        }

        public override void SetPosition(Rect _newPos)
        {
            Model.Position = _newPos;
        }
    }
}