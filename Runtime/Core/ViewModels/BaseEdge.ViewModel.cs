#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;

namespace CZToolKit.GraphProcessor
{
    public partial class BaseEdge : IntegratedViewModel
    {
        #region 静态方法
        public static BaseEdge CreateNewEdge(NodePort _inputPort, NodePort _outputPort)
        {
            BaseEdge edge = new BaseEdge();

            edge.guid = Guid.NewGuid().ToString();

            edge.inputNodeGUID = _inputPort.Owner.GUID;
            edge.inputFieldName = _inputPort.FieldName;

            edge.outputNodeGUID = _outputPort.Owner.GUID;
            edge.outputFieldName = _outputPort.FieldName;

            return edge;
        }
        #endregion


        [NonSerialized] BaseGraph owner;
        public BaseGraph Owner
        {
            get { return owner; }
            private set { owner = value; }
        }
        public string GUID { get { return guid; } }
        public string InputNodeGUID { get { return inputNodeGUID; } }
        public string OutputNodeGUID { get { return outputNodeGUID; } }
        public string InputFieldName { get { return inputFieldName; } }
        public string OutputFieldName { get { return outputFieldName; } }

        public BaseNode InputNode { get { Owner.Nodes.TryGetValue(InputNodeGUID, out BaseNode node); return node; } }
        public BaseNode OutputNode { get { Owner.Nodes.TryGetValue(OutputNodeGUID, out BaseNode node); return node; } }
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
        public void Enable(BaseGraph _graph)
        {
            Owner = _graph;
        }

        public override void InitializeBindableProperties() { }
    }
}
