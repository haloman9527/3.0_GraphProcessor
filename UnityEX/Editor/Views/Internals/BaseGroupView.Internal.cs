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
    public partial class BaseGroupView : GroupView, IGraphElementView<BaseGroupVM>
    {
        bool WithoutNotify { get; set; }
        public TextField TitleField { get; private set; }
        public ColorField BackgroudColorField { get; private set; }
        public Label TitleLabel { get; private set; }
        public BaseGroupVM ViewModel { get; protected set; }
        public BaseGraphView Owner { get; private set; }
        


        public BaseGroupView()
        {
            this.styleSheets.Add(GraphProcessorStyles.BaseGroupViewStyle);

            TitleLabel = headerContainer.Q<Label>();
            TitleField = headerContainer.Q<TextField>();

            BackgroudColorField = new ColorField();
            BackgroudColorField.name = "backgroundColorField";
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
            this.style.unityBackgroundImageTintColor = ViewModel.BackgroundColor.ToColor();
            this.BackgroudColorField.SetValueWithoutNotify(ViewModel.BackgroundColor.ToColor());
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            WithoutNotify = true;
            base.AddElements(ViewModel.Nodes.Where(nodeID=>Owner.NodeViews.ContainsKey(nodeID)).Select(nodeID => Owner.NodeViews[nodeID]).ToArray());
            WithoutNotify = false;
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            BackgroudColorField.RegisterValueChangedCallback(OnGroupColorChanged);
        }

        public void OnInitialize()
        {
            ViewModel[nameof(BaseGroup.groupName)].AsBindableProperty<string>().RegisterValueChangedEvent(OnTitleChanged);
            ViewModel[nameof(BaseGroup.position)].AsBindableProperty<InternalVector2Int>().RegisterValueChangedEvent(OnPositionChanged);
            ViewModel[nameof(BaseGroup.backgroundColor)].AsBindableProperty<InternalColor>().RegisterValueChangedEvent(OnBackgroundColorChanged);
            ViewModel.onNodeAdded += OnNodesAdded;
            ViewModel.onNodeRemoved += OnNodesRemoved;
        }

        public void OnDestroy()
        {
            ViewModel[nameof(BaseGroup.groupName)].AsBindableProperty<string>().UnregisterValueChangedEvent(OnTitleChanged);
            ViewModel[nameof(BaseGroup.position)].AsBindableProperty<InternalVector2Int>().UnregisterValueChangedEvent(OnPositionChanged);
            ViewModel[nameof(BaseGroup.backgroundColor)].AsBindableProperty<InternalColor>().UnregisterValueChangedEvent(OnBackgroundColorChanged);
            ViewModel.onNodeAdded -= OnNodesAdded;
            ViewModel.onNodeRemoved -= OnNodesRemoved;
        }

        #region Callbacks
        private void OnTitleChanged(string newTitle)
        {
            if (string.IsNullOrEmpty(newTitle))
                return;
            this.title = ViewModel.GroupName;
            Owner.SetDirty();
        }

        private void OnPositionChanged(InternalVector2Int newPosition)
        {
            base.SetPosition(new Rect(newPosition.ToVector2(), GetPosition().size));
        }

        private void OnBackgroundColorChanged(InternalColor newColor)
        {
            this.BackgroudColorField.SetValueWithoutNotify(newColor.ToColor());
            this.style.backgroundColor = newColor.ToColor();
            this.style.unityBackgroundImageTintColor = newColor.ToColor();
            Owner.SetDirty();
        }

        private void OnNodesAdded(BaseNodeVM node)
        {
            WithoutNotify = false;
            base.AddElements(new BaseNodeView[] { Owner.NodeViews[node.ID] });
            WithoutNotify = true;
        }

        private void OnNodesRemoved(BaseNodeVM node)
        {
            WithoutNotify = false;
            base.RemoveElements(new BaseNodeView[] { Owner.NodeViews[node.ID] });
            WithoutNotify = true;
        }
        #endregion

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Owner.CommandDispatcher.Do(new RenameGroupCommand(ViewModel, newName));
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
            if (WithoutNotify)
            {
                return;
            }
            foreach (var element in elements)
            {
                if (!(element is BaseNodeView nodeView))
                    continue;
                Owner.ViewModel.Groups.AddNodeToGroup(ViewModel, nodeView.ViewModel);
            }
            Owner.SetDirty();
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (WithoutNotify)
            {
                return;
            }
            foreach (var element in elements)
            {
                if (!(element is BaseNodeView nodeView))
                    continue;
                Owner.ViewModel.Groups.RemoveNodeFromGroup(nodeView.ViewModel);
            }
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