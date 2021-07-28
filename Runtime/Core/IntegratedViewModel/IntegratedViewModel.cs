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
    public interface IReadOnlyIntegratedViewModel<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        TValue this[TKey key] { get; }

        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }

        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }

    public abstract class IntegratedViewModel : IReadOnlyIntegratedViewModel<string, IBindableProperty>
    {
        [NonSerialized] Dictionary<string, IBindableProperty> bindableProperties;

        Dictionary<string, IBindableProperty> InternalBindableProperties { get { CheckPropertiesIsNull(); return bindableProperties; } set { bindableProperties = value; } }

        public IEnumerable<string> Keys { get { return InternalBindableProperties.Keys; } }

        public IEnumerable<IBindableProperty> Values { get { return InternalBindableProperties.Values; } }

        public IBindableProperty this[string _propertyName]
        {
            get { return InternalBindableProperties[_propertyName]; }
            set { InternalBindableProperties[_propertyName] = value; }
        }

        public IEnumerator<KeyValuePair<string, IBindableProperty>> GetEnumerator()
        {
            return InternalBindableProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalBindableProperties.GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return InternalBindableProperties.ContainsKey(key);
        }

        public bool TryGetValue(string key, out IBindableProperty value)
        {
            return InternalBindableProperties.TryGetValue(key, out value);
        }

        #region API
        public abstract void InitializeBindableProperties();

        void CheckPropertiesIsNull()
        {
            if (bindableProperties != null) return;
            bindableProperties = new Dictionary<string, IBindableProperty>();
            InitializeBindableProperties();
        }

        public virtual void BindingProperty<T>(string _propertyName, Action<T> _onValueChangedCallback)
        {
            this[_propertyName].AsBindableProperty<T>().RegesterValueChangedEvent(_onValueChangedCallback);
        }

        public virtual void UnBindingProperty<T>(string _propertyName, Action<T> _onValueChangedCallback)
        {
            this[_propertyName].AsBindableProperty<T>().UnregesterValueChangedEvent(_onValueChangedCallback);
        }

        protected virtual T GetPropertyValue<T>(string _propertyName)
        {
            return this[_propertyName].AsBindableProperty<T>().Value;
        }

        protected virtual void SetPropertyValue<T>(string _propertyName, T _value)
        {
            this[_propertyName].AsBindableProperty<T>().Value = _value;
        }
        #endregion
    }
}
