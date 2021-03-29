using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    public class EdgesDictionary : Dictionary<string, SerializableEdge>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<SerializableEdge> values = new List<SerializableEdge>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<string, SerializableEdge> pair in this)
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
