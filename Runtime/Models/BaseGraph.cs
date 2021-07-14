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
    public class BaseGraph : BaseGraphElement
    {
        public static readonly Vector2 DefaultBlackboardSize = new Vector2(150, 200);

        #region Model
        [SerializeField] Vector3 position = Vector3.zero;
        [SerializeField] Vector3 scale = Vector3.one;
        [SerializeField] bool blackboardVisible = true;
        [SerializeField] Rect blackboardPosition = new Rect(Vector2.zero, DefaultBlackboardSize);

        [SerializeField] Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [SerializeField] Dictionary<string, BaseEdge> edges = new Dictionary<string, BaseEdge>();
        //[SerializeField] Dictionary<string, StackPanel> stacks = new Dictionary<string, StackPanel>();
        [SerializeField] List<GroupPanel> groups = new List<GroupPanel>();

        [SerializeField] CZBlackboard blackboard = new CZBlackboard();
        #endregion

        #region ViewModel
        #region 字段
        public event Action<BaseNode> onNodeAdded;
        public event Action<BaseNode> onNodeRemoved;

        public event Action<BaseEdge> onEdgeAdded;
        public event Action<BaseEdge> onEdgeRemoved;

        //public event Action<StackPanel> onStackAdded;
        //public event Action<StackPanel> onStackRemoved;

        public event Action<GroupPanel> onGroupAdded;
        public event Action<GroupPanel> onGroupRemoved;

        public event Action<string, ICZType> onBlackboardDataAdded;
        public event Action<string> onBlackboardDataRemoved;
        public event Action<string, string> onBlackboardDataRenamed;

        [NonSerialized] public List<SharedVariable> variables = new List<SharedVariable>();
        #endregion

        #region 属性
        public Vector3 Position
        {
            get { return GetPropertyValue<Vector3>(nameof(Position)); }
            set { SetPropertyValue(nameof(Position), value); }
        }
        public Vector3 Scale
        {
            get { return GetPropertyValue<Vector3>(nameof(Scale)); }
            set { SetPropertyValue(nameof(Scale), value); }
        }
        public bool BlackboardVisible
        {
            get { return GetPropertyValue<bool>(nameof(BlackboardVisible)); }
            set { SetPropertyValue(nameof(BlackboardVisible), value); }
        }
        public Rect BlackboardPosition
        {
            get { return GetPropertyValue<Rect>(nameof(BlackboardPosition)); }
            set { SetPropertyValue(nameof(BlackboardPosition), value); }
        }
        public IReadOnlyCZBlackboard Blackboard { get { return blackboard; } }
        public IReadOnlyDictionary<string, BaseNode> Nodes { get { return nodes; } }
        public IReadOnlyDictionary<string, BaseEdge> Edges { get { return edges; } }
        //public IReadOnlyDictionary<string, StackPanel> Stacks { get { return stacks; } }
        public IReadOnlyList<GroupPanel> Groups { get { return groups; } }
        public IVariableOwner VarialbeOwner { get; private set; }
        public IReadOnlyList<SharedVariable> Variables
        {
            get
            {
                if (variables == null) CollectionVariables();
                return variables;
            }
        }
        #endregion

        public void Enable()
        {
            foreach (var node in nodes.Values)
            {
                node.Enable(this);
            }
            foreach (var edge in edges.Values)
            {
                edge.Enable(this);
            }
        }

        public override void InitializeBindableProperties()
        {
            SetBindableProperty(nameof(Position), new BindableProperty<Vector3>(position, v => { position = v; }));

            SetBindableProperty(nameof(Scale), new BindableProperty<Vector3>(scale, v => scale = v));
            SetBindableProperty(nameof(BlackboardVisible), new BindableProperty<bool>(blackboardVisible, v => blackboardVisible = v));
            SetBindableProperty(nameof(BlackboardPosition), new BindableProperty<Rect>(blackboardPosition, v => blackboardPosition = v));
        }

        #region API
        public virtual void Initialize(IGraphOwner _graphOwner)
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
            if (variables == null)
                CollectionVariables();
            VarialbeOwner = _variableOwner;
            foreach (var variable in variables)
            {
                variable.InitializePropertyMapping(VarialbeOwner);
            }
        }

        public void AddParameterNode(string _dataName, Vector2 _position)
        {
            ParameterNode parameterNode = BaseNode.CreateNew<ParameterNode>(_position);
            parameterNode.Name = _dataName;
            foreach (var port in parameterNode.Ports.Values)
            {
                if (TryGetData_BB(_dataName, out ICZType data))
                    port.typeQualifiedName = data.ValueType.AssemblyQualifiedName;
            }
            AddNode(parameterNode);
        }

        public void AddRelayNode(BaseEdge _edge, Vector2 _position)
        {
            NodePort inputPort = _edge.InputPort;
            NodePort outputPort = _edge.OutputPort;
            DisconnectEdge(_edge.GUID);

            var relayNode = BaseNode.CreateNew<RelayNode>(_position);
            BaseNode node = AddNode(relayNode);
            Connect(node.Ports[nameof(RelayNode.input)], outputPort);
            Connect(inputPort, node.Ports[nameof(RelayNode.output)]);
        }

        public void RemoveRelayNode(RelayNode _relayNode)
        {
            NodePort inputPort = _relayNode.Ports[nameof(RelayNode.output)].Connection;
            NodePort outputPort = _relayNode.Ports[nameof(RelayNode.input)].Connection;

            RemoveNode(_relayNode);
            if (inputPort != null && outputPort != null)
                Connect(inputPort, outputPort);
        }

        public BaseNode AddNode(BaseNode _node)
        {
            if (_node == null) return null;
            GraphProcessorCache.UpdateStaticPorts(_node);
            _node.Enable(this);
            nodes[_node.GUID] = _node;
            if (variables == null)
                CollectionVariables();
            IEnumerable<SharedVariable> nodeVariables = SharedVariableUtility.CollectionObjectSharedVariables(_node);
            variables.AddRange(nodeVariables);
            if (VarialbeOwner != null)
            {
                foreach (var variable in nodeVariables)
                {
                    variable.InitializePropertyMapping(VarialbeOwner);
                }
            }
            onNodeAdded?.Invoke(_node);
            return _node;
        }

        public void RemoveNode(BaseNode _node)
        {
            if (_node == null) return;
            // 断开这个节点的所有连接,移除节点
            Disconnect(_node);
            BaseNode node = Nodes[_node.GUID];
            nodes.Remove(_node.GUID);
            onNodeRemoved?.Invoke(node);
        }

        public BaseEdge Connect(NodePort _inputPort, NodePort _outputPort)
        {
            // 在连接两个端口，如果端口设置为只能连接一个端口，则需要在连接前把其他所有连接断开
            if (!_inputPort.Multiple)
                Disconnect(_inputPort);
            if (!_outputPort.Multiple)
                Disconnect(_outputPort);

            // 创建一条连线
            BaseEdge edge = BaseEdge.CreateNewEdge(_inputPort, _outputPort);
            edge.Enable(this);
            edges[edge.GUID] = edge;

            _inputPort.ConnectToEdge(edge);
            _outputPort.ConnectToEdge(edge);

            onEdgeAdded?.Invoke(edge);

            _inputPort.Owner.OnConnected(_inputPort, _outputPort);
            _outputPort.Owner.OnConnected(_outputPort, _inputPort);
            return edge;
        }

        public void DisconnectEdge(string _edgeGUID)
        {
            if (!Edges.TryGetValue(_edgeGUID, out BaseEdge edge))
                return;

            if (edge == null)
            {
                edges.Remove(_edgeGUID);
                return;
            }

            edge.InputPort.DisconnectToEdge(edge);
            edge.OutputPort.DisconnectToEdge(edge);
            edge.InputNode.OnDisconnected(edge.InputPort, edge.OutputPort);
            edge.OutputNode.OnDisconnected(edge.OutputPort, edge.InputPort);

            edges.Remove(_edgeGUID);
            onEdgeRemoved?.Invoke(edge);
        }

        public void Disconnect(NodePort _nodePort)
        {
            for (int i = 0; i < _nodePort.EdgeGUIDs.Count; i++)
            {
                DisconnectEdge(_nodePort.EdgeGUIDs[i]);
            }
        }

        public void Disconnect(BaseNode _node)
        {
            foreach (NodePort nodePort in _node.Ports.Values)
            {
                Disconnect(nodePort);
            }
        }

        //public void AddStackNode(StackPanel _stack)
        //{
        //    _stack.Enable(this);
        //    stacks[_stack.GUID] = _stack;
        //    onStackAdded?.Invoke(_stack);
        //}

        //public void RemoveStackNode(StackPanel _stack)
        //{
        //    stacks.Remove(_stack.GUID);
        //    onStackRemoved?.Invoke(_stack);
        //}

        public void AddGroup(GroupPanel _group)
        {
            _group.Enable(this);
            groups.Add(_group);
            onGroupAdded?.Invoke(_group);
        }

        public void AddGroup(string _title, Vector2 _position)
        {
            GroupPanel group = GroupPanel.Create(_title, _position);
            AddGroup(group);
        }

        public void RemoveGroup(GroupPanel _group)
        {
            if (groups.Remove(_group))
                onGroupRemoved?.Invoke(_group);
        }
        #endregion

        #region Blackboard
        public bool ContainsName_BB(string _name)
        {
            return blackboard.ContainsName(_name);
        }

        public bool ContainsGUID_BB(string _guid)
        {
            return blackboard.ContainsGUID(_guid);
        }

        public bool AddData_BB(string _name, ICZType _data)
        {
            if (blackboard.SetData(_name, _data))
            {
                onBlackboardDataAdded?.Invoke(_name, _data);
                return true;
            }
            return false;
        }

        public bool TryGetData_BB(string _name, out ICZType _data)
        {
            return blackboard.TryGetData(_name, out _data);
        }

        public bool TryGetValue_BB<T>(string _name, out T _value)
        {
            return blackboard.TryGetValue(_name, out _value);
        }

        public bool RemoveData_BB(string _name)
        {
            foreach (var parameterNode in Nodes.Values.OfType<ParameterNode>())
            {
                if (parameterNode.Name == _name)
                {
                    Debug.LogWarning("此参数正被节点引用");
                    return false;
                }
            }
            if (blackboard.RemoveData(_name))
            {
                onBlackboardDataRemoved?.Invoke(_name);
                return true;
            }
            return false;
        }

        public void SetValue_BB<T>(string _name, T _value)
        {
            blackboard.SetValue(_name, _value);
        }

        public bool RenameData_BB(string _oldName, string _newName)
        {
            if (blackboard.Rename(_oldName, _newName))
            {
                foreach (var parameterNode in Nodes.Values.OfType<ParameterNode>())
                {
                    if (parameterNode.Name == _oldName)
                        parameterNode.Name = _newName;
                }
                onBlackboardDataRenamed?.Invoke(_oldName, _newName);
                return true;
            }
            return false;
        }
        #endregion
        #endregion
    }
}
