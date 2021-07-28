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
    public abstract class BaseGraphElement : IEnumerable<KeyValuePair<string, IBindableProperty>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, IBindableProperty>>
    {
        #region ViewModel
        [NonSerialized] Dictionary<string, IBindableProperty> bindableProperties;

        Dictionary<string, IBindableProperty> InternalBindableProperties { get { CheckPropertiesIsNull(); return bindableProperties; } set { bindableProperties = value; } }
        public IReadOnlyDictionary<string, IBindableProperty> BindableProperties { get { return InternalBindableProperties; } }

        public int Count { get { return InternalBindableProperties.Count; } }

        public IBindableProperty this[string _propertyName]
        {
            get { return InternalBindableProperties[_propertyName]; }
            set { InternalBindableProperties[_propertyName] = value; }
        }

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

        public IEnumerator<KeyValuePair<string, IBindableProperty>> GetEnumerator()
        {
            return InternalBindableProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalBindableProperties.GetEnumerator();
        }
        #endregion
    }
}
