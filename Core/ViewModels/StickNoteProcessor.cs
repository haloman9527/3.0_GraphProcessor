using System;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(StickNote))]
    public sealed class StickNoteProcessor : ViewModel, IGraphScopeViewModel
    {
        public StickNote Model { get; }
        public Type ModelType { get; }

        /// <summary> 唯一标识 </summary>
        public int ID
        {
            get { return Model.id; }
        }

        public InternalVector2Int Position
        {
            get { return GetField<InternalVector2Int>(nameof(StickNote.position)); }
            set { SetField(nameof(StickNote.position), value); }
        }

        public InternalVector2Int Size
        {
            get { return GetField<InternalVector2Int>(nameof(StickNote.size)); }
            set { SetField(nameof(StickNote.size), value); }
        }

        public string Title
        {
            get { return GetField<string>(nameof(StickNote.title)); }
            set { SetField(nameof(StickNote.title), value); }
        }

        public string Content
        {
            get { return GetField<string>(nameof(StickNote.content)); }
            set { SetField(nameof(StickNote.content), value); }
        }

        public StickNoteProcessor(StickNote model)
        {
            this.Model = model;
            this.ModelType = model.GetType();

            this.RegisterField(nameof(StickNote.title), () => ref model.title);
            this.RegisterField(nameof(StickNote.content), () => ref model.content);
            this.RegisterField(nameof(StickNote.position), () => ref model.position);
            this.RegisterField(nameof(StickNote.size), () => ref model.size);
        }
    }
}