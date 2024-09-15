using System;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(StickyNote))]
    public sealed class StickyNoteProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        public StickyNote Model { get; }
        public Type ModelType { get; }

        /// <summary> 唯一标识 </summary>
        public int ID => Model.id;

        public InternalVector2Int Position
        {
            get => GetPropertyValue<InternalVector2Int>(nameof(StickyNote.position));
            set => SetPropertyValue(nameof(StickyNote.position), value);
        }

        public InternalVector2Int Size
        {
            get => GetPropertyValue<InternalVector2Int>(nameof(StickyNote.size));
            set => SetPropertyValue(nameof(StickyNote.size), value);
        }

        public string Title
        {
            get => GetPropertyValue<string>(nameof(StickyNote.title));
            set => SetPropertyValue(nameof(StickyNote.title), value);
        }

        public string Content
        {
            get => GetPropertyValue<string>(nameof(StickyNote.content));
            set => SetPropertyValue(nameof(StickyNote.content), value);
        }

        public StickyNoteProcessor(StickyNote model)
        {
            this.Model = model;
            this.ModelType = model.GetType();

            this.RegisterProperty(nameof(StickyNote.title), () => ref model.title);
            this.RegisterProperty(nameof(StickyNote.content), () => ref model.content);
            this.RegisterProperty(nameof(StickyNote.position), () => ref model.position);
            this.RegisterProperty(nameof(StickyNote.size), () => ref model.size);
        }
    }
}