using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class ExposedParmetersDictionary : Dictionary<string, ExposedParameter>, ISerializationCallbackReceiver
    {
        [SerializeField] List<string> keys = new List<string>();
        [SerializeField] List<ExposedParameter> values = new List<ExposedParameter>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<string, ExposedParameter> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception("there are " + keys.Count + " keys and " + values.Count + " values after deserialization. Make sure that both key and value types are serializable.");

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }

    [Serializable]
    public class ExposedParameter
    {
        [SerializeField] 
        string name;
        [SerializeField] 
        string guid;
        [SerializeField]
        string typeQualifiedName;
        Type valueType;
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

        public ExposedParameter(string _name, object _defaultValue, Type _valueType)
        {
            name = _name;
            guid = Guid.NewGuid().ToString();
            ValueType = _valueType;
            value = _defaultValue;
        }
    }
}