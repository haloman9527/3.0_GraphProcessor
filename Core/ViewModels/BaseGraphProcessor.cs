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
    public class BaseGraphProcessor : ViewModel
    {
        #region Fields

        private Dictionary<int, BaseNodeProcessor> nodes;
        private Dictionary<int, StickNoteProcessor> notes;
        private List<BaseConnectionProcessor> connections;
        private Events<string> events;
        private BlackboardProcessor<string> blackboard;
        private Groups groups;

        public event Action<BaseNodeProcessor> OnNodeAdded;
        public event Action<BaseNodeProcessor> OnNodeRemoved;
        public event Action<BaseConnectionProcessor> OnConnected;
        public event Action<BaseConnectionProcessor> OnDisconnected;
        public event Action<BaseGroupProcessor> OnGroupAdded;
        public event Action<BaseGroupProcessor> OnGroupRemoved;

        public event Action<StickNoteProcessor> OnNoteAdded;
        public event Action<StickNoteProcessor> OnNoteRemoved;

        #endregion

        #region Properties

        public BaseGraph Model { get; }

        public Type ModelType { get; }

        public InternalVector2Int Pan
        {
            get { return GetField<InternalVector2Int>(nameof(BaseGraph.pan)); }
            set { SetField(nameof(BaseGraph.pan), value); }
        }

        public float Zoom
        {
            get { return GetField<float>(nameof(BaseGraph.zoom)); }
            set { SetField(nameof(BaseGraph.zoom), value); }
        }

        public IReadOnlyDictionary<int, BaseNodeProcessor> Nodes
        {
            get { return nodes; }
        }

        public Groups Groups
        {
            get { return groups; }
        }

        public IReadOnlyList<BaseConnectionProcessor> Connections
        {
            get { return connections; }
        }

        public IReadOnlyDictionary<int, StickNoteProcessor> Notes
        {
            get { return notes; }
        }

        public Events<string> Events
        {
            get { return events; }
        }

        public BlackboardProcessor<string> Blackboard
        {
            get { return blackboard; }
        }

        #endregion

        public BaseGraphProcessor(BaseGraph model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.pan = Model.pan == default ? InternalVector2Int.zero : Model.pan;
            Model.zoom = Model.zoom == 0 ? 1 : Model.zoom;
            if (Model.notes == null)
                Model.notes = new Dictionary<int, StickNote>();

            this.events = new Events<string>();
            this.blackboard = new BlackboardProcessor<string>(new Blackboard<string>(), events);
            this.nodes = new Dictionary<int, BaseNodeProcessor>();
            this.connections = new List<BaseConnectionProcessor>();
            this.groups = new Groups();
            this.notes = new Dictionary<int, StickNoteProcessor>();

            this.RegisterField(nameof(BaseGraph.pan), () => ref Model.pan);
            this.RegisterField(nameof(BaseGraph.zoom), () => ref Model.zoom);

            foreach (var pair in Model.nodes)
            {
                if (pair.Value == null)
                    continue;
                var nodeVM = ViewModelFactory.CreateViewModel(pair.Value) as BaseNodeProcessor;
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

                var connectionVM = ViewModelFactory.CreateViewModel(connection) as BaseConnectionProcessor;
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

                var groupVM = ViewModelFactory.CreateViewModel(group) as BaseGroupProcessor;
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

            foreach (var pair in model.notes)
            {
                var note = ViewModelFactory.CreateViewModel(pair.Value) as StickNoteProcessor;
                notes.Add(pair.Key, note);
            }
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

        public void AddGroup(BaseGroupProcessor group)
        {
            groups.AddGroup(group);
            Model.groups.Add(group.Model);
            group.Owner = this;
            OnGroupAdded?.Invoke(group);
        }

        public void RemoveGroup(BaseGroupProcessor group)
        {
            groups.RemoveGroup(group);
            Model.groups.Remove(group.Model);
            OnGroupRemoved?.Invoke(group);
        }

        public void AddNote(string title, string content, InternalVector2Int position)
        {
            var note = new StickNote();
            note.id = NewID();
            note.position = position;
            note.title = title;
            note.content = content;
            var noteVm = ViewModelFactory.CreateViewModel(note) as StickNoteProcessor;

            AddNote(noteVm);
        }

        public void AddNote(StickNoteProcessor note)
        {
            notes.Add(note.ID, note);
            Model.notes.Add(note.ID, note.Model);
            OnNoteAdded?.Invoke(note);
        }

        public void RemoveNote(int id)
        {
            if (!notes.TryGetValue(id, out var note))
                return;
            notes.Remove(note.ID);
            Model.notes.Remove(note.ID);
            OnNoteRemoved?.Invoke(note);
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

        public virtual BaseGroupProcessor NewGroup(string groupName)
        {
            var group = new BaseGroup()
            {
                id = NewID(),
                groupName = groupName
            };
            return ViewModelFactory.CreateViewModel(group) as BaseGroupProcessor;
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

    public class Groups
    {
        private Dictionary<int, BaseGroupProcessor> groupMap = new Dictionary<int, BaseGroupProcessor>();
        private Dictionary<int, BaseGroupProcessor> nodeGroupMap = new Dictionary<int, BaseGroupProcessor>();

        public IReadOnlyDictionary<int, BaseGroupProcessor> GroupMap
        {
            get { return groupMap; }
        }

        public IReadOnlyDictionary<int, BaseGroupProcessor> NodeGroupMap
        {
            get { return nodeGroupMap; }
        }

        public void AddNodeToGroup(BaseGroupProcessor group, BaseNodeProcessor node)
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

        public void RemoveNodeFromGroup(BaseNodeProcessor node)
        {
            if (!nodeGroupMap.TryGetValue(node.ID, out var group))
                return;

            if (node.Owner != group.Owner)
                return;

            nodeGroupMap.Remove(node.ID);
            group.Model.nodes.Remove(node.ID);
            group.NotifyNodeRemoved(node);
        }

        public void AddGroup(BaseGroupProcessor group)
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

        public void RemoveGroup(BaseGroupProcessor group)
        {
            foreach (var nodeID in group.Nodes)
            {
                nodeGroupMap.Remove(nodeID);
            }

            groupMap.Remove(group.ID);
        }
    }
}