using System;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(StickyNote))]
    public sealed class StickyNoteProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        private StickyNote m_Model;
        private Type m_ModelType;

        public StickyNoteProcessor(StickyNote model)
        {
            this.m_Model = model;
            this.m_ModelType = model.GetType();
        }

        public StickyNote Model
        {
            get { return m_Model; }
        }

        public Type ModelType
        {
            get { return m_ModelType; }
        }

        object IGraphElementProcessor.Model
        {
            get { return m_Model; }
        }

        /// <summary> 唯一标识 </summary>
        public long ID
        {
            get { return Model.id; }
        }

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
    }
}