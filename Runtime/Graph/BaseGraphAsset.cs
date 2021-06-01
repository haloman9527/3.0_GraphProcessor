using OdinSerializer;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class BaseGraphAsset : ScriptableObject, ICloneable
    {
        public BaseGraphAsset() { }

        public abstract IBaseGraph Graph { get; }

        public object Clone() { return Instantiate(this); }
    }

    [Serializable]
    public abstract class BaseGraphAsset<T> : BaseGraphAsset, ISerializationCallbackReceiver where T : IBaseGraph, IBaseGraphFromUnityObject, new()
    {
        [SerializeField, HideInInspector]
        T graph = new T();

        public T TGraph { get { return graph; } }
        public override IBaseGraph Graph { get { return graph; } }

        public BaseGraphAsset() { }

        protected virtual void OnEnable()
        {
            CheckSerialization();
            graph.SetFrom(this);
            graph.Flush();
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
    }
}