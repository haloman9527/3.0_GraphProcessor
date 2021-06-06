using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public abstract class BaseNode : IBaseNode
    {
        #region 静态
        /// <summary> 根据T创建一个节点，并设置位置 </summary>
        public static T CreateNew<T>(Vector2 _position) where T : BaseNode
        {
            return CreateNew(typeof(T), _position) as T;
        }

        /// <summary> 根据_type创建一个节点，并设置位置 </summary>
        public static BaseNode CreateNew(Type _type, Vector2 _position)
        {
            if (!_type.IsSubclassOf(typeof(BaseNode)))
                return null;
            var node = Activator.CreateInstance(_type) as BaseNode;
            node.position = new Rect(_position, new Vector2(100, 100));
            IDAllocation(node);
            node.OnCreated();
            return node;
        }

        /// <summary> 给节点分配一个GUID，这将会覆盖已有GUID </summary>
        public static void IDAllocation(BaseNode _node)
        {
            _node.guid = Guid.NewGuid().ToString();
        }
        #endregion

        [NonSerialized]
        IBaseGraph owner;

        /// <summary> 唯一标识 </summary>
        [SerializeField, HideInInspector]
        string guid;
        /// <summary> 位置坐标 </summary>
        [SerializeField, HideInInspector]
        public Rect position;
        /// <summary> 展开状态 </summary>
        [SerializeField, HideInInspector]
        bool expanded = true;
        /// <summary> 锁定状态 </summary>
        [SerializeField, HideInInspector]
        bool locked = false;
        [SerializeField, HideInInspector]
        Dictionary<string, NodePort> ports = new Dictionary<string, NodePort>();

        /// <summary> 节点对象的所有者，即图 </summary>
        public IBaseGraph Owner { get { return owner; } }
        public string GUID { get { return guid; } }
        public bool Expanded { get { return expanded; } set { expanded = value; } }
        public bool Locked { get { return locked; } set { locked = value; } }
        public Dictionary<string, NodePort> Ports { get { return ports; } }

        protected BaseNode() { }

        /// <summary> 在节点被创建出来后调用，调用优先级最高 </summary>
        public virtual void OnCreated() { }

        /// <summary> 请不要在其它任何地方调用 </summary>
        public void Enable(IBaseGraph _graph)
        {
            owner = _graph;
            foreach (var port in Ports.Values)
            {
                port.Owner = this;
            }
        }

        public virtual void OnEnabled() { }

        public virtual void Initialize(IGraphOwner _graphOwner) { }

        #region Ports
        /// <summary> 通过字段名获取一个本地Input接口 </summary>
        public bool TryGetInputPort(string _fieldName, out NodePort _localNodePort)
        {
            if (TryGetPort(_fieldName, out _localNodePort) && _localNodePort.Direction == PortDirection.Input)
                return true;
            _localNodePort = null;
            return false;
        }

        /// <summary> 通过字段名获取一个本地Output接口 </summary>
        public bool TryGetOutputPort(string _fieldName, out NodePort _localNodePort)
        {
            if (TryGetPort(_fieldName, out _localNodePort) && _localNodePort.Direction == PortDirection.Output)
                return true;
            _localNodePort = null;
            return false;
        }

        /// <summary> 通过字段名获取一个本地接口 </summary>
        public bool TryGetPort(string _fieldName, out NodePort _localNodePort)
        {
            if (Ports.TryGetValue(_fieldName, out _localNodePort)) return true;
            else return false;
        }

        /// <summary> 本地接口是否存在 </summary>
        public bool HasPort(string _fieldName)
        {
            return Ports.ContainsKey(_fieldName);
        }

        #endregion

        #region Inputs/Outputs
        /// <summary> 通过字段名获取本地Input接口连接的远程接口的返回值 </summary>
        /// <typeparam name="T"> 目标返回值类型 </typeparam>
        /// <param name="_fieldName"></param>
        /// <param name="_value"></param>
        /// <param name="_fallback"></param>
        /// <returns></returns>
        public bool TryGetInputValue<T>(string _fieldName, out T _value, T _fallback = default)
        {
            _value = _fallback;
            if (TryGetInputPort(_fieldName, out NodePort port))
                return port.TryGetConnectValue(ref _value);
            return false;
        }

        /// <summary> 通过字段名获取本地Output接口连接的远程接口的返回值 </summary>
        /// <typeparam name="T"> 目标返回值类型 </typeparam>
        /// <param name="_fieldName"></param>
        /// <param name="_value"></param>
        /// <param name="_fallback"></param>
        /// <returns></returns>
        public bool TryGetOutputValue<T>(string _fieldName, out T _value, T _fallback = default)
        {
            _value = _fallback;
            if (TryGetOutputPort(_fieldName, out NodePort port))
                return port.TryGetConnectValue(ref _value);
            return false;
        }

        /// <summary> 通过字段名获取本地接口连接的远程接口的返回值 </summary>
        /// <typeparam name="T"> 目标返回值类型 </typeparam>
        /// <param name="_fieldName"></param>
        /// <param name="_value"></param>
        /// <param name="_fallback"></param>
        /// <returns></returns>
        public bool TryGetConnectValue<T>(string _fieldName, out T _value, T _fallback = default)
        {
            _value = _fallback;
            if (TryGetPort(_fieldName, out NodePort port))
                return port.TryGetConnectValue(ref _value);
            return false;
        }

        public IEnumerable<T> GetConnectValues<T>(string _fieldName)
        {
            if (TryGetPort(_fieldName, out NodePort localPort))
            {
                foreach (var value in localPort.GetConnectValues<T>())
                {
                    yield return value;
                }
            }
        }

        /// <summary> 向本地接口连接的远程接口返回一个值(override) </summary>
        public virtual bool GetValue<T>(NodePort _localPort, ref T _value)
        {
            Debug.LogWarning("No GetValue(NodePort port) override defined for " + GetType());
            return false;
        }

        /// <summary> 执行节点逻辑，指定接口和参数(可判断接口执行相应逻辑) </summary>
        public virtual void Execute(NodePort _localPort, params object[] _params) { }

        /// <summary> 通过字段名执行本地接口连接的远程接口的<see cref="Execute(NodePort, object[])"/>方法 </summary>
        public void ExecuteConnections(string _localPortName, params object[] _params)
        {
            if (TryGetPort(_localPortName, out NodePort port))
                ExecuteConnections(port, _params);
        }

        /// <summary> 执行本地接口连接的远程接口的<see cref="Execute(NodePort, object[])"/>方法 </summary>
        public void ExecuteConnections(NodePort _localPort, params object[] _params)
        {
            foreach (var targetPort in _localPort.GetConnections())
            {
                targetPort.Execute(_params);
            }
        }

        #endregion
        /// <summary> 在接口连接时触发 </summary>
        /// <param name="_localPort"> 本地接口 </param>
        /// <param name="_targetPort"> 目标接口 </param>
        public virtual void OnConnected(NodePort _localPort, NodePort _targetPort) { }

        /// <summary> 在接口断开连接时触发 </summary>
        /// <param name="_localPort"> 本地接口 </param>
        /// <param name="_targetPort"> 目标接口 </param>
        public virtual void OnDisconnected(NodePort _localPort, NodePort _targetPort) { }

        /// <summary> 动态返回接口类型 </summary>
        public virtual Type PortDynamicType(string _localPortName) { return null; }

        /// <summary> 清理连接(不通知) </summary>
        public void ClearConnectionsWithoutNotification()
        {
            foreach (var port in Ports.Values)
            {
                port.EdgeGUIDS.Clear();
            }
        }

        public virtual void DrawGizmos(GraphAssetOwner _graphOwner) { }
    }
}
