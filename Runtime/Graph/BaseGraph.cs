using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using CZToolKit.Core.Blackboards;

namespace GraphProcessor
{
    /// <summary> 若项目中安装了Odin，则继承Odin的SO基类 </summary>
    [Serializable]
#if ODIN_INSPECTOR
    public class BaseGraph : Sirenix.OdinInspector.SerializedScriptableObject
#else
    public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
#endif
    {
        /// <summary> 黑板的默认大小 </summary>
        public static readonly Vector2 DefaultBlackboardSize = new Vector2(150, 200);

        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public Rect blackboardPosition = new Rect(Vector2.zero, DefaultBlackboardSize);
        public bool blackboardoVisible = true;

        //SerializeField] NodesDictionary nodes = new NodesDictionary();
        [SerializeField] Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();

        [SerializeField]
        EdgesDictionary edges = new EdgesDictionary();

        [SerializeField]
        List<BaseGroup> groups = new List<BaseGroup>();

        [SerializeField]
        StackNodesDictionary stackNodes = new StackNodesDictionary();

        [SerializeField]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.HideLabel]
#endif
        BlackboardWithGUID blackboard = new BlackboardWithGUID();

        public Dictionary<string, BaseNode> Nodes { get { return nodes; } }
        public Dictionary<string, SerializableEdge> Edges { get { return edges; } }
        public List<BaseGroup> Groups { get { return groups; } }
        public Dictionary<string, BaseStackNode> StackNodes { get { return stackNodes; } }
        public BlackboardWithGUID Blackboard { get { return blackboard; } }

#if !ODIN_INSPECTOR
        [SerializeField]
        public List<JsonElement> serializedNodes = new List<JsonElement>();


        public virtual void OnBeforeSerialize()
        {
            serializedNodes.Clear();

            foreach (var node in nodes.Values)
            {
                serializedNodes.Add(JsonSerializer.SerializeNode(node));
            }
        }

        public virtual void OnAfterDeserialize() { }

        public void Deserialize()
        {
            nodes.Clear();

            foreach (var serializedNode in serializedNodes.ToList())
            {
                var node = JsonSerializer.DeserializeNode(serializedNode) as BaseNode;
                if (node == null)
                {
                    serializedNodes.Remove(serializedNode);
                    continue;
                }
                AddNode(node);
            }
        }
#endif

        protected virtual void OnEnable()
        {
#if !ODIN_INSPECTOR
            Deserialize();
#endif

#if UNITY_EDITOR
            Flush();
#endif
        }

        /// <summary> 刷新及修复数据 </summary>
        public void Flush()
        {
            // 更新节点端口
            foreach (var node in Nodes.Values)
            {
                NodeDataCache.UpdateStaticPorts(node);
            }

            Clean();
        }

        /// <summary> 克隆 </summary>
        public BaseGraph Clone()
        {
            BaseGraph graph = Instantiate(this);
            graph.name = name;
            foreach (var node in graph.Nodes.Values)
            {
                node.Initialize(graph);
            }
            return graph;
        }

        #region Operation
        /// <summary> 根据类型添加一个节点 </summary>
        public T AddNode<T>(Vector2 _nodePosition) where T : BaseNode
        {
            T node = BaseNode.CreateNew<T>(_nodePosition);
            AddNode(node);
            return node;
        }

        /// <summary> 添加节点 </summary>
        public void AddNode(BaseNode _node)
        {
            if (_node == null) return;
            _node.Initialize(this);
            Nodes[_node.GUID] = _node;
            NodeDataCache.UpdateStaticPorts(_node);
        }

        /// <summary> 移除指定节点 </summary>
        public void RemoveNode(BaseNode _node)
        {
            if (_node == null) return;
            // 断开这个节点的所有连接
            Disconnect(_node);
            // 移除节点
            Nodes.Remove(_node.GUID);
        }

        /// <summary> 连接两个端口 </summary>
        public SerializableEdge Connect(NodePort _inputPort, NodePort _outputPort)
        {
            // 在连接两个端口是，如果端口设置为只能连接一个端口，则需要在连接前把其他所有连接断开
            if (!_inputPort.IsMulti)
                Disconnect(_inputPort);
            if (!_outputPort.IsMulti)
                Disconnect(_outputPort);

            // 创建一条连线
            SerializableEdge edge = SerializableEdge.CreateNewEdge(this, _inputPort, _outputPort);
            Edges[edge.GUID] = edge;

            _inputPort.ConnectEdge(edge);
            _outputPort.ConnectEdge(edge);

            _inputPort.Owner.OnConnected(_inputPort, _outputPort);
            _outputPort.Owner.OnConnected(_outputPort, _inputPort);

            return edge;
        }

        /// <summary> 断开指定连接 </summary>
        public void Disconnect(SerializableEdge _edge)
        {
            if (_edge == null) return;
            _edge.InputPort?.DisconnectEdge(_edge);
            _edge.OutputPort?.DisconnectEdge(_edge);

            _edge.InputNode?.OnDisconnected(_edge.InputPort, _edge.OutputPort);
            _edge.OutputNode?.OnDisconnected(_edge.OutputPort, _edge.InputPort);
            Edges.Remove(_edge.GUID);
        }

