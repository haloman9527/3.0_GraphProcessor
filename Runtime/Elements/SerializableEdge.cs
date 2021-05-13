using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class SerializableEdge
    {
        /// <summary> 自身GUID </summary>
        [SerializeField]
        string guid;

        [SerializeField]
        BaseGraph owner;

        [SerializeField]
        string inputNodeGUID;
        [SerializeField]
        string inputFieldName;

        [SerializeField]
        string outputNodeGUID;
        [SerializeField]
        string outputFieldName;

        public BaseGraph Owner { get { return owner; } }
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

        public void Initialize(BaseGraph _owner)
        {
            owner = _owner;
        }

        public static SerializableEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort)
        {
            SerializableEdge edge = new SerializableEdge();

            edge.guid = Guid.NewGuid().ToString();
            edge.owner = graph;

            edge.inputNodeGUID = inputPort.Owner.GUID;
            edge.inputFieldName = inputPort.FieldName;
            edge.outputNodeGUID = outputPort.Owner.GUID;
            edge.outputFieldName = outputPort.FieldName;

            return edge;
        }
    }
}
