using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace GraphProcessor
{
    [Serializable]
    public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
    {
        public static readonly Vector2 DefaultBlackboardSize = new Vector2(150, 200);

        [SerializeField]
        public List<JsonElement> serializedNodes = new List<JsonElement>();

        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public Rect blackboardPosition = new Rect(Vector2.zero, DefaultBlackboardSize);
        public bool blackboardoVisible = true;

        [SerializeField]
        NodesDictionary nodes = new NodesDictionary();

        [SerializeField]
        EdgesDictionary edges = new EdgesDictionary();

        [SerializeField]
        List<BaseGroup> groups = new List<BaseGroup>();

        [SerializeField]
        StackNodesDictionary stackNodes = new StackNodesDictionary();

        // GUID和变量的映射表
        [SerializeField]
        ExposedParametersDictionary parametersGUID = new ExposedParametersDictionary();

        // 字段名和GUID的映射表
        [SerializeField, HideInInspector]
        ParamNameGUIDDictionary parametersName = new ParamNameGUIDDictionary();

        public Dictionary<string, BaseNode> Nodes { get { return nodes; } }
        public EdgesDictionary Edges { get { return edges; } }
        public List<BaseGroup> Groups { get { return groups; } }
        public StackNodesDictionary StackNodes { get { return stackNodes; } }

        protected virtual void OnEnable()
        {
            Deserialize();

#if UNITY_EDITOR
            if (!Application.isPlaying)
                Flush();
#endif
        }

        public void Flush()
        {
            // 更新节点端口
            foreach (var node in nodes.Values)
            {
                NodeDataCache.UpdateStaticPorts(node);
            }

            Clean();
        }

        public BaseGraph Clone()
        {
            BaseGraph graph = Instantiate(this);
            return graph;
        }

        #region Operation
        /// <summary> 添加个节点 </summary>
        public void AddNode(BaseNode _node)
        {
            _node.Initialize(this);
            nodes[_node.GUID] = _node;
            NodeDataCache.UpdateStaticPorts(_node);
        }

        /// <summary> 移除指定节点 </summary>
        public void RemoveNode(BaseNode _node)
        {
            // 断开这个节点的所有连接
            Disconnect(_node);
            // 移除这个节点
            nodes.Remove(_node.GUID);
        }

        /// <summary> 连接两个端口 </summary>
        public SerializableEdge Connect(NodePort _inputPort, NodePort _outputPort)
        {
            if (!_inputPort.IsMulti)
                Disconnect(_inputPort);
            if (!_outputPort.IsMulti)
                Disconnect(_outputPort);

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

        public bool ContainsPrameter(string _name)
        {
            return parametersName.ContainsKey(_name);
        }

        public ExposedParameter AddExposedParameter(string _name, Type _valueType, object _value)
        {
            if (ContainsPrameter(_name)) return null;
            ExposedParameter parameter = new ExposedParameter(_name, _valueType) { Value = _value };
            parametersGUID[parameter.GUID] = parameter;
            parametersName[parameter.Name] = parameter.GUID;
            return parameter;
        }

        public ExposedParameter AddExposedParameter(ExposedParameter _param)
        {
            if (ContainsPrameter(_param.Name)) return null;
            parametersGUID[_param.GUID] = _param;
            parametersName[_param.Name] = _param.GUID;
            return _param;
        }

        public void RenameParameter(ExposedParameter _param, string _newName)
        {
            parametersName.Remove(_param.Name);
            _param.Name = _newName;
            parametersName[_param.Name] = _param.GUID;
        }

        public bool RemoveExposedParameter(ExposedParameter _exposedParameter)
        {
            if (nodes.Values.OfType<ParameterNode>().Where(_node => _node.paramGUID == _exposedParameter.GUID).Count() > 0)
            {
                Debug.LogWarning("该参数正被节点引用");
                return false;
            }
            parametersGUID.Remove(_exposedParameter.GUID);
            parametersName.Remove(_exposedParameter.Name);
            return true;
        }

        public ExposedParameter GetExposedParameterFromGUID(string _guid)
        {
            if (parametersGUID.TryGetValue(_guid, out ExposedParameter _param))
                return _param;
            return null;
        }

        public ExposedParameter GetExposedParameterFromName(string _name)
        {
            if (parametersName.TryGetValue(_name, out string _paramGUID))
                return parametersGUID[_paramGUID];
            return null;
        }

        public IEnumerable<ExposedParameter> GetParameters()
        {
            foreach (var item in parametersGUID)
            {
                yield return item.Value;
            }
        }
        #endregion

        public void OnBeforeSerialize()
        {
            serializedNodes.Clear();

            foreach (var node in nodes.Values)
            {
                serializedNodes.Add(JsonSerializer.SerializeNode(node));
            }
        }

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

        public void OnAfterDeserialize() { }

        /// <summary> 清理无用数据 </summary>
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
            }

            foreach (var node in nodes.ToArray())
            {
                // 清理无效节点
                if (node.Value == null)
                {
                    nodes.Remove(node.Key);
                    continue;
                }

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
                group.innerNodeGUIDs.RemoveAll(nodeGUID => !nodes.ContainsKey(nodeGUID));
                group.innerStackGUIDs.RemoveAll(stackGUID => !stackNodes.ContainsKey(stackGUID));
            }

            // 清理Stack中的无效节点
            foreach (var stack in stackNodes.ToArray())
            {
                if (stack.Value == null) stackNodes.Remove(stack.Key);
                stack.Value.nodeGUIDs.RemoveAll(nodeGUID => !nodes.ContainsKey(nodeGUID));
            }
        }
    }
}