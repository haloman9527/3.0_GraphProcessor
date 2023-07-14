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

using CZToolKit.Common;
using CZToolKit.Common.Blackboard;
using CZToolKit.Common.ViewModel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseGraph))]
    public class BaseGraphVM : ViewModel, IGraphElementViewModel
    {
        #region Fields

        private Dictionary<int, BaseNodeVM> nodes;
        private List<BaseConnectionVM> connections;
        private Blackboard<string> blackboard;
        private Groups groups;

        public event Action<BaseNodeVM> OnNodeAdded;
        public event Action<BaseNodeVM> OnNodeRemoved;
        public event Action<BaseConnectionVM> OnConnected;
        public event Action<BaseConnectionVM> OnDisconnected;
        public event Action<BaseGroupVM> OnGroupAdded;
        public event Action<BaseGroupVM> OnGroupRemoved;

        #endregion

        #region Properties

        public BaseGraph Model { get; }

        public Type ModelType { get; }

        public InternalVector2Int Pan
        {
            get { return GetPropertyValue<InternalVector2Int>(nameof(BaseGraph.pan)); }
            set { SetPropertyValue(nameof(BaseGraph.pan), value); }
        }

        public float Zoom
        {
            get { return GetPropertyValue<float>(nameof(BaseGraph.zoom)); }
            set { SetPropertyValue(nameof(BaseGraph.zoom), value); }
        }

        public IReadOnlyDictionary<int, BaseNodeVM> Nodes
        {
            get { return nodes; }
        }

        public Groups Groups
        {
            get { return groups; }
        }

        public IReadOnlyList<BaseConnectionVM> Connections
        {
            get { return connections; }
        }

        public Blackboard<string> Blackboard
        {
            get { return blackboard; }
        }

        #endregion

        public BaseGraphVM(BaseGraph model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.pan = Model.pan == default ? InternalVector2Int.zero : Model.pan;
            Model.zoom = Model.zoom == 0 ? 1 : Model.zoom;

            this.blackboard = new Blackboard<string>();
            this.nodes = new Dictionary<int, BaseNodeVM>();
            this.connections = new List<BaseConnectionVM>();
            this.groups = new Groups();

            this.RegisterProperty(nameof(BaseGraph.pan), new BindableProperty<InternalVector2Int>(() => Model.pan, v => Model.pan = v));
            this.RegisterProperty(nameof(BaseGraph.zoom), new BindableProperty<float>(() => Model.zoom, v => Model.zoom = v));

            foreach (var pair in Model.nodes)
            {
                if (pair.Value == null)
                    continue;
                var nodeVM = ViewModelFactory.CreateViewModel(pair.Value) as BaseNodeVM;
                nodeVM.Owner = this;
                nodes.Add(pair.Key, nodeVM);
            }

            for (int i = 0; i < Model.connections.Count; i++)
            {
                var connection = Model.connections[i];

                if (!nodes.TryGetValue(connection.fromNode, out var fromNode) || !fromNode.Ports.TryGetValue(connection.fromPort, out var fromPort))
                {
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                if (!nodes.TryGetValue(connection.toNode, out var toNode) || !toNode.Ports.TryGetValue(connection.toPort, out var toPort))
                {
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                var connectionVM = ViewModelFactory.CreateViewModel(connection) as BaseConnectionVM;
                connectionVM.Owner = this;
                fromPort.connections.Add(connectionVM);
                toPort.connections.Add(connectionVM);
                connections.Add(connectionVM);
            }

            for (int i = 0; i < Model.groups.Count; i++)
            {
                var group = model.groups[i];
                if (group == null)
                {
                    model.groups.RemoveAt(i--);
                    continue;
                }

                for (int j = group.nodes.Count - 1; j >= 0; j--)
                {
                    if (!nodes.ContainsKey(group.nodes[j]))
                        group.nodes.RemoveAt(j);
                }

                var groupVM = ViewModelFactory.CreateViewModel(group) as BaseGroupVM;
                groupVM.Owner = this;
                groups.AddGroup(groupVM);
            }


            foreach (var connection in connections)
            {
                connection.Enable();
            }

            foreach (var node in nodes.Values)
            {
                node.Enable();
            }
        }

        #region API

        public BaseNodeVM AddNode<T>(InternalVector2Int position) where T : BaseNode, new()
        {
            return AddNode(typeof(T), position);
        }

        public BaseNodeVM AddNode(Type nodeType, InternalVector2Int position)
        {
            var nodeVM = NewNode(nodeType, position);
            AddNode(nodeVM);
            return nodeVM;
        }

        public BaseNodeVM AddNode(BaseNode node)
        {
            var nodeVM = ViewModelFactory.CreateViewModel(node) as BaseNodeVM;
            AddNode(nodeVM);
            return nodeVM;
        }

        public void AddNode(BaseNodeVM node)
        {
            nodes.Add(node.ID, node);
            Model.nodes.Add(node.ID, node.Model);
            node.Owner = this;
            node.Enable();
            OnNodeAdded?.Invoke(node);
        }

        public void RemoveNodeByID(int id)
        {
            RemoveNode(Nodes[id]);
        }

        public void RemoveNode(BaseNodeVM node)
        {
            if (node.Owner != this)
                throw new NullReferenceException("节点不是此Graph中");

            if (groups.NodeGroupMap.TryGetValue(node.ID, out var group))
                groups.RemoveNodeFromGroup(node);
            
            Disconnect(node);
            nodes.Remove(node.ID);
            Model.nodes.Remove(node.ID);
            node.Disable();
            OnNodeRemoved?.Invoke(node);
        }

        public BaseConnectionVM Connect(BasePortVM fromPort, BasePortVM toPort)
        {
            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.FromNode == fromPort.Owner && tmp.ToPortName == toPort.Name);
            if (tmpConnection != null)
                return tmpConnection;

            if (fromPort.Capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.Capacity == BasePort.Capacity.Single)
                Disconnect(toPort);
            var connection = NewConnection(fromPort, toPort);

            connection.Owner = this;
            connection.Enable();
            connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            fromPort.Resort();
            toPort.Resort();

            OnConnected?.Invoke(connection);
            return connection;
        }

        public void Connect(BaseConnectionVM connection)
        {
            var fromNode = Nodes[connection.FromNodeID];
            var fromPort = fromNode.Ports[connection.FromPortName];
            var toNode = Nodes[connection.ToNodeID];
            var toPort = toNode.Ports[connection.ToPortName];
            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToPort == toPort);
            if (tmpConnection != null)
                return;

            if (fromPort.Capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.Capacity == BasePort.Capacity.Single)
                Disconnect(toPort);

            connection.Owner = this;
            connection.Enable();
            connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            fromPort.Resort();
            toPort.Resort();

            OnConnected?.Invoke(connection);
        }

        public void Disconnect(BaseConnectionVM connection)
        {
            if (connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePortVM fromPort))
            {
                fromPort.DisconnectTo(connection);
                fromPort.Resort();
            }

            if (connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePortVM toPort))
            {
                toPort.DisconnectTo(connection);
                toPort.Resort();
            }

            connections.Remove(connection);
            Model.connections.Remove(connection.Model);
            connection.Owner = null;
            OnDisconnected?.Invoke(connection);
        }

        public void Disconnect(BaseNodeVM node)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection.FromNodeID == node.ID || connection.ToNodeID == node.ID)
                {
                    Disconnect(connection);
                    i--;
                }
            }
        }

        public void Disconnect(BasePortVM port)
        {
            for (int i = 0; i < port.connections.Count; i++)
            {
                Disconnect(port.connections[i--]);
            }
        }

        public void RevertDisconnect(BaseConnectionVM connection)
        {
            var fromNode = nodes[connection.FromNodeID];
            var fromPort = fromNode.Ports[connection.FromPortName];

            var toNode = nodes[connection.ToNodeID];
            var toPort = toNode.Ports[connection.ToPortName];

            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToNodeID == connection.ToNodeID && tmp.ToPortName == connection.ToPortName);
            if (tmpConnection != null)
                return;

            if (fromPort.Capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.Capacity == BasePort.Capacity.Single)
                Disconnect(toPort);

            connection.Owner = this;
            connection.Enable();
            connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            fromPort.Resort();
            toPort.Resort();
            OnConnected?.Invoke(connection);
        }

        public void AddGroup(BaseGroupVM group)
        {
            groups.AddGroup(group);
            Model.groups.Add(group.Model);
            group.Owner = this;
            OnGroupAdded?.Invoke(group);
        }

        public void RemoveGroup(BaseGroupVM group)
        {
            groups.RemoveGroup(group);
            Model.groups.Remove(group.Model);
            OnGroupRemoved?.Invoke(group);
        }

        public virtual BaseNodeVM NewNode(Type nodeType, InternalVector2Int position)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            node.id = NewID();
            node.position = position;
            return ViewModelFactory.CreateViewModel(node) as BaseNodeVM;
        }

        public virtual BaseNodeVM NewNode<TNode>(InternalVector2Int position) where TNode : BaseNode, new()
        {
            var node = new TNode()
            {
                id = NewID(),
                position = position
            };
            return ViewModelFactory.CreateViewModel(node) as BaseNodeVM;
        }

        public virtual BaseConnectionVM NewConnection(BasePortVM from, BasePortVM to)
        {
            var connection = new BaseConnection()
            {
                fromNode = from.Owner.ID,
                fromPort = from.Name,
                toNode = to.Owner.ID,
                toPort = to.Name
            };
            return ViewModelFactory.CreateViewModel(connection) as BaseConnectionVM;
        }

        public virtual BaseGroupVM NewGroup(string groupName)
        {
            var group = new BaseGroup()
            {
                id = NewID(),
                groupName = groupName
            };
            return ViewModelFactory.CreateViewModel(group) as BaseGroupVM;
        }

        public int NewID()
        {
            var id = 0;
            do
            {
                id = Util_Snowflake.GenerateIDBySnowflake32();
            } while (nodes.ContainsKey(id) || id == 0);

            return id;
        }

        #endregion
    }
    
    public class Groups
    {
        private Dictionary<int, BaseGroupVM> groupMap = new Dictionary<int, BaseGroupVM>();
        private Dictionary<int, BaseGroupVM> nodeGroupMap = new Dictionary<int, BaseGroupVM>();

        public IReadOnlyDictionary<int, BaseGroupVM> GroupMap
        {
            get { return groupMap; }
        }

        public IReadOnlyDictionary<int, BaseGroupVM> NodeGroupMap
        {
            get { return nodeGroupMap; }
        }

        public void AddNodeToGroup(BaseGroupVM group, BaseNodeVM node)
        {
            if (node.Owner != group.Owner)
                return;

            if (nodeGroupMap.TryGetValue(node.ID, out var _group))
            {
                if (_group == group)
                {
                    return;
                }
                else
                {
                    _group.Model.nodes.Remove(node.ID);
                    _group.NotifyNodeRemoved(node);
                }
            }

            nodeGroupMap[node.ID] = group;
            group.Model.nodes.Add(node.ID);
            group.NotifyNodeAdded(node);
        }

        public void RemoveNodeFromGroup(BaseNodeVM node)
        {
            if (!nodeGroupMap.TryGetValue(node.ID, out var group))
                return;
            
            if (node.Owner != group.Owner)
                return;

            nodeGroupMap.Remove(node.ID);
            group.Model.nodes.Remove(node.ID);
            group.NotifyNodeRemoved(node);
        }

        public void AddGroup(BaseGroupVM group)
        {
            this.groupMap.Add(group.ID, group);
            foreach (var pair in groupMap)
            {
                foreach (var nodeID in pair.Value.Nodes)
                {
                    this.nodeGroupMap[nodeID] = pair.Value;
                }
            }
        }

        public void RemoveGroup(BaseGroupVM group)
        {
            foreach (var nodeID in group.Nodes)
            {
                nodeGroupMap.Remove(nodeID);
            }

            groupMap.Remove(group.ID);
        }
    }
}