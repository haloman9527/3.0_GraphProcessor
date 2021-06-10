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

        public abstract IGraph Graph { get; }

        public abstract void SaveGraph();

        public abstract void CheckGraphSerialization();

        public virtual object Clone() { return Instantiate(this); }
    }

    [Serializable]
    public abstract class BaseGraphAsset<GraphClass> : BaseGraphAsset, ISerializationCallbackReceiver
        where GraphClass : IGraph, IGraphFromAsset, new()
    {
        [HideInInspector]
        [SerializeField]
        GraphClass graph = new GraphClass();

        public GraphClass TGraph { get { return graph; } }
        public override IGraph Graph { get { return graph; } }

        public BaseGraphAsset() { }

        protected virtual void OnEnable()
        {
            CheckGraphSerialization();
        }

        #region Serialize
        [NonSerialized]
        bool initializedGraph;
        [HideInInspector]
        [SerializeField]
        [TextArea(20, 20)]
        string serializedGraph = String.Empty;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> graphUnityReferences = new List<UnityObject>();

        public override void SaveGraph()
        {
            serializedGraph = Encoding.UTF8.GetString(SerializationUtility.SerializeValue(graph, DataFormat.JSON, out graphUnityReferences));
        }

        void DeserializeGraph()
        {
            graph = SerializationUtility.DeserializeValue<GraphClass>(Encoding.UTF8.GetBytes(serializedGraph), DataFormat.JSON, graphUnityReferences);
            if (graph == null)
                graph = new GraphClass();
            graph.Enable(this);
            graph.Flush();
        }

        public void OnBeforeSerialize()
        {
            //SaveGraph();
        }

        public void OnAfterDeserialize()
        {
            CheckGraphSerialization();
        }

        public override void CheckGraphSerialization()
        {
            if (initializedGraph) return;
            initializedGraph = true;
            DeserializeGraph();
        }
        #endregion
    }
}