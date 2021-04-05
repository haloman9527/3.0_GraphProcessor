using System;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    public class ExposedParameter
    {
        [SerializeField]
        string name;
        [SerializeField]
        string typeQualifiedName;
        Type valueType;
        [SerializeField]
        string guid;
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.HideReferenceObjectPicker]
#endif
        [SerializeField, SerializeReference]
        object value;

        public string Name { get { return name; } set { name = value; } }
        public string GUID { get { return guid; } }
        public object Value
        {
            get { return value; }
            set
            {
                if (value != null && value.GetType() == ValueType)
                    this.value = value;
            }
        }
        public Type ValueType
        {
            get
            {
                if (valueType == null && !string.IsNullOrEmpty(typeQualifiedName)) valueType = Type.GetType(typeQualifiedName, false);
                return valueType;
            }
            private set
            {
                if (value != null)
                {
                    valueType = value;
                    typeQualifiedName = value.AssemblyQualifiedName;
                }
            }
        }

        public ExposedParameter(string _name, Type _valueType)
        {
            name = _name;
            guid = Guid.NewGuid().ToString();
            ValueType = _valueType;
        }
    }
}