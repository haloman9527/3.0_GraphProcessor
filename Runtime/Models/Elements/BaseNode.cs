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
using CZToolKit.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public abstract class BaseNode : BaseGraphElement
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
            node.position = _position;
            IDAllocation(node);
            return node;
        }

        /// <summary> 给节点分配一个GUID，这将会覆盖已有GUID </summary>
        public static void IDAllocation(BaseNode _node)
        {
            _node.guid = Guid.NewGuid().ToString();
        }
        #endregion

        #region Model
        /// <summary> 唯一标识 </summary>
        [SerializeField] string guid;
        /// <summary> 位置坐标 </summary>
        [SerializeField] Vector2 position;
        /// <summary> 展开状态 </summary>
        [SerializeField] bool expanded = true;
        /// <summary> 锁定状态 </summary>
        [SerializeField] bool locked = false;

        [SerializeField] Dictionary<string, NodePort> ports = new Dictionary<string, NodePort>();
        #endregion

        #region ViewModel
        public event Action<NodePort, NodePort> onPortConnected;
        public event Action<NodePort, NodePort> onPortDisconnected;
        public event Action<NodePort> onPortAdded;
        public event Action<NodePort> onPortRemoved;

        [NonSerialized] BaseGraph owner;
        public BaseGraph Owner
        {
            get { return owner; }
            private set { owner = value; }
        }
        public string GUID { get { return guid; } }
        public Dictionary<string, NodePort> Ports
        {
            get { return ports; }
        }
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }
        public Texture Icon
        {
            get { return GetPropertyValue<Texture>(nameof(Icon)); }
            set { SetPropertyValue(nameof(Icon), value); }
        }
        public Vector2 IconSize
        {
            get { return GetPropertyValue<Vector2>(nameof(IconSize)); }
            set { SetPropertyValue(nameof(IconSize), value); }
        }
        public Color TitleColor
        {
            get { return GetPropertyValue<Color>(nameof(TitleColor)); }
            set { SetPropertyValue(nameof(TitleColor), value); }
        }
        public string Tooltip
        {
            get { return GetPropertyValue<string>(nameof(Tooltip)); }
            set { SetPropertyValue(nameof(Tooltip), value); }
        }
        public bool Locked
        {
            get { return GetPropertyValue<bool>(nameof(Locked)); }
            set { SetPropertyValue(nameof(Locked), value); }
        }
        public Vector2 Position
        {
            get { return GetPropertyValue<Vector2>(nameof(Position)); }
            set { if (!Locked) SetPropertyValue(nameof(Position), value); }
        }
        public bool Expanded
        {
            get { return GetPropertyValue<bool>(nameof(Expanded)); }
            set { SetPropertyValue(nameof(Expanded), value); }
        }

        public virtual void Enable(BaseGraph _graph)
        {
            Owner = _graph;
            foreach (var port in ports.Values)
            {
                port.Enable(this);
            }
        }

        public override void InitializeBindableProperties()
        {
            SetBindableProperty(nameof(Title), new BindableProperty<string>());
            SetBindableProperty(nameof(TitleColor), new BindableProperty<Color>(new Color(0.2f, 0.2f, 0.2f, 0.8f)));
            SetBindableProperty(nameof(Icon), new BindableProperty<Texture>());
            SetBindableProperty(nameof(IconSize), new BindableProperty<Vector2>(new Vector2(20, 20)));
            SetBindableProperty(nameof(Tooltip), new BindableProperty<string>());
            SetBindableProperty(nameof(Locked), new BindableProperty<bool>(locked, v => locked = v));
            SetBindableProperty(nameof(Position), new BindableProperty<Vector2>(position, v => position = v));
            SetBindableProperty(nameof(Expanded), new BindableProperty<bool>(true, v => expanded = v));

            Type type = GetType();

            if (Utility_Attribute.TryGetTypeAttribute(type, out NodeMenuItemAttribute displayName))
            {
                if (displayName.titles != null && displayName.titles.Length != 0)
                    Title = displayName.titles[displayName.titles.Length - 1];
            }
            else
                Title = type.Name;

            if (Utility_Attribute.TryGetTypeAttribute(type, out NodeTitleColorAttribute nodeTitleColor))
                TitleColor = nodeTitleColor.color;

            if (Utility_Attribute.TryGetTypeAttribute(type, out NodeIconAttribute iconAttribute))
            {
                IconSize = new Vector2(iconAttribute.width, iconAttribute.height);
                Icon = Resources.Load<Texture2D>(iconAttribute.iconPath);
            }

            if (Utility_Attribute.TryGetTypeAttribute(type, out NodeTooltipAttribute tooltip))
                Tooltip = tooltip.Tooltip;
        }

        public void UpdateExpanded()
        {
            Expanded = expanded;
        }

        public object GetFieldInfoValue(FieldInfo _fieldInfo)
        {
            return _fieldInfo.GetValue(this);
        }

        public void SetFieldInfoValue(FieldInfo _fieldInfo, object _value)
        {
            _fieldInfo.SetValue(this, _value);
        }

        public List<FieldInfo> GetNodeFieldInfos()
        {
            return Utility_Reflection.GetFieldInfos(GetType());
        }

        #region Overrides
        public virtual void Initialize(IGraphOwner _graphOwner) { }

        public virtual void DrawGizmos(GraphAssetOwner _graphOwner) { }
        #endregion

        #region API
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
            return ports.ContainsKey(_fieldName);
        }

        #endregion

        #region Inputs/Outputs
        /// <summary> 通过字段名获取本地Input接口连接的远程接口的返回值 </summary>
        /// <param name="_fieldName"></param>
        /// <param name="_value"></param>
        /// <param name="_fallback"></param>
        /// <returns></returns>
        public object GetInputValue(string _fieldName)
        {
            if (TryGetInputPort(_fieldName, out NodePort port))
                return port.GetConnectValue();
            return null;
        }

        /// <summary> 通过字段名获取本地Output接口连接的远程接口的返回值 </summary>
        /// <param name="_fieldName"></param>
        /// <param name="_value"></param>
        /// <param name="_fallback"></param>
        /// <returns></returns>
        public object GetOutputValue(string _fieldName)
        {
            if (TryGetOutputPort(_fieldName, out NodePort port))
                return port.GetConnectValue();
            return null;
        }

        /// <summary> 通过字段名获取本地接口连接的远程接口的返回值 </summary>
        /// <param name="_fieldName"></param>
        /// <param name="_value"></param>
        /// <param name="_fallback"></param>
        /// <returns></returns>
        public object GetConnectValue(string _fieldName)
        {
            if (TryGetPort(_fieldName, out NodePort port))
                return port.GetConnectValue();
            return null;
        }

        /// <summary> 通过字段名获取本地接口连接的远程接口的返回值 </summary>
        /// <param name="_fieldName"></param>
        /// <param name="_fallback"></param>
        /// <returns></returns>
        public T GetConnectValue<T>(string _fieldName, T _fallback)
        {
            object v = GetConnectValue(_fieldName);
            if (v != null)
                return (T)v;
            return _fallback;
        }

        public IEnumerable<object> GetConnectValues(string _fieldName)
        {
            if (TryGetPort(_fieldName, out NodePort localPort))
            {
                foreach (var value in localPort.GetConnectValues())
                {
                    yield return value;
                }
            }
        }

        /// <summary> 向本地接口连接的远程接口返回一个值(override) </summary>
        public virtual object GetValue(NodePort _localPort)
        {
            Debug.LogWarning("No GetValue(NodePort port) override defined for " + GetType());
            return null;
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
        public virtual void OnConnected(NodePort _localPort, NodePort _targetPort)
        {

        }

        /// <summary> 在接口断开连接时触发 </summary>
        /// <param name="_localPort"> 本地接口 </param>
        /// <param name="_targetPort"> 目标接口 </param>
        public virtual void OnDisconnected(NodePort _localPort, NodePort _targetPort)
        {

        }

        /// <summary> 清理连接(不通知) </summary>
        public void ClearConnectionsWithoutNotification()
        {
            foreach (var port in ports.Values)
            {
                port.EdgeGUIDs.Clear();
            }
        }

        public void AddPort(NodePort _port)
        {
            if (ports.ContainsKey(_port.FieldName)) return;

            ports[_port.FieldName] = _port;
            onPortAdded?.Invoke(_port);
        }

        public void RemovePort(string _fieldName)
        {
            if (!ports.ContainsKey(_fieldName)) return;

            if (!ports.TryGetValue(_fieldName, out NodePort port))
                return;
            Owner.Disconnect(port);
            ports.Remove(_fieldName);
            onPortRemoved?.Invoke(port);
        }
        #endregion
        #endregion
    }
}
