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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GroupView = UnityEditor.Experimental.GraphView.Group;

namespace Jiange.GraphProcessor.Editors
{
    public partial class BaseGroupView : GroupView, IGraphElementView<BaseGroupProcessor>
    {
        bool WithoutNotify { get; set; }
        public TextField TitleField { get; private set; }
        public ColorField BackgroudColorField { get; private set; }
        public Label TitleLabel { get; private set; }
        public BaseGroupProcessor ViewModel { get; protected set; }
        public IGraphElementProcessor V => ViewModel;
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

        public void SetUp(BaseGroupProcessor group, BaseGraphView graphView)
        {
            this.ViewModel = group;
            this.Owner = graphView;
            this.title = ViewModel.GroupName;
            this.style.backgroundColor = ViewModel.BackgroundColor.ToColor();
            this.style.unityBackgroundImageTintColor = ViewModel.BackgroundColor.ToColor();
            this.BackgroudColorField.SetValueWithoutNotify(ViewModel.BackgroundColor.ToColor());
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            WithoutNotify = true;
            base.AddElements(ViewModel.Nodes.Where(nodeID => Owner.NodeViews.ContainsKey(nodeID)).Select(nodeID => Owner.NodeViews[nodeID]).ToArray());
            WithoutNotify = false;
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            BackgroudColorField.RegisterValueChangedCallback(OnGroupColorChanged);
        }

        public void OnCreate()
        {
            ViewModel.PropertyChanged += OnViewModelChanged;
            ViewModel.onNodeAdded += OnNodesAdded;
            ViewModel.onNodeRemoved += OnNodesRemoved;
        }

        public void OnDestroy()
        {
            ViewModel.PropertyChanged -= OnViewModelChanged;
            ViewModel.onNodeAdded -= OnNodesAdded;
            ViewModel.onNodeRemoved -= OnNodesRemoved;
        }

        #region Callbacks

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            var group = sender as BaseGroupProcessor;
            switch (e.PropertyName)
            {
                case nameof(BaseGroup.position):
                {
                    base.SetPosition(new Rect(group.Position.ToVector2(), GetPosition().size));
                    break;
                }
                case nameof(BaseGroup.groupName):
                {
                    if (string.IsNullOrEmpty(group.GroupName))
                        return;
                    this.title = ViewModel.GroupName;
                    Owner.SetDirty();
                    break;
                }
                case nameof(BaseGroup.backgroundColor):
                {
                    this.BackgroudColorField.SetValueWithoutNotify(group.BackgroundColor.ToColor());
                    this.style.backgroundColor = group.BackgroundColor.ToColor();
                    this.style.unityBackgroundImageTintColor = group.BackgroundColor.ToColor();
                    Owner.SetDirty();
                    break;
                }
            }
        }

        private void OnNodesAdded(BaseNodeProcessor node)
        {
            if (WithoutNotify)
                return;
            base.AddElements(new BaseNodeView[] { Owner.NodeViews[node.ID] });
        }

        private void OnNodesRemoved(BaseNodeProcessor node)
        {
            if (WithoutNotify)
                return;
            base.RemoveElementsWithoutNotification(new BaseNodeView[] { Owner.NodeViews[node.ID] });
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
            switch (element)
            {
                case BaseNodeView:
                    return true;
            }

            return false;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (WithoutNotify)
                return;

            var temp = WithoutNotify;
            WithoutNotify = true;
            foreach (var element in elements)
            {
                switch (element)
                {
                    case BaseNodeView nodeView:
                    {
                        try
                        {
                            Owner.ViewModel.Groups.AddNodeToGroup(ViewModel, nodeView.ViewModel);
                        }
                        catch
                        {
                            // ignored
                        }

                        break;
                    }
                }
            }

            WithoutNotify = temp;

            Owner.SetDirty();
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (WithoutNotify)
                return;

            foreach (var element in elements)
            {
                var nodeView = (BaseNodeView)element;
                try
                {
                    Owner.ViewModel.Groups.RemoveNodeFromGroup(nodeView.ViewModel);
                }
                catch
                {
                    // ignored
                }
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