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
    public interface IReadOnlyIntegratedViewModel<TKey, TValue>
    {
        TValue this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }

        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }

    public abstract class IntegratedViewModel : IReadOnlyIntegratedViewModel<string, IBindableProperty>, IEnumerable<KeyValuePair<string, IBindableProperty>>, IEnumerable
    {
        [NonSerialized]
        Dictionary<string, IBindableProperty> bindableProperties;

        Dictionary<string, IBindableProperty> InternalBindableProperties
        {
            get
            {
                if (bindableProperties == null)
                {
                    bindableProperties = new Dictionary<string, IBindableProperty>();
                    BindProperties();
                }
                return bindableProperties;
            }
            set { bindableProperties = value; }
        }

        public IEnumerable<string> Keys { get { return InternalBindableProperties.Keys; } }

        public IEnumerable<IBindableProperty> Values { get { return InternalBindableProperties.Values; } }

        public IBindableProperty this[string propertyName]
        {
            get { return InternalBindableProperties[propertyName]; }
            set { InternalBindableProperties[propertyName] = value; }
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

        void CheckPropertiesIsNull()
        {

        }

        protected abstract void BindProperties();

        protected T GetPropertyValue<T>(string propertyName)
        {
            return this[propertyName].AsBindableProperty<T>().Value;
        }

        protected void SetPropertyValue<T>(string propertyName, T value)
        {
            this[propertyName].AsBindableProperty<T>().Value = value;
        }

        public void BindingProperty<T>(string propertyName, Action<T> onValueChangedCallback)
        {
            this[propertyName].AsBindableProperty<T>().RegesterValueChangedEvent(onValueChangedCallback);
        }

        public void UnBindingProperty<T>(string propertyName, Action<T> onValueChangedCallback)
        {
            this[propertyName].AsBindableProperty<T>().UnregesterValueChangedEvent(onValueChangedCallback);
        }
    }
}
