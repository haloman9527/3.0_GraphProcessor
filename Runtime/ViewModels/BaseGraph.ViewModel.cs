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
            foreach (var node in nodes.Values)
            {
                node.Enable(this);
            }
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];

                if (connection == null)
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!nodes.TryGetValue(connection.FromNodeGUID, out var fromNode))
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!nodes.TryGetValue(connection.ToNodeGUID, out var toNode))
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!fromNode.Ports.TryGetValue(connection.FromPortName, out var fromPort))
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!toNode.Ports.TryGetValue(connection.ToPortName, out var toPort))
                {
                    connections.RemoveAt(i--);
                    continue;
                }

                connection.Enable(this);

                fromPort.ConnectTo(connection);
                toPort.ConnectTo(connection);
            }
        }

        public virtual void Initialize(IGraphOwner graphOwner)
        {
            InitializePropertyMapping(graphOwner);
            foreach (var node in nodes.Values)
            {
                node.Initialize(graphOwner);
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

        public T AddNode<T>(Vector2 position) where T : BaseNode
        {
            T node = BaseNode.CreateNew<T>(this, position);
            AddNode(node);
            return node;
        }

        public BaseNode AddNode(Type tpye, Vector2 position)
        {
            BaseNode node = BaseNode.CreateNew(this, tpye, position);
            AddNode(node);
            return node;
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
            Nodes.TryGetValue(connection.FromNodeGUID, out var fromNode);
            fromNode.Ports.TryGetValue(connection.FromPortName, out BasePort fromPort);

            Nodes.TryGetValue(connection.ToNodeGUID, out var toNode);
            toNode.Ports.TryGetValue(connection.ToPortName, out BasePort toPort);

            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToNodeGUID == connection.ToNodeGUID && tmp.ToPortName == connection.ToPortName);
            if (tmpConnection != null)
                return;

            connection.Enable(this);

            if (fromPort.capacity == BasePort.Capacity.Single)
                Disconnect(connection.FromNode, fromPort);

            if (toPort.capacity == BasePort.Capacity.Single)
                Disconnect(connection.ToNode, toPort);

            connections.Add(connection);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            onConnected?.Invoke(connection);
        }

        public BaseConnection Connect(BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            var connection = NewConnection(from, fromPortName, to, toPortName);
            Connect(connection);
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
            foreach (var connection in connections.ToArray())
            {
                if ((connection.FromNode == node && connection.FromPortName == portName) || (connection.ToNode == node && connection.ToPortName == portName))
                    Disconnect(connection);
            }
        }
        #endregion

        #region Overrides
        public T NewNode<T>(Vector2 position) where T : BaseNode { return NewNode(typeof(T), position) as T; }
        public virtual BaseNode NewNode(Type type, Vector2 position)
        {
            return BaseNode.CreateNew(this, type, position);
        }
        public virtual BaseConnection NewConnection(BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            return BaseConnection.CreateNew<BaseConnection>(from, fromPortName, to, toPortName);
        }
        #endregion
    }
}
