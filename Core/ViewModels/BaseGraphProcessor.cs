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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using CZToolKit.Blackboard;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseGraph))]
    public partial class BaseGraphProcessor : ViewModel
    {
        #region Fields

        private Dictionary<int, BaseNodeProcessor> nodes;
        private List<BaseConnectionProcessor> connections;

        public event Action<BaseNodeProcessor> OnNodeAdded;
        public event Action<BaseNodeProcessor> OnNodeRemoved;
        public event Action<BaseConnectionProcessor> OnConnected;
        public event Action<BaseConnectionProcessor> OnDisconnected;

        #endregion

        #region Properties

        public BaseGraph Model { get; }

        public Type ModelType { get; }

        public InternalVector2Int Pan
        {
            get => GetPropertyValue<InternalVector2Int>(nameof(BaseGraph.pan));
            set => SetPropertyValue(nameof(BaseGraph.pan), value);
        }

        public float Zoom
        {
            get => GetPropertyValue<float>(nameof(BaseGraph.zoom));
            set => SetPropertyValue(nameof(BaseGraph.zoom), value);
        }

        public IReadOnlyDictionary<int, BaseNodeProcessor> Nodes => nodes;

        public IReadOnlyList<BaseConnectionProcessor> Connections => connections;

        public Events<string> Events { get; }

        public BlackboardProcessor<string> Blackboard { get; }

        #endregion

        public BaseGraphProcessor(BaseGraph model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.pan = Model.pan == default ? InternalVector2Int.zero : Model.pan;
            Model.zoom = Model.zoom == 0 ? 1 : Model.zoom;
            Model.notes = Model.notes == null ? new Dictionary<int, StickyNote>() : Model.notes;

            this.RegisterProperty(nameof(BaseGraph.pan), () => ref Model.pan);
            this.RegisterProperty(nameof(BaseGraph.zoom), () => ref Model.zoom);

            this.Events = new Events<string>();
            this.Blackboard = new BlackboardProcessor<string>(new Blackboard<string>(), Events);
            this.nodes = new Dictionary<int, BaseNodeProcessor>();
            this.connections = new List<BaseConnectionProcessor>();
            this.notes = new Dictionary<int, StickyNoteProcessor>();

            
            foreach (var pair in Model.nodes)
            {
                if (pair.Value == null)
                    continue;
                var nodeProcessor = (BaseNodeProcessor)ViewModelFactory.CreateViewModel(pair.Value);
                nodeProcessor.Owner = this;
                nodes.Add(pair.Key, nodeProcessor);
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

                var connectionVM = (BaseConnectionProcessor)ViewModelFactory.CreateViewModel(connection);
                connectionVM.Owner = this;
                fromPort.connections.Add(connectionVM);
                toPort.connections.Add(connectionVM);
                connections.Add(connectionVM);
            }

            foreach (var connection in connections)
            {
                connection.Enable();
            }

            foreach (var node in nodes.Values)
            {
                node.Enable();
            }

            
            InitGroups();
            InitNotes();
        }

        #region API

        public BaseNodeProcessor AddNode<T>(InternalVector2Int position) where T : BaseNode, new()
        {
            return AddNode(typeof(T), position);
        }

        public BaseNodeProcessor AddNode(Type nodeType, InternalVector2Int position)
        {
            var nodeVM = NewNode(nodeType, position);
            AddNode(nodeVM);
            return nodeVM;
        }

        public BaseNodeProcessor AddNode(BaseNode node)
        {
            var nodeVM = ViewModelFactory.CreateViewModel(node) as BaseNodeProcessor;
            AddNode(nodeVM);
            return nodeVM;
        }

        public void AddNode(BaseNodeProcessor node)
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

        public void RemoveNode(BaseNodeProcessor node)
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

        public BaseConnectionProcessor Connect(BasePortProcessor fromPort, BasePortProcessor toPort)
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

        public void Connect(BaseConnectionProcessor connection)
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

        public void Disconnect(BaseConnectionProcessor connection)
        {
            if (connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePortProcessor fromPort))
            {
                fromPort.DisconnectTo(connection);
                fromPort.Resort();
            }

            if (connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePortProcessor toPort))
            {
                toPort.DisconnectTo(connection);
                toPort.Resort();
            }

            connections.Remove(connection);
            Model.connections.Remove(connection.Model);
            connection.Owner = null;
            OnDisconnected?.Invoke(connection);
        }

        public void Disconnect(BaseNodeProcessor node)
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

        public void Disconnect(BasePortProcessor port)
        {
            for (int i = 0; i < port.connections.Count; i++)
            {
                Disconnect(port.connections[i--]);
            }
        }

        public void RevertDisconnect(BaseConnectionProcessor connection)
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

        public virtual BaseNodeProcessor NewNode(Type nodeType, InternalVector2Int position)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            node.id = NewID();
            node.position = position;
            return ViewModelFactory.CreateViewModel(node) as BaseNodeProcessor;
        }

        public virtual BaseNodeProcessor NewNode<TNode>(InternalVector2Int position) where TNode : BaseNode, new()
        {
            var node = new TNode()
            {
                id = NewID(),
                position = position
            };
            return ViewModelFactory.CreateViewModel(node) as BaseNodeProcessor;
        }

        public virtual BaseConnectionProcessor NewConnection(BasePortProcessor from, BasePortProcessor to)
        {
            var connection = new BaseConnection()
            {
                fromNode = from.Owner.ID,
                fromPort = from.Name,
                toNode = to.Owner.ID,
                toPort = to.Name
            };
            return ViewModelFactory.CreateViewModel(connection) as BaseConnectionProcessor;
        }

        public int NewID()
        {
            var id = 0;
            do
            {
                id++;
            } while (nodes.ContainsKey(id) || notes.ContainsKey(id) || id == 0);

            return id;
        }

        #endregion
    }
}