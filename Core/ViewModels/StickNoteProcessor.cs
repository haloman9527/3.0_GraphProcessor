using System;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(StickNote))]
    public sealed class StickNoteProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        public StickNote Model { get; }
        public Type ModelType { get; }

        /// <summary> 唯一标识 </summary>
        public int ID => Model.id;

        public InternalVector2Int Position
        {
            get => GetPropertyValue<InternalVector2Int>(nameof(StickNote.position));
            set => SetPropertyValue(nameof(StickNote.position), value);
        }

        public InternalVector2Int Size
        {
            get => GetPropertyValue<InternalVector2Int>(nameof(StickNote.size));
            set => SetPropertyValue(nameof(StickNote.size), value);
        }

        public string Title
        {
            get => GetPropertyValue<string>(nameof(StickNote.title));
            set => SetPropertyValue(nameof(StickNote.title), value);
        }

        public string Content
        {
            get => GetPropertyValue<string>(nameof(StickNote.content));
            set => SetPropertyValue(nameof(StickNote.content), value);
        }

        public StickNoteProcessor(StickNote model)
        {
            this.Model = model;
            this.ModelType = model.GetType();

            this.RegisterProperty(nameof(StickNote.title), () => ref model.title);
            this.RegisterProperty(nameof(StickNote.content), () => ref model.content);
            this.RegisterProperty(nameof(StickNote.position), () => ref model.position);
            this.RegisterProperty(nameof(StickNote.size), () => ref model.size);
        }
    }
}