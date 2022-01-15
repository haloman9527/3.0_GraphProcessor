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
    public interface IBindableProperty
    {
        event Action<object> onBoxedValueChanged;

        object ValueBoxed { get; set; }
        Type ValueType { get; }

        void SetValueWithoutNotify(object value);
        IBindableProperty<T> AsBindableProperty<T>();
        void RegisterValueChangedEvent<T>(Action<T> onValueChanged);
        void UnregisterValueChangedEvent<T>(Action<T> onValueChanged);
        void ClearChangedEvent();
    }

    public interface IBindableProperty<T>
    {
        event Action<T> onValueChanged;

        T Value { get; set; }

        void ValueChanged();
        void RegisterValueChangedEvent(Action<T> onValueChanged);
        void UnregisterValueChangedEvent(Action<T> onValueChanged);
        void SetValueWithoutNotify(T value);
        void ClearChangedEvent();
    }
}
