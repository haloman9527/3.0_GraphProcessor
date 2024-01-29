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
            get { return GetPropertyValue<InternalVector2Int>(nameof(StickNote.position)); }
            set { SetPropertyValue(nameof(StickNote.position), value); }
        }

        public InternalVector2Int Size
        {
            get { return GetPropertyValue<InternalVector2Int>(nameof(StickNote.size)); }
            set { SetPropertyValue(nameof(StickNote.size), value); }
        }

        public string Title
        {
            get { return GetPropertyValue<string>(nameof(StickNote.title)); }
            set { SetPropertyValue(nameof(StickNote.title), value); }
        }

        public string Content
        {
            get { return GetPropertyValue<string>(nameof(StickNote.content)); }
            set { SetPropertyValue(nameof(StickNote.content), value); }
        }

        public StickNoteProcessor(StickNote model)
        {
            this.Model = model;
            this.ModelType = model.GetType();

            this[nameof(StickNote.title)] = new BindableProperty<string>(() => model.title, v => model.title = v);
            this[nameof(StickNote.content)] = new BindableProperty<string>(() => model.content, v => model.content = v);
            this[nameof(StickNote.position)] = new BindableProperty<InternalVector2Int>(() => model.position, v => model.position = v);
            this[nameof(StickNote.size)] = new BindableProperty<InternalVector2Int>(() => model.size, v => model.size = v);
        }
    }
}