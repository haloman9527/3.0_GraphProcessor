using System;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(StickyNode))]
    public sealed class StickyNodeProcessor : BaseNodeProcessor, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        private StickyNode stickyNode;

        public StickyNodeProcessor(StickyNode model) : base(model)
        {
            this.stickyNode = model;
        }

        public InternalVector2Int Size
        {
            get => stickyNode.size;
            set => SetFieldValue(ref stickyNode.size, value, nameof(StickyNode.size));
        }

        public override string Title
        {
            get => stickyNode.title;
            set => SetFieldValue(ref stickyNode.title, value, nameof(StickyNode.title));
        }

        public string Contents
        {
            get => stickyNode.contents;
            set => SetFieldValue(ref stickyNode.contents, value, nameof(StickyNode.contents));
        }
    }
}