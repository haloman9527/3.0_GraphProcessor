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
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GroupView = UnityEditor.Experimental.GraphView.Group;

namespace Atom.GraphProcessor.Editors
{
    public sealed partial class GroupView : UnityEditor.Experimental.GraphView.Group, IGraphElementView<GroupProcessor>
    {
        public VisualElement GroupBorder { get; private set; }
        public TextField TitleField { get; private set; }
        public ColorField BackgroudColorField { get; private set; }
        public Label TitleLabel { get; private set; }
        public GroupProcessor ViewModel { get; protected set; }
        public IGraphElementProcessor V => ViewModel;
        public BaseGraphView Owner { get; private set; }

        bool WithoutNotify { get; set; }
        bool IsEditingColor { get; set; }
        InternalColor ColorEditStart { get; set; }
        InternalColor ColorEditLatest { get; set; }

        public GroupView()
        {
            this.styleSheets.Add(GraphProcessorEditorStyles.DefaultStyles.GroupViewStyle);

            this.GroupBorder = new VisualElement() { name = "group-border", pickingMode = PickingMode.Ignore };
            this.Add(GroupBorder);

            this.TitleLabel = headerContainer.Q<Label>();
            this.TitleField = headerContainer.Q<TextField>();
            this.BackgroudColorField = new ColorField();
            this.BackgroudColorField.name = "backgroundColorField";
            this.headerContainer.Add(BackgroudColorField);

            this.TitleField.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            this.TitleField.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
        }