        /// <summary> 根据连接的GUID断开连接 </summary>
        public void Disconnect(string _edgeGUID)
        {
            if (Edges.TryGetValue(_edgeGUID, out SerializableEdge edge) && edge == null)
            {
                edges.Remove(_edgeGUID);
                return;
            }
            Disconnect(edge);
        }

        /// <summary> 断开指定端口的所有连接 </summary>
        public void Disconnect(NodePort _nodePort)
        {
            while (_nodePort.IsConnected)
            {
                Disconnect(_nodePort.GetEdge(0));
            }
        }

        /// <summary> 断开指定节点的所有连接 </summary>
        public void Disconnect(BaseNode _node)
        {
            foreach (NodePort nodePort in _node.Ports.Values)
            {
                Disconnect(nodePort);
            }
        }

        /// <summary> 添加一个Group </summary>
        public void AddGroup(BaseGroup _group)
        {
            groups.Add(_group);
        }

        /// <summary> 移除一个Group </summary>
        public void RemoveGroup(BaseGroup _group)
        {
            groups.Remove(_group);
        }

        /// <summary> 添加一个栈 </summary>
        public void AddStackNode(BaseStackNode _stackNode)
        {
            stackNodes[_stackNode.GUID] = _stackNode;
        }

        /// <summary> 移除一个栈 </summary>
        public void RemoveStackNode(BaseStackNode _stackNode)
        {
            stackNodes.Remove(_stackNode.GUID);
        }

        public bool RemoveExposedParameter(IBlackboardPropertyGUID _exposedParameter)
        {
            if (Nodes.Values.OfType<ParameterNode>().Where(_node => _node.paramGUID == _exposedParameter.GUID).Count() > 0)
            {
                Debug.LogWarning("该参数正被节点引用");
                return false;
            }
            Blackboard.RemoveData(_exposedParameter.Name);
            return true;
        }
        #endregion

        /// <summary> 清理和修复 </summary>
        public void Clean()
        {
            // 清理无效连接
            foreach (var edge in edges.ToArray())
            {
                // 如果线段为空
                if (edge.Value == null)
                {
                    Disconnect(edge.Key);
                    continue;
                }

                if (edge.Value.InputNode == null || edge.Value.OutputNode == null || edge.Value.InputNode == edge.Value.OutputNode)
                {
                    Disconnect(edge.Key);
                    edge.Value.InputPort?.DisconnectEdge(edge.Value);
                    edge.Value.OutputPort?.DisconnectEdge(edge.Value);
                    continue;
                }

                if (edge.Value.InputPort == null || edge.Value.OutputPort == null || edge.Value.InputPort.Direction == edge.Value.OutputPort.Direction)
                {
                    Disconnect(edge.Key);
                    edge.Value.InputPort?.DisconnectEdge(edge.Value);
                    edge.Value.OutputPort?.DisconnectEdge(edge.Value);
                    continue;
                }

                if (!edge.Value.InputPort.EdgeGUIDS.Contains(edge.Key) || !edge.Value.OutputPort.EdgeGUIDS.Contains(edge.Key))
                {
                    Disconnect(edge.Key);
                    edge.Value.InputPort.DisconnectEdge(edge.Value);
                    edge.Value.OutputPort.DisconnectEdge(edge.Value);
                    continue;
                }
            }

            foreach (var node in Nodes.ToArray())
            {
                // 清理无效节点
                if (node.Value == null)
                {
                    Nodes.Remove(node.Key);
                    continue;
                }
                // 修复节点
                node.Value.Initialize(this);

                // 清理节点无效连接
                foreach (var nodePort in node.Value.Ports.ToArray())
                {
                    if (nodePort.Value == null)
                    {
                        node.Value.Ports.Remove(nodePort.Key);
                        continue;
                    }
                    nodePort.Value.EdgeGUIDS.RemoveAll(edgeGUID => !edges.ContainsKey(edgeGUID));
                }
            }

            // 清理Group中的无效节点
            foreach (var group in groups.ToArray())
            {
                if (group == null)
                {
                    groups.Remove(group);
                    continue;
                }
                group.innerNodeGUIDs.RemoveAll(nodeGUID => !Nodes.ContainsKey(nodeGUID));
                group.innerStackGUIDs.RemoveAll(stackGUID => !stackNodes.ContainsKey(stackGUID));
            }

            // 清理Stack中的无效节点
            foreach (var stack in stackNodes.ToArray())
            {
                if (stack.Value == null)
                {
                    stackNodes.Remove(stack.Key);
                    continue;
                }
                stack.Value.nodeGUIDs.RemoveAll(nodeGUID => !Nodes.ContainsKey(nodeGUID));
            }

            Blackboard.Clean();
        }
    }
}