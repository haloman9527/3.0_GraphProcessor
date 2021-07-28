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
using System.Collections;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public class BindableList<T> : BindableProperty<List<T>>, IEnumerable<T>, IList<T>
    {
        public event Action onAdded;
        public event Action<int> onInserted;
        public event Action<T> onRemoved;
        public event Action<int> onItemChanged;
        public event Action onClear;

        public T this[int index] { get { return Value[index]; } set { SetItem(index, value); } }
        public int Count { get { return Value.Count; } }
        public bool IsReadOnly { get { return false; } }

        public BindableList() { Value = new List<T>(); }
        public BindableList(List<T> _default) : this(_default as IEnumerable<T>) { }
        public BindableList(IEnumerable<T> _enumerable) { Value = new List<T>(_enumerable); }

        public override void SetValueWithoutNotify(List<T> _value)
        {
            base.SetValueWithoutNotify(new List<T>(_value));
        }

        public void Add(T _item)
        {
            Value.Add(_item);
            onAdded?.Invoke();
        }

        public void Insert(int _index, T _item)
        {
            Value.Insert(_index, _item);
            onInserted?.Invoke(_index);
        }

        public bool Remove(T _item)
        {
            if (Value.Remove(_item))
            {
                onRemoved?.Invoke(_item);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            Value.Clear();
            onClear?.Invoke();
        }

        protected void SetItem(int index, T item)
        {
            Value[index] = item;
            onItemChanged?.Invoke(index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Value.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            Remove(Value[index]);
        }

        public bool Contains(T item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }
    }
}
