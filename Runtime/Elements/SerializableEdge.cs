using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    public class SerializableEdge
    {
        #region nonserialize
        [SerializeField]
        BaseGraph owner;
        #endregion

        #region serialize
        [SerializeField]
        string guid;

        [SerializeField]
        string inputNodeGUID;
        public string inputFieldName;

        [SerializeField]
        string outputNodeGUID;
        public string outputFieldName;
        #endregion
        public BaseGraph Owner { get { return owner; } }
        public string GUID { get { return guid; } }
        public string InputNodeGUID { get { return inputNodeGUID; } }
        public string OutputNodeGUID { get { return outputNodeGUID; } }

        public BaseNode InputNode { get { owner.Nodes.TryGetValue(inputNodeGUID, out BaseNode node); return node; } }
        public BaseNode OutputNode { get { owner.Nodes.TryGetValue(outputNodeGUID, out BaseNode node); return node; } }
        public NodePort InputPort { get { return InputNode != null ? InputNode.Ports[inputFieldName] : null; } }
        public NodePort OutputPort { get { return OutputNode != null ? OutputNode.Ports[outputFieldName] : null; } }

        public SerializableEdge() { }

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
