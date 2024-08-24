#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class StickNoteView : UnityEditor.Experimental.GraphView.StickyNote, IGraphElementView<StickNoteProcessor>
    {
        private TextField titleField;
        private TextField contentsField;

        private StickNoteProcessor viewModel;

        public StickNoteProcessor ViewModel
        {
            get { return viewModel; }
        }

        public BaseGraphView Owner { get; private set; }

        public StickNoteView()
        {
            var contents = this.Q<Label>("contents", (string)null);
            this.titleField = this.Q<TextField>("title-field", (string)null);
            this.contentsField = contents.Q<TextField>("contents-field", (string)null);
        }

        public void SetUp(StickNoteProcessor note, BaseGraphView graphView)
        {
            this.viewModel = note;
            this.Owner = graphView;
            // 初始化
            base.SetPosition(new Rect(ViewModel.Position.ToVector2(), ViewModel.Size.ToVector2()));
            this.title = note.Title;
            this.contents = note.Content;
        }

        public void OnCreate()
        {
            ViewModel.GetProperty<InternalVector2Int>(nameof(StickNote.position)).RegisterValueChangedEvent(OnPositionChanged);
            ViewModel.GetProperty<InternalVector2Int>(nameof(StickNote.size)).RegisterValueChangedEvent(OnSizeChanged);
            ViewModel.GetProperty<string>(nameof(StickNote.title)).RegisterValueChangedEvent(OnTitleChanged);
            ViewModel.GetProperty<string>(nameof(StickNote.content)).RegisterValueChangedEvent(OnContentsChanged);

            this.RegisterCallback<StickyNoteChangeEvent>(OnChanged);
        }

        public void OnDestroy()
        {
            ViewModel.GetProperty<InternalVector2Int>(nameof(StickNote.position)).UnregisterValueChangedEvent(OnPositionChanged);
            ViewModel.GetProperty<InternalVector2Int>(nameof(StickNote.size)).UnregisterValueChangedEvent(OnSizeChanged);
            ViewModel.GetProperty<string>(nameof(StickNote.title)).UnregisterValueChangedEvent(OnTitleChanged);
            ViewModel.GetProperty<string>(nameof(StickNote.content)).UnregisterValueChangedEvent(OnContentsChanged);

            this.UnregisterCallback<StickyNoteChangeEvent>(OnChanged);
        }

        private void OnChanged(StickyNoteChangeEvent evt)
        {
            switch (evt.change)
            {
                case StickyNoteChange.Title:
                {
                    var oldTitle = ViewModel.Title;
                    Owner.CommandDispatcher.Do(() => { ViewModel.Title = this.title; }, () => { ViewModel.Title = oldTitle; });
                    break;
                }
                case StickyNoteChange.Contents:
                {
                    var oldContent = ViewModel.Content;
                    Owner.CommandDispatcher.Do(() => { ViewModel.Content = this.title; }, () => { ViewModel.Content = oldContent; });
                    break;
                }
                case StickyNoteChange.Theme:
                    break;
                case StickyNoteChange.FontSize:
                    break;
                case StickyNoteChange.Position:
                {
                    var newPosition = GetPosition().position.ToInternalVector2Int();
                    var newSize = GetPosition().size.ToInternalVector2Int();
                    var oldPosition = ViewModel.Position;
                    var oldSize = ViewModel.Size;
                    Owner.CommandDispatcher.Do(() =>
                    {
                        ViewModel.Position = newPosition;
                        ViewModel.Size = newSize;
                    }, () =>
                    {
                        ViewModel.Position = oldPosition;
                        ViewModel.Size = oldSize;
                    });
                    break;
                }
            }
        }

        void OnPositionChanged(InternalVector2Int oldPosition, InternalVector2Int newPosition)
        {
            base.SetPosition(new Rect(newPosition.ToVector2(), GetPosition().size));
            Owner.SetDirty();
        }

        void OnSizeChanged(InternalVector2Int oldSize, InternalVector2Int newSize)
        {
            base.SetPosition(new Rect(GetPosition().position, newSize.ToVector2()));
            Owner.SetDirty();
        }

        private void OnContentsChanged(string oldvalue, string newvalue)
        {
            contentsField.SetValueWithoutNotify(newvalue);
        }

        private void OnTitleChanged(string oldvalue, string newvalue)
        {
            titleField.SetValueWithoutNotify(newvalue);
        }
    }
}
#endif