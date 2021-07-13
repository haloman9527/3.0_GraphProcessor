#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class BindableProperty<T> : IBindableProperty, IBindableProperty<T>
    {
        T value;
        public event Action<T> onValueChanged;
        event Action<T> updateModel;

        public T Value
        {
            get { return value; }
            set
            {
                updateModel?.Invoke(value);
                if (!Equals(this.value, value))
                {
                    this.value = value;
                    ValueChanged();
                }
            }
        }

        public object ValueBoxed
        {
            get { return Value; }
            set { Value = (T)value; }
        }

        public Type ValueType { get { return typeof(T); } }

        public BindableProperty() { }
        public BindableProperty(T _default) { value = _default; }
        public BindableProperty(Action<T> _updateModel) { updateModel = _updateModel; }
        public BindableProperty(T _default, Action<T> _updateModel) { value = _default; updateModel = _updateModel; }

        public void ValueChanged()
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(Value);
        }

        public void RegesterValueChangedEvent(Action<T> _onValueChanged)
        {
            onValueChanged += _onValueChanged;
        }

        public void UnregesterValueChangedEvent(Action<T> _onValueChanged)
        {
            onValueChanged -= _onValueChanged;
        }

        public void SetValueWithoutNotify(T _value)
        {
            value = _value;
        }

        public override string ToString()
        {
            return (Value != null ? Value.ToString() : "null");
        }
    }
}
