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
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using GroupView = UnityEditor.Experimental.GraphView.Group;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGroupView : GroupView, IBindableView<Group>
    {
        public bool Initialized { get; private set; }
        public TextField TitleField { get; private set; }
        public Label TitleLabel { get; private set; }
        public Group Model { get; protected set; }
        public BaseGraphView Owner { get; private set; }

        public override string title
        {
            get => Model.GroupName;
            set => Model.GroupName = value;
        }

        public BaseGroupView()
        {
            TitleLabel = headerContainer.Q<Label>();
            TitleField = headerContainer.Q<TextField>();

            TitleField.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            TitleField.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
        }

        public void SetUp(Group group, BaseGraphView graphView)
        {
            this.Model = group;
            this.Owner = graphView;

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            this.title = Model.GroupName;
            this.TitleLabel.text = Model.GroupName;
            base.SetPosition(new Rect(Model.Position, GetPosition().size));
            base.AddElements(Model.Nodes.Select(v => Owner.NodeViews[v]));

            Model[nameof(group.GroupName)].RegisterValueChangedEvent<string>(OnTitleChanged);
            Model[nameof(group.Position)].RegisterValueChangedEvent<Vector2>(OnPositionChanged);
            Model.onElementsAdded += OnNodesAdded;
            Model.onElementsRemoved += OnNodesRemoved;
            Initialized = true;
        }

        private void OnPositionChanged(Vector2 newPos)
        {
            base.SetPosition(new Rect(newPos, GetPosition().size));
        }

        public void UnBindingProperties()
        {
            Model[nameof(Model.GroupName)].UnregisterValueChangedEvent<string>(OnTitleChanged);
            Model.onElementsAdded -= OnNodesAdded;
            Model.onElementsRemoved -= OnNodesRemoved;
        }

        private void OnTitleChanged(string title)
        {
            if (string.IsNullOrEmpty(title))
                return;
            this.title = Model.GroupName;
            this.TitleLabel.text = Model.GroupName;
            Owner.SetDirty();
        }

        private void OnNodesAdded(IEnumerable<BaseNode> nodes)
        {
            base.AddElements(nodes.Select(node => Owner.NodeViews[node.GUID]));
        }

        private void OnNodesRemoved(IEnumerable<BaseNode> nodes)
        {
            base.RemoveElements(nodes.Select(node => Owner.NodeViews[node.GUID]));
        }

        protected virtual void BuildContextualMenu(ContextualMenuPopulateEvent obj)
        {

        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Model.GroupName = newName;
        }

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (element is BaseNodeView)
                return true;
            reasonWhyNotAccepted = "Nested group is not supported yet.";
            return false;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);
            if (!Initialized)
                return;
            var nodes = elements.Where(element => element is BaseNodeView).Select(element => (element as BaseNodeView).Model);
            Model.AddNodesWithoutNotify(nodes);
            Owner.SetDirty();
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);
            if (!Initialized)
                return;
            var nodes = elements.Where(element => element is BaseNodeView).Select(element => (element as BaseNodeView).Model);
            Model.RemoveNodesWithoutNotify(nodes);
            if (Model.Nodes.Count <= 0)
                Model.Owner.RemoveGroup(Model);
            Owner.SetDirty();
        }
    }
}
#endif