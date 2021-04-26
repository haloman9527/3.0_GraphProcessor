using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class ParamNameGUIDDictionary : Dictionary<string, string>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<string> values = new List<string>();

        public void OnBeforeSerialize()
        {
            if (keys == null)
                keys = new List<string>();
            else
                keys.Clear();
            if (values == null)
                values = new List<string>();
            else
                values.Clear();

            foreach (KeyValuePair<string, string> pair in this)
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
}