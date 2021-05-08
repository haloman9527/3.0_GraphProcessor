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
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.HideReferenceObjectPicker]
#endif
    public abstract class SharedVariable : ICloneable
    {
        [SerializeField]
        string guid;

        public string GUID
        {
            get { return this.guid; }
            protected set { this.guid = value; }
        }

        public SharedVariable() { guid = Guid.NewGuid().ToString(); }

        public SharedVariable(string _guid) { guid = _guid; }

        public abstract object GetValue();

        public abstract void SetValue(object value);

        public abstract Type GetValueType();

        public virtual void InitializePropertyMapping(BehaviorSource behaviorSource) { }

        public abstract object Clone();
    }

    [Serializable]
    public abstract class SharedVariable<T> : SharedVariable
    {
        [SerializeField]
        protected T value;

        [NonSerialized]
        Func<T> getter;
        [NonSerialized]
        Action<T> setter;

        public T Value
        {
            get
            {
                return this.getter == null ? this.value : this.getter();
            }
            set
            {
                if (this.setter != null)
                    this.setter(value);
                else
                    this.value = value;
            }
        }

        protected SharedVariable() : base() { value = default; }

        public SharedVariable(string _guid) : base(_guid) { value = default; }

        public SharedVariable(T _value) : base() { value = _value; }

        public override void InitializePropertyMapping(BehaviorSource _behaviorSource)
        {
            if (!(_behaviorSource.Owner.GetObject() is GraphOwner)) return;

            getter = () =>
            {
                SharedVariable variable = _behaviorSource.GetVariable(GUID);
                if (variable != null) return (T)variable.GetValue();
                return (T)GetValue();
            };
            setter = _value =>
            {
                SharedVariable variable = _behaviorSource.GetVariable(GUID);
                if (variable == null)
                {
                    variable = Activator.CreateInstance(this.GetType(), this.GUID) as SharedVariable;
                    _behaviorSource.SetVariable(variable.GUID, variable);
                }
                variable.SetValue(_value);
            };
        }

        public override object GetValue()
        {
            if (getter != null)
                return getter();
            else
                return value;
        }

        public override void SetValue(object _value)
        {
            if (setter != null)
                setter((T)_value);
            else
                value = (T)_value;
        }

        public override Type GetValueType() { return typeof(T); }

        public override string ToString()
        {
            string result;
            if (Value == null)
                result = "(null)";
            else
            {
                T value = this.Value;
                result = value.ToString();
            }
            return result;
        }
    }
}
