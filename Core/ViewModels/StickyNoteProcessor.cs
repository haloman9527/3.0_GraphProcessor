using System;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(StickyNote))]
    public sealed class StickyNoteProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        private StickyNote model;
        private Type modelType;

        public StickyNote Model => model;
        public Type ModelType => modelType;

        object IGraphElementProcessor.Model => model;

        Type IGraphElementProcessor.ModelType => modelType;

        /// <summary> 唯一标识 </summary>
        public int ID => Model.id;

        public InternalVector2Int Position
        {
            get => Model.position;
            set => SetFieldValue(ref Model.position, value, nameof(StickyNote.position));
        }

        public InternalVector2Int Size
        {
            get => Model.size;
            set => SetFieldValue(ref Model.size, value, nameof(StickyNote.size));
        }

        public string Title
        {
            get => Model.title;
            set => SetFieldValue(ref Model.title, value, nameof(StickyNote.title));
        }

        public string Content
        {
            get => Model.content;
            set => SetFieldValue(ref Model.content, value, nameof(StickyNote.content));
        }

        public StickyNoteProcessor(StickyNote model)
        {
            this.model = model;
            this.modelType = model.GetType();
        }
    }
}