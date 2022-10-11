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

        private Dictionary<int, BaseNodeVM> nodes;
        private List<BaseConnectionVM> connections;
        private List<BaseGroupVM> groups;

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

        public InternalVector2 Pan
        {
            get { return GetPropertyValue<InternalVector2>(nameof(BaseGraph.pan)); }
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
            Model.pan = Model.pan == default ? InternalVector2.zero : Model.pan;
            Model.zoom = Model.zoom == default ? 1 : Model.zoom;

            this.nodes = new Dictionary<int, BaseNodeVM>();
            this.groups = new List<BaseGroupVM>();
            this.connections = new List<BaseConnectionVM>();

            this[nameof(BaseGraph.pan)] = new BindableProperty<InternalVector2>(() => Model.pan, v => Model.pan = v);
            this[nameof(BaseGraph.zoom)] = new BindableProperty<float>(() => Model.zoom, v => Model.zoom = v);

            foreach (var pair in Model.nodes)
            {
                if (pair.Value == null)
                    continue;
                var nodeVM = ViewModelFactory.CreateViewModel(pair.Value) as BaseNodeVM;
                nodeVM.ID = pair.Key;
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
                var groupVM = ViewModelFactory.CreateViewModel(group) as BaseGroupVM;
                groups.Add(groupVM);
            }

            foreach (var connection in connections)
            {
                connection.Enable(this);
            }
            
            foreach (var node in nodes.Values)
            {
                node.Enable(this);
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
            var nodeVM = ViewModelFactory.CreateViewModel(node) as BaseNodeVM;
            AddNode(nodeVM);
            return nodeVM;
        }

        public BaseNodeVM AddNode(Type nodeType, InternalVector2 position)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            node.position = position;
            var nodeVM = ViewModelFactory.CreateViewModel(node) as BaseNodeVM;
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
            if (node.ID == 0)
                AllocID(node);
            nodes.Add(node.ID, node);
            Model.nodes.Add(node.ID, node.Model);
            node.Enable(this);
            OnNodeAdded?.Invoke(node);
        }

        public void RemoveNode(int id)
        {
            RemoveNode(Nodes[id]);
        }

        public void RemoveNode(BaseNodeVM node)
        {
            if (node == null)
                throw new NullReferenceException("节点不能为空");
            if (node.Owner != this)
                throw new NullReferenceException("节点不是此Graph中");
            Disconnect(node);
            nodes.Remove(node.ID);
            Model.nodes.Remove(node.ID);
            node.Disable();
            OnNodeRemoved?.Invoke(node);
        }

        public void Connect(BaseConnectionVM connection)
        {
            if (!Nodes.TryGetValue(connection.FromNodeID, out var fromNode))
                throw new Exception($"Graph中不存在From节点:{connection.FromNodeID}");
            if (!fromNode.Ports.TryGetValue(connection.FromPortName, out var fromPort))
                throw new Exception($"From节点中不存在接口:{connection.FromPortName}");

            if (!Nodes.TryGetValue(connection.ToNodeID, out var toNode))
                throw new Exception($"Graph中不存在To节点:{connection.ToNodeID}");
            if (!toNode.Ports.TryGetValue(connection.ToPortName, out var toPort))
                throw new Exception($"To节点中不存在接口:{connection.ToPortName}");

            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToNodeID == connection.ToNodeID && tmp.ToPortName == connection.ToPortName);
            if (tmpConnection != null)
                return;

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
        }

        public BaseConnectionVM Connect(BaseConnection connection)
        {
            var vm = ViewModelFactory.CreateViewModel(connection) as BaseConnectionVM;
            Connect(vm);
            return vm;
        }

        public BaseConnectionVM Connect(BasePortVM from, BasePortVM to)
        {
            var connection = NewConnection(from, to);
            Connect(connection);
            return connection;
        }

        public void Disconnect(BaseNodeVM node)
        {
            foreach (var connection in Connections.ToArray())
            {
                if (connection.FromNodeID == node.ID || connection.ToNodeID == node.ID)
                    Disconnect(connection);
            }
        }

        public void Disconnect(BaseConnectionVM connection)
        {
            if (!connections.Contains(connection)) return;

            if (connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePortVM fromPort))
                fromPort.DisconnectTo(connection);

            if (connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePortVM toPort))
                toPort.DisconnectTo(connection);

            connections.Remove(connection);
            Model.connections.Remove(connection.Model);
            OnDisconnected?.Invoke(connection);
        }

        public void Disconnect(BasePortVM port)
        {
            if (port.Owner == null || !nodes.ContainsKey(port.Owner.ID))
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

        public BaseGroupVM AddGroup(BaseGroup group)
        {
            var vm = ViewModelFactory.CreateViewModel(group) as BaseGroupVM;
            AddGroup(vm);
            return vm;
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
                fromNode = from.Owner.ID,
                fromPort = from.Name,
                toNode = to.Owner.ID,
                toPort = to.Name
            };
            return ViewModelFactory.CreateViewModel(connection) as BaseConnectionVM;
        }

        public int GenerateNodeGUID()
        {
            while (true)
            {
                int id = GraphProcessorUtil.Random.Next();
                if (id != 0 && !Nodes.ContainsKey(id)) return id;
            }
        }

        /// <summary> 给节点分配一个GUID，这将会覆盖已有GUID </summary>
        public void AllocID(BaseNodeVM node)
        {
            node.ID = GenerateNodeGUID();
        }

        #endregion
    }
}