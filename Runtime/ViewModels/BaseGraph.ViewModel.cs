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
using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public abstract partial class BaseGraph : IntegratedViewModel
    {
        public const string PAN_NAME = nameof(pan);
        public const string ZOOM_NAME = nameof(zoom);

        #region 字段
        public event Action<BaseNode> onNodeAdded;
        public event Action<BaseNode> onNodeRemoved;

        public event Action<BaseConnection> onConnected;
        public event Action<BaseConnection> onDisconnected;

        [NonSerialized] public List<SharedVariable> variables = new List<SharedVariable>();
        #endregion

        #region 属性
        public Vector3 Pan
        {
            get { return GetPropertyValue<Vector3>(PAN_NAME); }
            set { SetPropertyValue(PAN_NAME, value); }
        }
        public Vector3 Zoom
        {
            get { return GetPropertyValue<Vector3>(ZOOM_NAME); }
            set { SetPropertyValue(ZOOM_NAME, value); }
        }
        public IReadOnlyDictionary<string, BaseNode> Nodes { get { return nodes; } }
        public IReadOnlyList<BaseConnection> Connections { get { return connections; } }
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

        #region API
        public void Enable()
        {
            foreach (var pair in nodes)
            {
                pair.Value.Enable(this);
            }
            foreach (var connection in connections)
            {
                connection.Enable(this);
                connection.FromNode.Ports.TryGetValue(connection.FromPortName, out var fromPort);
                connection.ToNode.Ports.TryGetValue(connection.ToPortName, out var toPort);
                fromPort.connections.Add(connection);
                toPort.connections.Add(connection);
            }
        }

        public virtual void Initialize(IGraphOwner graphOwner)
        {
            InitializePropertyMapping(graphOwner);
            foreach (var pair in nodes)
            {
                pair.Value.Initialize(graphOwner);
            }
        }

        protected override void BindProperties()
        {
            this[PAN_NAME] = new BindableProperty<Vector3>(pan, v => pan = v);
            this[ZOOM_NAME] = new BindableProperty<Vector3>(zoom, v => zoom = v);
        }

        public void InitializePropertyMapping(IVariableOwner variableOwner)
        {
            if (variables == null)
                CollectionVariables();
            VarialbeOwner = variableOwner;
            foreach (var variable in variables)
            {
                variable.InitializePropertyMapping(VarialbeOwner);
            }
        }

        void CollectionVariables()
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

        public string GenerateNodeGUID()
        {
            while (true)
            {
                string guid = Guid.NewGuid().ToString();
                if (!nodes.ContainsKey(guid)) return guid;
            }
        }

        public void AddNode(BaseNode node)
        {
            if (node.ContainsKey(node.GUID))
                return;
            node.Enable(this);
            nodes[node.GUID] = node;
            if (variables == null)
                CollectionVariables();
            IEnumerable<SharedVariable> nodeVariables = SharedVariableUtility.CollectionObjectSharedVariables(node);
            variables.AddRange(nodeVariables);
            if (VarialbeOwner != null)
            {
                foreach (var variable in nodeVariables)
                {
                    variable.InitializePropertyMapping(VarialbeOwner);
                }
            }
            onNodeAdded?.Invoke(node);
        }

        public void RemoveNode(BaseNode node)
        {
            if (node == null) return;
            Disconnect(node);
            nodes.Remove(node.GUID);
            onNodeRemoved?.Invoke(node);
        }

        public void Connect(BaseConnection connection)
        {
            BaseConnection tempConnection = connections.Find(item =>
            item.FromNodeGUID == connection.FromNodeGUID
            && item.FromPortName == connection.FromPortName
            && item.ToNodeGUID == connection.ToNodeGUID
            && item.ToPortName == connection.ToPortName
            );
            if (tempConnection != null)
                return;

            connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePort fromPort);
            if (fromPort == null) return;
            if (fromPort.capacity == BasePort.Capacity.Single)
                Disconnect(connection.FromNode, fromPort);

            connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePort toPort);
            if (toPort == null) return;
            if (toPort.capacity == BasePort.Capacity.Single)
                Disconnect(connection.ToNode, toPort);

            connection.Enable(this);
            connections.Add(connection);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            onConnected?.Invoke(connection);
        }

        public BaseConnection Connect(BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            BaseConnection connection = connections.Find(edge => edge.FromNodeGUID == from.GUID && edge.FromPortName == fromPortName && edge.ToNodeGUID == to.GUID && edge.ToPortName == toPortName);
            if (connection != null)
                return connection;

            from.Ports.TryGetValue(fromPortName, out BasePort fromPort);
            if (fromPort.capacity == BasePort.Capacity.Single)
                Disconnect(from, fromPort);

            to.Ports.TryGetValue(toPortName, out BasePort toPort);
            if (toPort.capacity == BasePort.Capacity.Single)
                Disconnect(to, toPort);

            connection = NewConnection(from, fromPortName, to, toPortName);
            connection.Enable(this);
            connections.Add(connection);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            onConnected?.Invoke(connection);

            return connection;
        }

        public void Disconnect(BaseNode node)
        {
            // 断开节点所有连接
            foreach (var connection in Connections.ToArray())
            {
                if (connection.FromNodeGUID == node.GUID || connection.ToNodeGUID == node.GUID)
                    Disconnect(connection);
            }
        }

        public void Disconnect(BaseConnection connection)
        {
            if (!connections.Contains(connection)) return;

            connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePort fromPort);
            fromPort.DisconnectTo(connection);

            connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePort toPort);
            toPort.DisconnectTo(connection);

            connections.Remove(connection);
            onDisconnected?.Invoke(connection);
        }

        public void Disconnect(BaseNode node, BasePort port)
        {
            Disconnect(node, port.name);
        }

        public void Disconnect(BaseNode node, string portName)
        {
            foreach (var edge in connections.ToArray())
            {
                if ((edge.FromNode == node && edge.FromPortName == portName) || (edge.ToNode == node && edge.ToPortName == portName))
                    Disconnect(edge);
            }
        }
        #endregion

        #region Overrides
        public T NewNode<T>(Vector2 position) where T : BaseNode { return NewNode(typeof(T), position) as T; }
        public virtual BaseNode NewNode(Type type, Vector2 position)
        {
            return BaseNode.CreateNew(type, this, position);
        }
        public virtual BaseConnection NewConnection(BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            return BaseConnection.CreateNew<BaseConnection>(from, fromPortName, to, toPortName);
        }
        #endregion
    }
}
