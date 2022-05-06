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
using CZToolKit.Core.ViewModel;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public partial class BaseGraph : ViewModel, IGraph, IGraph<BaseNode>
    {
        #region Fields
        public event Action<BaseNode> onNodeAdded;
        public event Action<BaseNode> onNodeRemoved;

        public event Action<BaseConnection> onConnected;
        public event Action<BaseConnection> onDisconnected;

        public event Action<Group> onGroupAdded;
        public event Action<Group> onGroupRemoved;
        #endregion

        #region Properties
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
        IReadOnlyDictionary<string, INode> IGraph.Nodes
        {
            get { return nodes as IReadOnlyDictionary<string, INode>; }
        }
        public IReadOnlyDictionary<string, BaseNode> Nodes
        {
            get { return nodes; }
        }
        public IReadOnlyList<Group> Groups
        {
            get { return groups; }
        }
        public IReadOnlyList<BaseConnection> Connections
        {
            get { return connections; }
        }
        #endregion

        public void Enable()
        {
            if (nodes == null)
                nodes = new Dictionary<string, BaseNode>();
            if (connections == null)
                connections = new List<BaseConnection>();
            if (groups == null)
                groups = new List<Group>();
            foreach (var pair in Nodes)
            {
                pair.Value.guid = pair.Key;
                pair.Value.Enable(this);
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
                connection.fromNode = this.nodes[connection.FromNodeGUID];
                connection.toNode = this.nodes[connection.ToNodeGUID];
                connection.Enable(this);

                fromPort.ConnectTo(connection);
                toPort.ConnectTo(connection);
            }
            foreach (var group in Groups)
            {
                group.Enable(this);
            }

            this[PAN_NAME] = new BindableProperty<Vector3>(() => pan, v => pan = v);
            this[ZOOM_NAME] = new BindableProperty<Vector3>(() => zoom, v => zoom = v);

            OnEnabled();
        }

        #region API
        public void AddNode(BaseNode node)
        {
            if (node.Owner != null && node.Owner != this)
                throw new Exception("节点存在其它Graph中");
            if (node.ContainsKey(node.GUID))
                throw new Exception("节点添加失败，GUID重复");
            nodes.Add(node.GUID, node);
            node.Enable(this);
            onNodeAdded?.Invoke(node);
        }

        public T AddNode<T>(Vector2 position) where T : BaseNode
        {
            T node = NewNode<T>(position);
            AddNode(node);
            return node;
        }

        public BaseNode AddNode(Type type, Vector2 position)
        {
            BaseNode node = NewNode(type, position);
            AddNode(node);
            return node;
        }

        public void RemoveNode(BaseNode node)
        {
            if (node == null)
                throw new NullReferenceException("节点不能为空");
            Disconnect(node);
            nodes.Remove(node.GUID);
            onNodeRemoved?.Invoke(node);
        }

        public T NewNode<T>(Vector2 position) where T : BaseNode
        {
            return NewNode(typeof(T), position) as T;
        }

        public string GenerateNodeGUID()
        {
            while (true)
            {
                string guid = Guid.NewGuid().ToString();
                if (!Nodes.ContainsKey(guid)) return guid;
            }
        }

        /// <summary> 给节点分配一个GUID，这将会覆盖已有GUID </summary>
        public void AllocID(BaseNode node)
        {
            node.guid = GenerateNodeGUID();
        }

        public bool Connect(BaseConnection connection)
        {
            if (!Nodes.TryGetValue(connection.FromNodeGUID, out var fromNode))
                return false;
            if (!fromNode.Ports.TryGetValue(connection.FromPortName, out var fromPort))
                return false;

            if (!Nodes.TryGetValue(connection.ToNodeGUID, out var toNode))
                return false;
            if (!toNode.Ports.TryGetValue(connection.ToPortName, out var toPort))
                return false;

            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToNodeGUID == connection.ToNodeGUID && tmp.ToPortName == connection.ToPortName);
            if (tmpConnection != null)
                return false;

            if (fromPort.capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.capacity == BasePort.Capacity.Single)
                Disconnect(toPort);

            connection.Enable(this);
            connections.Add(connection);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            onConnected?.Invoke(connection);
            return true;
        }

        public BaseConnection Connect(INode from, string fromPortName, INode to, string toPortName)
        {
            var connection = NewConnection(from, fromPortName, to, toPortName);
            if (!Connect(connection))
                return null;
            return connection;
        }

        public void Disconnect(INode node)
        {
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

        public void Disconnect(BasePort port)
        {
            if (port.Owner == null || !nodes.ContainsKey(port.Owner.GUID))
                return;
            foreach (var connection in port.Connections.ToArray())
            {
                Disconnect(connection);
            }
        }

        public void Disconnect(INode node, string portName)
        {
            Disconnect(node.Ports[portName]);
        }

        public BaseConnection NewConnection(Type type, INode from, string fromPortName, INode to, string toPortName)
        {
            var connection = Activator.CreateInstance(type) as BaseConnection;
            connection.fromNode = from;
            connection.from = from.GUID;
            connection.fromPortName = fromPortName;
            connection.toNode = to;
            connection.to = to.GUID;
            connection.toPortName = toPortName;
            return connection;
        }

        public void AddGroup(Group group)
        {
            if (groups.Contains(group))
                return;
            groups.Add(group);
            group.Enable(this);
            onGroupAdded?.Invoke(group);
        }

        public void RemoveGroup(Group group)
        {
            bool removed = groups.Remove(group);
            if (removed)
                onGroupRemoved?.Invoke(group);
        }

        public virtual BaseNode NewNode(Type type, Vector2 position)
        {
            if (!type.IsSubclassOf(typeof(BaseNode)))
                return null;
            var node = Activator.CreateInstance(type) as BaseNode;
            node.Owner = this;
            node.position = position;
            AllocID(node);
            return node;
        }

        public virtual BaseConnection NewConnection(INode from, string fromPortName, INode to, string toPortName)
        {
            return NewConnection(typeof(BaseConnection), from, fromPortName, to, toPortName);
        }
        #endregion

        #region Overrides
        protected virtual void OnEnabled() { }
        #endregion

        #region Static
        public const string PAN_NAME = nameof(pan);
        public const string ZOOM_NAME = nameof(zoom);
        #endregion
    }
}
