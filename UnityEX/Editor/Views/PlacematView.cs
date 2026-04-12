#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    // 内置 Placemat 适配层：
    // - 交互过程仅更新视图
    // - 提交统一走 GraphViewChanged -> MoveElementsCommand
    // - title 使用会话化提交，避免每次 setter 都入栈
    public sealed class PlacematView : Placemat, IGraphElementView<PlacematProcessor>
    {
        private BaseGraphView m_Owner;
        private PlacematProcessor m_ViewModel;

        private bool m_SyncingFromModel;
        private bool m_TitleSessionActive;
        private string m_TitleStart;
        private string m_TitlePending;
        private IVisualElementScheduledItem m_TitleCommitTimer;

        public IGraphElementProcessor V => m_ViewModel;
        public PlacematProcessor ViewModel => m_ViewModel;
        public BaseGraphView Owner => m_Owner;

        public void SetUp(PlacematProcessor placemat, BaseGraphView owner)
        {
            m_ViewModel = placemat;
            m_Owner = owner;
        }

        public void Init()
        {
            m_SyncingFromModel = true;
            try
            {
                base.title = string.IsNullOrWhiteSpace(m_ViewModel.Title) ? "Placemat" : m_ViewModel.Title;
                base.Color = m_ViewModel.Color.ToColor();
                base.SetPosition(new Rect(m_ViewModel.Position.ToVector2(), m_ViewModel.Size.ToVector2()));
                SendToBack();
            }
            finally
            {
                m_SyncingFromModel = false;
            }

            m_ViewModel.RegisterValueChanged<string>(nameof(PlacematData.title), OnTitleChanged);
            m_ViewModel.RegisterValueChanged<InternalVector2Int>(nameof(PlacematData.position), OnPositionChanged);
            m_ViewModel.RegisterValueChanged<InternalVector2Int>(nameof(PlacematData.size), OnSizeChanged);
            m_ViewModel.RegisterValueChanged<InternalColor>(nameof(PlacematData.color), OnColorChanged);

            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
        }

        public void UnInit()
        {
            CommitTitleEdit();

            m_ViewModel.UnregisterValueChanged<string>(nameof(PlacematData.title), OnTitleChanged);
            m_ViewModel.UnregisterValueChanged<InternalVector2Int>(nameof(PlacematData.position), OnPositionChanged);
            m_ViewModel.UnregisterValueChanged<InternalVector2Int>(nameof(PlacematData.size), OnSizeChanged);
            m_ViewModel.UnregisterValueChanged<InternalColor>(nameof(PlacematData.color), OnColorChanged);

            UnregisterCallback<MouseUpEvent>(OnMouseUp);
            UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);

            m_TitleCommitTimer?.Pause();
            m_TitleCommitTimer = null;
        }

        public override string title
        {
            get => base.title;
            set
            {
                if (m_ViewModel == null || m_Owner == null || m_SyncingFromModel)
                {
                    base.title = value;
                    return;
                }

                if (!m_TitleSessionActive)
                {
                    m_TitleSessionActive = true;
                    m_TitleStart = string.IsNullOrWhiteSpace(m_ViewModel.Title) ? "Placemat" : m_ViewModel.Title;
                }

                m_TitlePending = string.IsNullOrWhiteSpace(value) ? "Placemat" : value.Trim();
                base.title = m_TitlePending;

                // 防抖：内置 Placemat 编辑时 title setter 可能多次触发
                m_TitleCommitTimer?.Pause();
                m_TitleCommitTimer = schedule.Execute(_ => CommitTitleEdit());
                m_TitleCommitTimer.ExecuteLater(250);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            // 只做视图预览，不直接写 Model、不直接入命令栈
            base.SetPosition(newPos);
            SendToBack();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            SendToBack();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
                CommitTitleEdit();
        }

        private void OnMouseCaptureOut(MouseCaptureOutEvent evt)
        {
            CommitTitleEdit();
        }

        private void CommitTitleEdit()
        {
            m_TitleCommitTimer?.Pause();
            m_TitleCommitTimer = null;

            if (!m_TitleSessionActive || m_Owner == null || m_ViewModel == null)
                return;

            m_TitleSessionActive = false;
            var oldTitle = string.IsNullOrWhiteSpace(m_TitleStart) ? "Placemat" : m_TitleStart;
            var newTitle = string.IsNullOrWhiteSpace(m_TitlePending) ? "Placemat" : m_TitlePending;
            if (oldTitle == newTitle)
                return;

            m_Owner.Context.Do(() => { m_ViewModel.Title = newTitle; }, () => { m_ViewModel.Title = oldTitle; });
        }

        private void OnTitleChanged(ViewModel.ValueChangedArg<string> arg)
        {
            m_SyncingFromModel = true;
            try
            {
                base.title = string.IsNullOrWhiteSpace(arg.newValue) ? "Placemat" : arg.newValue;
            }
            finally
            {
                m_SyncingFromModel = false;
            }

            m_Owner?.SetDirty();
        }

        private void OnPositionChanged(ViewModel.ValueChangedArg<InternalVector2Int> arg)
        {
            m_SyncingFromModel = true;
            try
            {
                base.SetPosition(new Rect(arg.newValue.ToVector2(), m_ViewModel.Size.ToVector2()));
                SendToBack();
            }
            finally
            {
                m_SyncingFromModel = false;
            }

            m_Owner?.SetDirty();
        }

        private void OnSizeChanged(ViewModel.ValueChangedArg<InternalVector2Int> arg)
        {
            m_SyncingFromModel = true;
            try
            {
                base.SetPosition(new Rect(m_ViewModel.Position.ToVector2(), arg.newValue.ToVector2()));
                SendToBack();
            }
            finally
            {
                m_SyncingFromModel = false;
            }

            m_Owner?.SetDirty();
        }

        private void OnColorChanged(ViewModel.ValueChangedArg<InternalColor> arg)
        {
            base.Color = arg.newValue.ToColor();
            m_Owner?.SetDirty();
        }
    }
}
#endif
