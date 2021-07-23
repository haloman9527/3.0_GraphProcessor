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

        void ValueChanged();
    }

    public interface IBindableProperty<T>
    {
        event Action<T> onValueChanged;

        T Value { get; set; }

        void ValueChanged();
        void RegesterValueChangedEvent(Action<T> _onValueChanged);
        void UnregesterValueChangedEvent(Action<T> _onValueChanged);
        void SetValueWithoutNotify(T _value);
    }
}