        public void SetUp(GroupProcessor group, BaseGraphView graphView)
        {
            this.ViewModel = group;
            this.Owner = graphView;
            this.title = ViewModel.GroupName;
            this.style.backgroundColor = ViewModel.BackgroundColor.ToColor();
            this.style.unityBackgroundImageTintColor = ViewModel.BackgroundColor.ToColor();
            this.BackgroudColorField.SetValueWithoutNotify(ViewModel.BackgroundColor.ToColor());
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), GetPosition().size));
            WithoutNotify = true;
            // 使用 TryGetValue 单次查找替代 ContainsKey + 索引器双重查找，避免 LINQ 分配
            var nodeViewsToAdd = new List<UnityEditor.Experimental.GraphView.GraphElement>(ViewModel.Nodes.Count);
            foreach (var nodeID in ViewModel.Nodes)
            {
                if (Owner.NodeViews.TryGetValue(nodeID, out var nv))
                    nodeViewsToAdd.Add(nv);
            }
            base.AddElements(nodeViewsToAdd);
            WithoutNotify = false;
            BackgroudColorField.RegisterValueChangedCallback(OnGroupColorChanged);
        }

        public void Init()
        {
            ViewModel.PropertyChanged += OnViewModelChanged;
        }

        public void UnInit()
        {
            CommitPendingColorChange();
            EditorApplication.update -= PollColorPickerClose;
            ViewModel.PropertyChanged -= OnViewModelChanged;
        }

        #region Callbacks

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            var group = sender as GroupProcessor;
            switch (e.PropertyName)
            {
                case nameof(Group.position):
                {
                    base.SetPosition(new Rect(group.Position.ToVector2(), GetPosition().size));
                    break;
                }
                case nameof(Group.groupName):
                {
                    if (string.IsNullOrEmpty(group.GroupName))
                        return;
                    this.title = ViewModel.GroupName;
                    Owner.SetDirty();
                    break;
                }
                case nameof(Group.backgroundColor):
                {
                    this.BackgroudColorField.SetValueWithoutNotify(group.BackgroundColor.ToColor());
                    this.style.backgroundColor = group.BackgroundColor.ToColor();
                    this.style.unityBackgroundImageTintColor = group.BackgroundColor.ToColor();
                    Owner.SetDirty();
                    break;
                }
            }
        }

        public void OnNodesAdded(BaseNodeProcessor node)
        {
            if (WithoutNotify)
            {
                return;
            }

            var tmp = WithoutNotify;
            try
            {
                WithoutNotify = true;
                if (Owner.NodeViews.TryGetValue(node.ID, out var nodeView))
                    base.AddElements(new GraphElement[] { nodeView });
            }
            finally
            {
                WithoutNotify = tmp;
            }
        }

        public void OnNodesRemoved(BaseNodeProcessor node)
        {
            if (WithoutNotify)
            {
                return;
            }

            var tmp = WithoutNotify;
            try
            {
                WithoutNotify = true;
                if (Owner.NodeViews.TryGetValue(node.ID, out var nodeView))
                    base.RemoveElementsWithoutNotification(new GraphElement[] { nodeView });
            }
            finally
            {
                WithoutNotify = tmp;
            }
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Owner.Context.Do(new RenameGroupCommand(ViewModel, newName));
        }

        private void OnGroupColorChanged(ChangeEvent<Color> evt)
        {
            var newColor = evt.newValue.ToInternalColor();

            if (!IsEditingColor)
            {
                IsEditingColor = true;
                ColorEditStart = ViewModel.BackgroundColor;
                EditorApplication.update -= PollColorPickerClose;
                EditorApplication.update += PollColorPickerClose;
            }

            ColorEditLatest = newColor;
            // 交互期间直接更新视觉，不立刻写入命令栈
            ViewModel.BackgroundColor = newColor;
        }

        private void PollColorPickerClose()
        {
            if (IsColorPickerOpened())
                return;

            CommitPendingColorChange();
            EditorApplication.update -= PollColorPickerClose;
        }

        private void CommitPendingColorChange()
        {
            if (!IsEditingColor)
                return;

            IsEditingColor = false;
            var oldColor = ColorEditStart;
            var newColor = ColorEditLatest;
            if (IsSameColor(oldColor, newColor))
                return;

            Owner.Context.Do(new ChangeGroupColorCommand(ViewModel, oldColor, newColor));
        }

        private static bool IsSameColor(InternalColor a, InternalColor b)
        {
            return Mathf.Approximately(a.r, b.r) &&
                   Mathf.Approximately(a.g, b.g) &&
                   Mathf.Approximately(a.b, b.b) &&
                   Mathf.Approximately(a.a, b.a);
        }

        private static bool IsColorPickerOpened()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (var i = 0; i < windows.Length; i++)
            {
                var window = windows[i];
                if (window == null)
                    continue;

                var type = window.GetType();
                if (type != null && (type.Name == "ColorPicker" || type.FullName == "UnityEditor.ColorPicker"))
                    return true;
            }

            return false;
        }

        #endregion

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (!base.AcceptsElement(element, ref reasonWhyNotAccepted))
                return false;
            switch (element)
            {
                case BaseNodeView:
                    return true;
                case StickyNoteView:
                    return true;
            }

            return false;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (WithoutNotify)
                return;

            var tmp = WithoutNotify;
            WithoutNotify = true;
            // 避免 LINQ Where/Select/ToArray GC 分配
            var nodes = new List<BaseNodeProcessor>();
            foreach (var item in elements)
            {
                if (item is BaseNodeView nv)
                    nodes.Add(nv.ViewModel);
            }
            Owner.Context.Do(new AddToGroupCommand(Owner.ViewModel, this.ViewModel, nodes.ToArray()));

            Owner.SetDirty();
            WithoutNotify = tmp;
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (WithoutNotify)
                return;
            var tmp = WithoutNotify;
            WithoutNotify = true;
            // 避免 LINQ Where/Select/ToArray GC 分配
            var nodes = new List<BaseNodeProcessor>();
            foreach (var item in elements)
            {
                if (item is BaseNodeView nv)
                    nodes.Add(nv.ViewModel);
            }
            Owner.Context.Do(new RemoveFromGroupCommand(Owner.ViewModel, this.ViewModel, nodes.ToArray()));

            Owner.SetDirty();
            WithoutNotify = tmp;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            this.BringToFront();
        }
    }
}
#endif
