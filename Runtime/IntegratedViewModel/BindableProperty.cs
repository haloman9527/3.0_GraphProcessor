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
        public BindableProperty(T defaultValue) { value = defaultValue; }
        public BindableProperty(Action<T> updateModel) { this.updateModel = updateModel; }
        public BindableProperty(T defaultValue, Action<T> updateModel) { value = defaultValue; this.updateModel = updateModel; }

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
        public void RegisterValueChangedEvent(Action<T> onValueChanged)
        {
            this.onValueChanged += onValueChanged;
        }
        public void RegisterValueChangedEvent<T1>(Action<T1> onValueChanged)
        {
            AsBindableProperty<T1>().RegisterValueChangedEvent(onValueChanged);
        }
        public void UnregisterValueChangedEvent(Action<T> onValueChanged)
        {
            this.onValueChanged -= onValueChanged;
        }
        public void UnregisterValueChangedEvent<T1>(Action<T1> onValueChanged)
        {
            AsBindableProperty<T1>().UnregisterValueChangedEvent(onValueChanged);
        }
        public virtual void SetValueWithoutNotify(T value)
        {
            this.value = value;
        }
        public void SetValueWithoutNotify(object value)
        {
            SetValueWithoutNotify((T)value);
        }
        public void ClearChangedEvent()
        {
            while (this.onValueChanged != null)
                this.onValueChanged -= this.onValueChanged;
        }
        public override string ToString()
        {
            return (Value != null ? Value.ToString() : "null");
        }
    }
}
