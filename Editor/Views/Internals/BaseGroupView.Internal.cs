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
            this.title = Model.GroupName;
            base.SetPosition(new Rect(Model.Position, GetPosition().size));
            base.AddElements(Model.Nodes.Select(nodeGUID => Owner.NodeViews[nodeGUID]));
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            Initialized = true;
        }

        public void BindingProperties()
        {
            Model[nameof(Model.groupName)].RegisterValueChangedEvent<string>(OnTitleChanged);
            Model[nameof(Model.position)].RegisterValueChangedEvent<Vector2>(OnPositionChanged);
            Model.onElementsAdded += OnNodesAdded;
            Model.onElementsRemoved += OnNodesRemoved;
        }

        public void UnBindingProperties()
        {
            Model[nameof(Model.groupName)].UnregisterValueChangedEvent<string>(OnTitleChanged);
            Model[nameof(Model.position)].UnregisterValueChangedEvent<Vector2>(OnPositionChanged);
            Model.onElementsAdded -= OnNodesAdded;
            Model.onElementsRemoved -= OnNodesRemoved;
        }

        private void OnTitleChanged(string title)
        {
            if (string.IsNullOrEmpty(title))
                return;
            this.title = Model.GroupName;
            Owner.SetDirty();
        }

        private void OnPositionChanged(Vector2 newPos)
        {
            base.SetPosition(new Rect(newPos, GetPosition().size));
        }

        private void OnNodesAdded(IEnumerable<INode> nodes)
        {
            base.AddElements(nodes.Select(node => Owner.NodeViews[node.GUID]));
        }

        private void OnNodesRemoved(IEnumerable<INode> nodes)
        {
            base.RemoveElements(nodes.Select(node => Owner.NodeViews[node.GUID]));
        }

        protected void BuildContextualMenu(ContextualMenuPopulateEvent obj)
        {

        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                return;
            Owner.CommandDispacter.Do(new RenameGroupCommand(Model, newName));
        }

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (!base.AcceptsElement(element,ref reasonWhyNotAccepted))
                return false;
            if (element is BaseNodeView)
                return true;
            if (element is BaseConnectionView)
                return true;
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
            Owner.SetDirty();
        }
    }
}
#endif