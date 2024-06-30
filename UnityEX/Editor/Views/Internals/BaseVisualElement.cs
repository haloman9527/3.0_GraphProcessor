
#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseVisualElement : VisualElement
    {
        public enum ChildChangedType
        {
            Added,
            Removed
        }
        
        public class ChildChangedEvent : EventBase<ChildChangedEvent>
        {
            public ChildChangedType type;
            public VisualElement target;
        }

        public new void Add(VisualElement child)
        {
            base.Add(child);
            var evt = ChildChangedEvent.GetPooled();
            evt.type = ChildChangedType.Added;
            evt.target = child;
            this.SendEvent(evt);
        }

        public new void Remove(VisualElement child)
        {
            base.Remove(child);
            var evt = ChildChangedEvent.GetPooled();
            evt.type = ChildChangedType.Removed;
            evt.target = child;
            this.SendEvent(evt);
        }
    }
}
#endif