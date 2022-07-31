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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using GroupView = UnityEditor.Experimental.GraphView.Group;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseGroupView : GroupView, IBindableView<BaseGroupVM>
    {
        bool WithoutNotify { get; set; }
        public TextField TitleField { get; private set; }
        public ColorField BackgroudColorField { get; private set; }
        public Label TitleLabel { get; private set; }
        public BaseGroupVM ViewModel { get; protected set; }
        public BaseGraphView Owner { get; private set; }


        public BaseGroupView()
        {
            this.styleSheets.Add(GraphProcessorStyles.BaseGroupStyle);

            TitleLabel = headerContainer.Q<Label>();
            TitleField = headerContainer.Q<TextField>();

            BackgroudColorField = new ColorField();
            BackgroudColorField.style.position = Position.Absolute;
            BackgroudColorField.style.width = 50;
            BackgroudColorField.style.right = 10;
            BackgroudColorField.style.top = 5;
            headerContainer.Add(BackgroudColorField);

            TitleField.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            TitleField.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
        }

        public void SetUp(BaseGroupVM group, BaseGraphView graphView)
        {
            this.ViewModel = group;
            this.Owner = graphView;
            this.title = ViewModel.GroupName;
            this.style.backgroundColor = ViewModel.BackgroundColor.ToColor();
            this.BackgroudColorField.SetValueWithoutNotify(ViewModel.BackgroundColor.ToColor());
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            WithoutNotify = true;
            base.AddElements(ViewModel.Nodes.Select(nodeGUID => Owner.NodeViews[nodeGUID]).ToArray());
            WithoutNotify = false;
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            BackgroudColorField.RegisterValueChangedCallback(OnGroupColorChanged);
        }

        public void BindingProperties()
        {
            ViewModel[nameof(BaseGroup.groupName)].RegisterValueChangedEvent<string>(OnTitleChanged);
            ViewModel[nameof(BaseGroup.position)].RegisterValueChangedEvent<InternalVector2>(OnPositionChanged);
            ViewModel[nameof(BaseGroup.backgroundColor)].RegisterValueChangedEvent<InternalColor>(OnBackgroundColorChanged);
            ViewModel.onNodesAdded += OnNodesAdded;
            ViewModel.onNodesRemoved += OnNodesRemoved;
        }

        public void UnBindingProperties()
        {
            ViewModel[nameof(BaseGroup.groupName)].UnregisterValueChangedEvent<string>(OnTitleChanged);
            ViewModel[nameof(BaseGroup.position)].UnregisterValueChangedEvent<InternalVector2>(OnPositionChanged);
            ViewModel[nameof(BaseGroup.backgroundColor)].UnregisterValueChangedEvent<InternalColor>(OnBackgroundColorChanged);
            ViewModel.onNodesAdded -= OnNodesAdded;
            ViewModel.onNodesRemoved -= OnNodesRemoved;
        }

        #region Callbacks
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

        private void OnBackgroundColorChanged(InternalColor newColor)
        {
            this.BackgroudColorField.SetValueWithoutNotify(newColor.ToColor());
            this.style.backgroundColor = newColor.ToColor();
            Owner.SetDirty();
        }

        private void OnNodesAdded(IEnumerable<BaseNodeVM> nodes)
        {
            WithoutNotify = true;
            base.AddElements(nodes.Select(node => Owner.NodeViews[node.GUID]));
            WithoutNotify = false;
        }

        private void OnNodesRemoved(IEnumerable<BaseNodeVM> nodes)
        {
            WithoutNotify = true;
            base.RemoveElements(nodes.Select(node => Owner.NodeViews[node.GUID]));
            WithoutNotify = false;
        }
        #endregion

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                return;
            Owner.CommandDispacter.Do(new RenameGroupCommand(ViewModel, newName));
        }

        private void OnGroupColorChanged(ChangeEvent<Color> evt)
        {
            ViewModel.BackgroundColor = evt.newValue.ToInternalColor();
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
            if (WithoutNotify)
                return;
            var nodes = elements.Where(element => element is BaseNodeView).Select(element => (element as BaseNodeView).ViewModel);
            ViewModel.AddNodesWithoutNotify(nodes);
            Owner.SetDirty();
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);
            if (WithoutNotify)
                return;
            var nodes = elements.Where(element => element is BaseNodeView).Select(element => (element as BaseNodeView).ViewModel).ToArray();
            ViewModel.RemoveNodesWithoutNotify(nodes);
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