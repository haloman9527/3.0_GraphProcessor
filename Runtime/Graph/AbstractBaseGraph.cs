#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using CZToolKit.Core.Blackboards;
using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public abstract class AbstrackBaseGraph : IBaseGraph
    {
        public static readonly Vector2 DefaultBlackboardSize = new Vector2(150, 200);

        #region 变量
        [SerializeField]
        Vector3 position = Vector3.zero;
        [SerializeField]
        Vector3 scale = Vector3.one;
        [SerializeField]
        bool blackboardVisible = true;
        [SerializeField]
        Rect blackboardPosition = new Rect(Vector2.zero, DefaultBlackboardSize);

        [SerializeField]
        Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [SerializeField]
        Dictionary<string, SerializableEdge> edges = new Dictionary<string, SerializableEdge>();
        [SerializeField]
        Dictionary<string, BaseStack> stacks = new Dictionary<string, BaseStack>();
        [SerializeField]
        List<BaseGroup> groups = new List<BaseGroup>();

        [SerializeField]
        CZBlackboard blackboard = new CZBlackboard();

        [NonSerialized]
        List<SharedVariable> variables = new List<SharedVariable>();
        #endregion

        #region 属性
        public Vector3 Position { get { return position; } set { position = value; } }
        public Vector3 Scale { get { return scale; } set { scale = value; } }
        public bool BlackboardVisible { get { return blackboardVisible; } set { blackboardVisible = value; } }
        public Rect BlackboardPosition { get { return blackboardPosition; } set { blackboardPosition = value; } }
        public CZBlackboard Blackboard { get { return blackboard; } }
        public IVariableOwner VarialbeOwner { get; private set; }
        public IReadOnlyList<BaseGroup> Groups { get { return groups; } }
        public IReadOnlyDictionary<string, BaseNode> NodesGUIDMapping { get { return nodes; } }
        public IReadOnlyDictionary<string, SerializableEdge> EdgesGUIDMapping { get { return edges; } }
        public IReadOnlyDictionary<string, BaseStack> StackNodesGUIDMapping { get { return stacks; } }
        public IReadOnlyList<SharedVariable> Variables
        {
            get
            {
                if (variables == null) CollectionVariables();
                return variables;
            }
        }
        #endregion

        public virtual void Initialize(IGraphAssetOwner _graphOwner)
        {
            InitializePropertyMapping(_graphOwner);
        }

        private void CollectionVariables()
        {
            if (variables == null)
                variables = new List<SharedVariable>();
            else
                variables.Clear();
            foreach (var node in nodes.Values)
            {
                variables.AddRange(SharedVariableUtility.CollectionObjectSharedVariables(node));
            }
        }

        public void InitializePropertyMapping(IVariableOwner _variableOwner)
        {
            VarialbeOwner = _variableOwner;
            foreach (var variable in Variables)
            {
                variable.InitializePropertyMapping(_variableOwner);
            }
        }

        public virtual void Flush()
        {
            if (blackboard == null)
                blackboard = new CZBlackboard();
            else
                blackboard.Fixed();
            CleanNullElements();
            // 更新节点端口
            foreach (var node in NodesGUIDMapping.Values)
            {
                NodeDataCache.UpdateStaticPorts(node);
            }
            FixElements();
        }

        // 清理无效数据
        void CleanNullElements()
        {
            foreach (var kv in nodes.ToArray())
            {
                if (kv.Value == null)
                {
                    nodes.Remove(kv.Key);
                    continue;
                }
                kv.Value.Initialize(this);
            }

            foreach (var kv in edges.ToArray())
            {
                if (kv.Value == null)
                {
                    nodes.Remove(kv.Key);
                    continue;
                }
                kv.Value.Initialize(this);
            }

            foreach (var kv in stacks.ToArray())
            {
                if (kv.Value == null)
                {
                    nodes.Remove(kv.Key);
                    continue;
                }
            }

            groups.RemoveAll(item => item == null);

        }

        void FixElements()
        {
            foreach (var edge in edges.Values.ToArray())
            {
                if (edge.InputNode == null || edge.OutputNode == null || edge.InputNode == edge.OutputNode)
                {
                    Disconnect(edge.GUID);
                    edge.InputPort?.DisconnectEdge(edge);
                    edge.OutputPort?.DisconnectEdge(edge);
                    continue;
                }

                if (edge.InputPort == null || edge.OutputPort == null || edge.InputPort.Direction == edge.OutputPort.Direction)
                {
                    Disconnect(edge.GUID);
                    edge.InputPort?.DisconnectEdge(edge);
                    edge.OutputPort?.DisconnectEdge(edge);
                    continue;
                }

                if (!edge.InputPort.EdgeGUIDS.Contains(edge.GUID) || !edge.OutputPort.EdgeGUIDS.Contains(edge.GUID))
                {
                    Disconnect(edge.GUID);
                    edge.InputPort.DisconnectEdge(edge);
                    edge.OutputPort.DisconnectEdge(edge);
                    continue;
                }
            }

            foreach (var node in nodes.Values.ToArray())
            {
                foreach (var nodePort in node.Ports.ToArray())
                {
                    if (nodePort.Value == null)
                    {
                        node.Ports.Remove(nodePort.Key);
                        continue;
                    }
                    nodePort.Value.EdgeGUIDS.RemoveAll(edgeGUID => !edges.ContainsKey(edgeGUID));
                }
            }

            for (int i = 0; i < groups.Count; i++)
            {
                BaseGroup group = groups[i];
                group.innerNodeGUIDs.RemoveAll(nodeGUID => !NodesGUIDMapping.ContainsKey(nodeGUID));
                group.innerStackGUIDs.RemoveAll(stackGUID => !stacks.ContainsKey(stackGUID));
            }
        }

        #region Operation
        public T AddNode<T>(Vector2 _nodePosition) where T : BaseNode
        {
            T node = BaseNode.CreateNew<T>(_nodePosition);
            AddNode(node);
            return node;
        }

        public void AddNode(BaseNode _node)
        {
            if (_node == null) return;
            _node.Initialize(this);
            nodes[_node.GUID] = _node;
            NodeDataCache.UpdateStaticPorts(_node);
            IEnumerable<SharedVariable> nodeVariables = SharedVariableUtility.CollectionObjectSharedVariables(_node);
            if (VarialbeOwner != null)
            {
                foreach (var variable in nodeVariables)
                {
                    variable.InitializePropertyMapping(VarialbeOwner);
                }
            }
            if (nodeVariables != null)
                variables.AddRange(nodeVariables);
        }

        public void RemoveNode(BaseNode _node)
        {
            if (_node == null) return;
            // 断开这个节点的所有连接,移除节点
            Disconnect(_node);
            nodes.Remove(_node.GUID);
        }

        public SerializableEdge Connect(NodePort _inputPort, NodePort _outputPort)
        {
            // 在连接两个端口，如果端口设置为只能连接一个端口，则需要在连接前把其他所有连接断开
            if (!_inputPort.IsMulti)
                Disconnect(_inputPort);
            if (!_outputPort.IsMulti)
                Disconnect(_outputPort);

            // 创建一条连线
            SerializableEdge edge = SerializableEdge.CreateNewEdge(this, _inputPort, _outputPort);
            AddEdge(edge);

            _inputPort.ConnectEdge(edge);
            _outputPort.ConnectEdge(edge);

            _inputPort.Owner.OnConnected(_inputPort, _outputPort);
            _outputPort.Owner.OnConnected(_outputPort, _inputPort);

            return edge;
        }

        private void AddEdge(SerializableEdge _edge)
        {
            _edge.Initialize(this);
            edges[_edge.GUID] = _edge;
        }

        public void Disconnect(SerializableEdge _edge)
        {
            if (_edge != null) Disconnect(_edge.GUID);
        }

        public void Disconnect(string _edgeGUID)
        {
            if (EdgesGUIDMapping.TryGetValue(_edgeGUID, out SerializableEdge edge))
            {
                if (edge != null)
                {
                    edge.InputPort?.DisconnectEdge(edge);
                    edge.OutputPort?.DisconnectEdge(edge);
                    edge.InputNode?.OnDisconnected(edge.InputPort, edge.OutputPort);
                    edge.OutputNode?.OnDisconnected(edge.OutputPort, edge.InputPort);
                }
                RemoveEdge(edge);
            }
        }

        private void RemoveEdge(SerializableEdge _edge)
        {
            edges.Remove(_edge.GUID);
        }

        public void Disconnect(NodePort _nodePort)
        {
            for (int i = 0; i < _nodePort.EdgeGUIDS.Count; i++)
            {
                Disconnect(_nodePort.EdgeGUIDS[i]);
            }
        }

        public void Disconnect(BaseNode _node)
        {
            foreach (NodePort nodePort in _node.Ports.Values)
            {
                Disconnect(nodePort);
            }
        }

        public void AddStackNode(BaseStack _stack)
        {
            stacks[_stack.GUID] = _stack;
        }

        public void RemoveStackNode(BaseStack _stack)
        {
            stacks.Remove(_stack.GUID);
        }

        public void AddGroup(BaseGroup _group)
        {
            groups.Add(_group);
        }

        public void RemoveGroup(BaseGroup _group)
        {
            groups.Remove(_group);
        }
        #endregion
    }
}
