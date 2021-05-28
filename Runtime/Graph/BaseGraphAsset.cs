using OdinSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;
using System.Text;

namespace CZToolKit.GraphProcessor
{
    public abstract class BaseGraphAsset : ScriptableObject, ICloneable
    {
        public BaseGraphAsset() { }

        public abstract BaseGraph Graph { get; }

        public object Clone() { return Instantiate(this); }
    }

    [Serializable]
    public abstract class BaseGraphAsset<T> : BaseGraphAsset, ISerializationCallbackReceiver where T : BaseGraph, new()
    {
        [SerializeField, HideInInspector]
        T graph = new T();

        public T TGraph { get { return graph; } }
        public override BaseGraph Graph { get { return graph; } }

        public BaseGraphAsset() { }

        protected virtual void OnEnable()
        {
            CheckSerialization();
            graph.OnEnable(this);
        }

        [NonSerialized]
        bool initializedVariables;
        [SerializeField]
        [TextArea(20, 20)]
        string serializedGraph = String.Empty;
        [SerializeField]
        List<UnityObject> unityReferences = new List<UnityObject>();

        void Serialize()
        {
            serializedGraph = Encoding.UTF8.GetString(SerializationUtility.SerializeValue(graph, DataFormat.JSON, out unityReferences));
        }

        void Deserialize()
        {
            graph = SerializationUtility.DeserializeValue<T>(Encoding.UTF8.GetBytes(serializedGraph), DataFormat.JSON, unityReferences);
            if (graph == null)
                graph = new T();
        }

        public void OnBeforeSerialize()
        {
            Serialize();
        }

        public void OnAfterDeserialize()
        {
            CheckSerialization();
        }

        public void CheckSerialization()
        {
            if (initializedVariables) return;
            initializedVariables = true;
            Deserialize();
        }

        public static implicit operator BaseGraph(BaseGraphAsset<T> _other) { return _other.Graph; }
    }
}