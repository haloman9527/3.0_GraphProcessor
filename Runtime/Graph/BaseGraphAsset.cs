using OdinSerializer;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class BaseGraphAsset : ScriptableObject, IGraphAsset, ICloneable
    {
        public BaseGraphAsset() { }

        public abstract IBaseGraph Graph { get; }

        public virtual object Clone() { return Instantiate(this); }
    }

    [Serializable]
    public abstract class BaseGraphAsset<GraphClass> : BaseGraphAsset, ISerializationCallbackReceiver
        where GraphClass : IBaseGraph, IBaseGraphFromAsset, new()
    {
        [HideInInspector]
        [SerializeField]
        GraphClass graph = new GraphClass();

        public GraphClass TGraph { get { return graph; } }
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
        [HideInInspector]
        [SerializeField]
        [TextArea(20, 20)]
        string serializedGraph = String.Empty;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> variablesUnityReference = new List<UnityObject>();

        void Serialize()
        {
            serializedGraph = Encoding.UTF8.GetString(SerializationUtility.SerializeValue(graph, DataFormat.JSON, out variablesUnityReference));
        }

        void Deserialize()
        {
            graph = SerializationUtility.DeserializeValue<GraphClass>(Encoding.UTF8.GetBytes(serializedGraph), DataFormat.JSON, variablesUnityReference);
            if (graph == null)
                graph = new GraphClass();
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