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
        [SerializeField]
        string inputFieldName;

        [SerializeField]
        string outputNodeGUID;
        [SerializeField]
        string outputFieldName;
        #endregion
        public BaseGraph Owner { get { return owner; } }
        public string GUID { get { return guid; } }
        public string InputNodeGUID { get { return inputNodeGUID; } }
        public string OutputNodeGUID { get { return outputNodeGUID; } }

        public BaseNode InputNode { get { owner.Nodes.TryGetValue(InputNodeGUID, out BaseNode node); return node; } }
        public BaseNode OutputNode { get { owner.Nodes.TryGetValue(OutputNodeGUID, out BaseNode node); return node; } }
        public string InputFieldName { get { return inputFieldName; } }
        public string OutputFieldName { get { return outputFieldName; } }
        public NodePort InputPort
        {
            get
            {
                NodePort nodePort = null;
                InputNode?.TryGetPort(InputFieldName, out nodePort);
                return nodePort;
            }
        }
        public NodePort OutputPort
        {
            get
            {
                NodePort nodePort = null;
                OutputNode?.TryGetPort(OutputFieldName, out nodePort);
                return nodePort;
            }
        }

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
