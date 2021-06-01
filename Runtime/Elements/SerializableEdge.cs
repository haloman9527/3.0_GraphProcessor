using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class SerializableEdge
    {
        [NonSerialized]
        IBaseGraph owner;

        /// <summary> 自身GUID </summary>
        [SerializeField]
        string guid;

        [SerializeField]
        string inputNodeGUID;
        [SerializeField]
        string inputFieldName;

        [SerializeField]
        string outputNodeGUID;
        [SerializeField]
        string outputFieldName;

        public IBaseGraph Owner { get { return owner; } }
        public string GUID { get { return guid; } }
        public string InputNodeGUID { get { return inputNodeGUID; } }
        public string OutputNodeGUID { get { return outputNodeGUID; } }

        public BaseNode InputNode { get { Owner.NodesGUIDMapping.TryGetValue(InputNodeGUID, out BaseNode node); return node; } }
        public BaseNode OutputNode { get { Owner.NodesGUIDMapping.TryGetValue(OutputNodeGUID, out BaseNode node); return node; } }
        public string InputFieldName { get { return inputFieldName; } }
        public string OutputFieldName { get { return outputFieldName; } }
        public NodePort InputPort
        {
            get
            {
                if (InputNode == null) return null;
                InputNode.TryGetPort(InputFieldName, out NodePort nodePort);
                return nodePort;
            }
        }
        public NodePort OutputPort
        {
            get
            {
                if (OutputNode == null) return null;
                OutputNode.TryGetPort(OutputFieldName, out NodePort nodePort);
                return nodePort;
            }
        }

        public SerializableEdge() { }

        public void Initialize(IBaseGraph _graph)
        {
            owner = _graph;
        }

        public static SerializableEdge CreateNewEdge(IBaseGraph _graph, NodePort _inputPort, NodePort _outputPort)
        {
            SerializableEdge edge = new SerializableEdge();

            edge.guid = Guid.NewGuid().ToString();
            edge.owner = _graph;

            edge.inputNodeGUID = _inputPort.Owner.GUID;
            edge.inputFieldName = _inputPort.FieldName;
            edge.outputNodeGUID = _outputPort.Owner.GUID;
            edge.outputFieldName = _outputPort.FieldName;

            return edge;
        }
    }
}
