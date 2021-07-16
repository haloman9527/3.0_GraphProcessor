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
    public abstract class BaseGraphElement
    {
        #region ViewModel
        [NonSerialized] Dictionary<string, IBindableProperty> bindableProperties;

        public IReadOnlyDictionary<string, IBindableProperty> BindableProperties { get { CheckPropertiesIsNull(); return bindableProperties; } }

        void CheckPropertiesIsNull()
        {
            if (bindableProperties != null) return;
            bindableProperties = new Dictionary<string, IBindableProperty>();
            InitializeBindableProperties();
        }

        public abstract void InitializeBindableProperties();

        public virtual void SetBindableProperty(string _propertyName, IBindableProperty _value)
        {
            CheckPropertiesIsNull();
            bindableProperties[_propertyName] = _value;
        }

        public virtual BindableProperty<T> GetBindableProperty<T>(string _propertyName)
        {
            CheckPropertiesIsNull();
            if (bindableProperties.TryGetValue(_propertyName, out IBindableProperty bindableProperty))
            {
                BindableProperty<T> tBindableProperty = bindableProperty as BindableProperty<T>;
                if (tBindableProperty != null)
                    return tBindableProperty;
                else
                    throw new Exception($"类型不一致，请检查！  {bindableProperty.GetType()}");
            }
            return null;
        }

        public virtual IBindableProperty GetBindableProperty(string _propertyName)
        {
            CheckPropertiesIsNull();
            if (bindableProperties.TryGetValue(_propertyName, out IBindableProperty bindableProperty))
                return bindableProperty;
            return null;
        }

        public virtual void RegisterValueChangedEvent<T>(string _propertyName, Action<T> _onValueChangedCallback)
        {
            GetBindableProperty<T>(_propertyName).RegesterValueChangedEvent(_onValueChangedCallback);
        }

        public virtual void UnregisterValueChangedEvent<T>(string _propertyName, Action<T> _onValueChangedCallback)
        {
            GetBindableProperty<T>(_propertyName).UnregesterValueChangedEvent(_onValueChangedCallback);
        }

        protected virtual T GetPropertyValue<T>(string _propertyName)
        {
            return GetBindableProperty<T>(_propertyName).Value;
        }

        protected virtual void SetPropertyValue<T>(string _propertyName, T _value)
        {
            GetBindableProperty<T>(_propertyName).Value = _value;
        }

        public virtual void UpdateProperties()
        {
            CheckPropertiesIsNull();
            foreach (var property in bindableProperties.Values)
            {
                property.ValueChanged();
            }
        }
        #endregion
    }
}
