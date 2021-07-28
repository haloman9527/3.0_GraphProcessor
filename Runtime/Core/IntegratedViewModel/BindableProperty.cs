#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class BindableProperty<T> : IBindableProperty, IBindableProperty<T>
    {
        T value;
        public event Action<T> onValueChanged;
        public event Action<object> onBoxedValueChanged;
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
            if (onBoxedValueChanged != null)
                onBoxedValueChanged.Invoke(Value);

        }
        public IBindableProperty<T1> AsBindableProperty<T1>()
        {
            return this as BindableProperty<T1>;
        }
        public void RegesterValueChangedEvent(Action<T> _onValueChanged)
        {
            onValueChanged += _onValueChanged;
        }
        public void UnregesterValueChangedEvent(Action<T> _onValueChanged)
        {
            onValueChanged -= _onValueChanged;
        }
        public virtual void SetValueWithoutNotify(T _value)
        {
            value = _value;
        }
        public void SetValueWithoutNotify(object _value)
        {
            SetValueWithoutNotify((T)_value);
        }
        public override string ToString()
        {
            return (Value != null ? Value.ToString() : "null");
        }
    }
}
