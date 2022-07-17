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
    public partial class BaseGroupView : GroupView, IBindableView<BaseGroupVM>
    {
        public bool Initialized { get; private set; }
        public TextField TitleField { get; private set; }
        public Label TitleLabel { get; private set; }
        public BaseGroupVM ViewModel { get; protected set; }
        public BaseGraphView Owner { get; private set; }

        public BaseGroupView()
        {
            TitleLabel = headerContainer.Q<Label>();
            TitleField = headerContainer.Q<TextField>();

            TitleField.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            TitleField.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
        }

        public void SetUp(BaseGroupVM group, BaseGraphView graphView)
        {
            this.ViewModel = group;
            this.Owner = graphView;
            this.title = ViewModel.GroupName;
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            base.AddElements(ViewModel.Nodes.Select(nodeGUID => Owner.NodeViews[nodeGUID]));
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            Initialized = true;
        }

        public void BindingProperties()
        {
            ViewModel[nameof(BaseGroup.groupName)].RegisterValueChangedEvent<string>(OnTitleChanged);
            ViewModel[nameof(BaseGroup.position)].RegisterValueChangedEvent<InternalVector2>(OnPositionChanged);
            ViewModel.onNodesAdded += OnNodesAdded;
            ViewModel.onNodesRemoved += OnNodesRemoved;
        }

        public void UnBindingProperties()
        {
            ViewModel[nameof(BaseGroup.groupName)].UnregisterValueChangedEvent<string>(OnTitleChanged);
            ViewModel[nameof(BaseGroup.position)].UnregisterValueChangedEvent<InternalVector2>(OnPositionChanged);
            ViewModel.onNodesAdded -= OnNodesAdded;
            ViewModel.onNodesRemoved -= OnNodesRemoved;
        }

        private void OnTitleChanged(string title)
        {
            if (string.IsNullOrEmpty(title))
                return;
            this.title = ViewModel.GroupName;
            Owner.SetDirty();
        }

        private void OnPositionChanged(InternalVector2 newPos)
        {
            base.SetPosition(new Rect(newPos.ToVector2(), GetPosition().size));
        }

        private void OnNodesAdded(IEnumerable<BaseNodeVM> nodes)
        {
            base.AddElements(nodes.Select(node => Owner.NodeViews[node.GUID]));
        }

        private void OnNodesRemoved(IEnumerable<BaseNodeVM> nodes)
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
            Owner.CommandDispacter.Do(new RenameGroupCommand(ViewModel, newName));
        }

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (!base.AcceptsElement(element, ref reasonWhyNotAccepted))
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
            var nodes = elements.Where(element => element is BaseNodeView).Select(element => (element as BaseNodeView).ViewModel);
            ViewModel.AddNodesWithoutNotify(nodes);
            Owner.SetDirty();
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);
            if (!Initialized)
                return;
            Owner.SetDirty();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            this.BringToFront();
        }
    }
}
#endif