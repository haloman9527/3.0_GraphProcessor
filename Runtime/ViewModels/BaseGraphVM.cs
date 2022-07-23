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

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseGraph))]
    public class BaseGraphVM : ViewModel
    {
        #region Fields
        Dictionary<string, BaseNodeVM> nodes;
        List<BaseConnectionVM> connections;
        List<BaseGroupVM> groups;

        public event Action<BaseNodeVM> OnNodeAdded;
        public event Action<BaseNodeVM> OnNodeRemoved;
        public event Action<BaseConnectionVM> OnConnected;
        public event Action<BaseConnectionVM> OnDisconnected;
        public event Action<BaseGroupVM> OnGroupAdded;
        public event Action<BaseGroupVM> OnGroupRemoved;
        #endregion

        #region Properties
        public BaseGraph Model
        {
            get;
        }
        public Type ModelType
        {
            get;
        }
        public InternalVector3 Pan
        {
            get { return GetPropertyValue<InternalVector3>(nameof(BaseGraph.pan)); }
            set { SetPropertyValue(nameof(BaseGraph.pan), value); }
        }
        public InternalVector3 Zoom
        {
            get { return GetPropertyValue<InternalVector3>(nameof(BaseGraph.zoom)); }
            set { SetPropertyValue(nameof(BaseGraph.zoom), value); }
        }
        public IReadOnlyDictionary<string, BaseNodeVM> Nodes
        {
            get { return nodes; }
        }
        public IReadOnlyList<BaseGroupVM> Groups
        {
            get { return groups; }
        }
        public IReadOnlyList<BaseConnectionVM> Connections
        {
            get { return connections; }
        }
        #endregion

        public BaseGraphVM(BaseGraph model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.pan = Model.pan == default ? InternalVector3.zero : Model.pan;
            Model.zoom = Model.zoom == default ? InternalVector3.one : Model.zoom;

            this.nodes = new Dictionary<string, BaseNodeVM>();
            this.groups = new List<BaseGroupVM>();
            this.connections = new List<BaseConnectionVM>();

            foreach (var pair in Model.nodes)
            {
                var nodeVM = GraphProcessorUtil.CreateViewModel(pair.Value) as BaseNodeVM;
                nodeVM.GUID = pair.Key;
                nodes.Add(pair.Key, nodeVM);
            }

            foreach (var connection in Model.connections)
            {
                connections.Add(GraphProcessorUtil.CreateViewModel(connection) as BaseConnectionVM);
            }

            foreach (var group in Model.groups)
            {
                groups.Add(GraphProcessorUtil.CreateViewModel(group) as BaseGroupVM);
            }

            this[nameof(BaseGraph.pan)] = new BindableProperty<InternalVector3>(() => Model.pan, v => Model.pan = v);
            this[nameof(BaseGraph.zoom)] = new BindableProperty<InternalVector3>(() => Model.zoom, v => Model.zoom = v);

            foreach (var pair in nodes)
            {
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
                if (fromNode == null)
                {
                    connections.RemoveAt(i--);
                    nodes.Remove(connection.FromNodeGUID);
                    continue;
                }
                if (!nodes.TryGetValue(connection.ToNodeGUID, out var toNode))
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (toNode == null)
                {
                    connections.RemoveAt(i--);
                    nodes.Remove(connection.ToNodeGUID);
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
            foreach (var group in Groups)
            {
                group.Enable(this);
            }
        }

        #region API
        public BaseNodeVM AddNode<T>(InternalVector2 position) where T : BaseNode, new()
        {
            var node = new T();
            node.position = position;
            var nodeVM = GraphProcessorUtil.CreateViewModel(node) as BaseNodeVM;
            AddNode(nodeVM);
            return nodeVM;
        }

        public BaseNodeVM AddNode(Type nodeType, InternalVector2 position)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            node.position = position;
            var nodeVM = GraphProcessorUtil.CreateViewModel(node) as BaseNodeVM;
            AddNode(nodeVM);
            return nodeVM;
        }

        public void AddNode(BaseNodeVM node)
        {
            if (string.IsNullOrEmpty(node.GUID) && node.GUID == null)
                AllocID(node);
            nodes.Add(node.GUID, node);
            Model.nodes.Add(node.GUID, node.Model);
            node.Enable(this);
            OnNodeAdded?.Invoke(node);
        }

        public void RemoveNode(string guid)
        {
            RemoveNode(Nodes[guid]);
        }

        public void RemoveNode(BaseNodeVM node)
        {
            if (node == null)
                throw new NullReferenceException("节点不能为空");
            if (node.Owner != this)
                throw new NullReferenceException("节点不是此Graph中");
            Disconnect(node);
            nodes.Remove(node.GUID);
            Model.nodes.Remove(node.GUID);
            node.Disable();
            OnNodeRemoved?.Invoke(node);
        }

        public bool Connect(BaseConnectionVM connection)
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

            if (fromPort.Capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.Capacity == BasePort.Capacity.Single)
                Disconnect(toPort);

            connection.Enable(this);
            connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            OnConnected?.Invoke(connection);
            return true;
        }

        public BaseConnectionVM Connect(BasePortVM from, BasePortVM to)
        {
            var connection = NewConnection(from, to);
            if (!Connect(connection))
                return null;
            return connection;
        }

        public void Disconnect(BaseNodeVM node)
        {
            foreach (var connection in Connections.ToArray())
            {
                if (connection.FromNodeGUID == node.GUID || connection.ToNodeGUID == node.GUID)
                    Disconnect(connection);
            }
        }

        public void Disconnect(BaseConnectionVM connection)
        {
            if (!connections.Contains(connection)) return;

            connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePortVM fromPort);
            fromPort.DisconnectTo(connection);

            connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePortVM toPort);
            toPort.DisconnectTo(connection);

            connections.Remove(connection);
            Model.connections.Remove(connection.Model);
            OnDisconnected?.Invoke(connection);
        }

        public void Disconnect(BasePortVM port)
        {
            if (port.Owner == null || !nodes.ContainsKey(port.Owner.GUID))
                return;
            foreach (var connection in port.Connections.ToArray())
            {
                Disconnect(connection);
            }
        }

        public void Disconnect(BaseNodeVM node, string portName)
        {
            Disconnect(node.Ports[portName]);
        }

        public void AddGroup(BaseGroupVM group)
        {
            if (groups.Contains(group))
                return;
            groups.Add(group);
            Model.groups.Add(group.Model);
            group.Enable(this);
            OnGroupAdded?.Invoke(group);
        }

        public void RemoveGroup(BaseGroupVM group)
        {
            bool removed = groups.Remove(group);
            Model.groups.Remove(group.Model);
            if (removed)
                OnGroupRemoved?.Invoke(group);
        }

        public virtual BaseConnectionVM NewConnection(BasePortVM from, BasePortVM to)
        {
            var connection = new BaseConnection()
            {
                fromNode = from.Owner.GUID,
                fromPort = from.Name,
                toNode = to.Owner.GUID,
                toPort = to.Name
            };
            return GraphProcessorUtil.CreateViewModel(connection) as BaseConnectionVM;
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
        public void AllocID(BaseNodeVM node)
        {
            node.GUID = GenerateNodeGUID();
        }
        #endregion
    }
}
