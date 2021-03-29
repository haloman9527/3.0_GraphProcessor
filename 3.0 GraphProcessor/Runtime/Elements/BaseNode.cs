using UnityEngine;
using System;

namespace GraphProcessor
{
    [Serializable]
    public abstract class BaseNode
    {
        #region 静态
        /// <summary> Creates a node of type T at a certain position </summary>
        public static T CreateNew<T>(Vector2 position) where T : BaseNode
        {
            return CreateNew(typeof(T), position) as T;
        }

        /// <summary> Creates a node of type nodeType at a certain position </summary>
        public static BaseNode CreateNew(Type nodeType, Vector2 position)
        {
            if (!nodeType.IsSubclassOf(typeof(BaseNode)))
                return null;
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            node.position = new Rect(position, new Vector2(100, 100));
            node.OnCreated();

            return node;
        }
        #endregion

        BaseGraph owner;

        /// <summary> 唯一标识 </summary>
        [SerializeField, HideInInspector] string guid;
        /// <summary> 位置坐标 </summary>
        public Rect position;
        /// <summary> 展开状态 </summary>
        [SerializeField] bool expanded = true;
        /// <summary> 锁定状态 </summary>
        [SerializeField] bool locked = false;
        [SerializeField] NodePortsDictionary ports = new NodePortsDictionary();

        public BaseGraph Owner { get { return owner; } }
        public string GUID { get { return guid; } }
        public bool Expanded { get { return expanded; } set { expanded = value; } }
        public bool Locked { get { return locked; } set { locked = value; } }
        public NodePortsDictionary Ports { get { return ports; } }

        /// <summary> 创建时调用，请不要在其它任何地方调用，因为这会重置GUID </summary>
        public void Initialize(BaseGraph _graph)
        {
            owner = _graph;
            foreach (var port in ports.Values)
            {
                port.Owner = this;
            }
        }

        /// <summary> 仅在新增加时调用 </summary>
        public virtual void OnCreated()
        {
            guid = Guid.NewGuid().ToString();
            Ports.Clear();
        }

        #region Ports
        /// <summary> 通过名字获取一个Input接口 </summary>
        public bool TryGetInputPort(string _fieldName, out NodePort _nodePort)
        {
            if (TryGetPort(_fieldName, out _nodePort) && _nodePort.Direction == PortDirection.Input)
                return true;
            _nodePort = null;
            return false;
        }

        /// <summary> 通过名字获取一个Output接口 </summary>
        public bool TryGetOutputPort(string _fieldName, out NodePort _nodePort)
        {
            if (TryGetPort(_fieldName, out _nodePort) && _nodePort.Direction == PortDirection.Output)
                return true;
            _nodePort = null;
            return false;
        }

        /// <summary> 通过名字获取一个接口 </summary>
        public bool TryGetPort(string _fieldName, out NodePort _nodePort)
        {
            if (ports.TryGetValue(_fieldName, out _nodePort)) return true;
            else return false;
        }

        /// <summary> 接口是否存在 </summary>
        public bool HasPort(string _fieldName)
        {
            return ports.ContainsKey(_fieldName);
        }

        #endregion

        #region Inputs/Outputs
        public bool TryGetInputValue<T>(string _fieldName, out T _value, T _fallback = default(T))
        {
            _value = _fallback;
            if (TryGetInputPort(_fieldName, out NodePort port))
                return port.TryGetConnectValue(ref _value);
            return false;
        }

        public bool TryGetOutputValue<T>(string _fieldName, out T _value, T _fallback = default(T))
        {
            _value = _fallback;
            if (TryGetOutputPort(_fieldName, out NodePort port))
                return port.TryGetConnectValue(ref _value);
            return false;
        }

        public bool TryGetConnectValue<T>(string _fieldName, out T _value, T _fallback = default(T))
        {
            _value = _fallback;
            if (TryGetPort(_fieldName, out NodePort port))
                return port.TryGetConnectValue(ref _value);
            return false;
        }

        /// <summary> 通过input或output接口返回相应的值(此方法从外部调用，不在内部使用，仅重写) </summary>
        public virtual bool GetValue<T>(NodePort _port, ref T _value)
        {
            Debug.LogWarning("No GetValue(NodePort port) override defined for " + GetType());
            return false;
        }

        /// <summary> 从外部调用 </summary>
        public virtual void Execute(NodePort _port, params object[] _params) { }

        /// <summary> 调用端口连接的Execute方法 </summary>
        public void ExecuteConnections(string _portName, params object[] _params)
        {
            if (TryGetPort(_portName, out NodePort port))
            {
                foreach (var targetPort in port.GetConnections())
                {
                    targetPort.Execute(_params);
                }
            }
        }
        #endregion

        public virtual void OnConnected(NodePort _port, NodePort _targetPort) { }

        public virtual void OnDisconnected(NodePort _port, NodePort _targetPort) { }
    }
}
