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
using System.Linq;
using System.Collections.Generic;
using CZToolKit.Core.ViewModel;

namespace CZToolKit.GraphProcessor
{
    public class AddNodeCommand : ICommand
    {
        BaseGraphVM graph;
        BaseNodeVM nodeVM;

        public AddNodeCommand(BaseGraphVM graph, Type nodeType, InternalVector2Int position)
        {
            this.graph = graph;
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            node.position = position;
            this.nodeVM = ViewModelFactory.CreateViewModel(node) as BaseNodeVM;
        }

        public AddNodeCommand(BaseGraphVM graph, BaseNode node)
        {
            this.graph = graph;
            this.nodeVM = ViewModelFactory.CreateViewModel(node) as BaseNodeVM;
        }

        public AddNodeCommand(BaseGraphVM graph, BaseNodeVM node)
        {
            this.graph = graph;
            this.nodeVM = node;
        }

        public void Do()
        {
            graph.AddNode(nodeVM);
        }

        public void Undo()
        {
            graph.RemoveNode(nodeVM);
        }
    }

    public class RemoveNodeCommand : ICommand
    {
        BaseGraphVM graph;
        BaseNodeVM node;

        List<BaseConnectionVM> connections = new List<BaseConnectionVM>();
        public RemoveNodeCommand(BaseGraphVM graph, BaseNodeVM node)
        {
            this.graph = graph;
            this.node = node;
        }

        public void Do()
        {
            foreach (var connection in graph.Connections.ToArray())
            {
                if (connection.FromNode == node || connection.ToNode == node)
                {
                    connections.Add(connection);
                }
            }
            graph.RemoveNode(node);
        }

        public void Undo()
        {
            graph.AddNode(node);
            foreach (var edge in connections)
            {
                graph.Connect(edge);
            }
            connections.Clear();
        }
    }

    public class MoveNodeCommand : ICommand
    {
        BaseNodeVM node;
        InternalVector2Int currentPosition;
        InternalVector2Int targetPosition;

        public MoveNodeCommand(BaseNodeVM node, InternalVector2Int position)
        {
            this.node = node;
            currentPosition = node.Position;
            targetPosition = position;
        }

        public void Do()
        {
            node.Position = targetPosition;
        }

        public void Undo()
        {
            node.Position = currentPosition;
        }
    }

    public class MoveNodesCommand : ICommand
    {
        Dictionary<BaseNodeVM, InternalVector2Int> oldPos = new Dictionary<BaseNodeVM, InternalVector2Int>();
        Dictionary<BaseNodeVM, InternalVector2Int> newPos = new Dictionary<BaseNodeVM, InternalVector2Int>();

        public MoveNodesCommand(Dictionary<BaseNodeVM, InternalVector2Int> newPos)
        {
            this.newPos = newPos;
        }

        public void Do()
        {
            foreach (var pair in newPos)
            {
                oldPos[pair.Key] = pair.Key.Position;
                pair.Key.Position = pair.Value;
            }
        }

        public void Undo()
        {
            foreach (var pair in oldPos)
            {
                pair.Key.Position = pair.Value;
            }
        }
    }

    public class AddGroupCommand : ICommand
    {
        public BaseGraphVM graph;
        public BaseGroupVM group;

        public AddGroupCommand(BaseGraphVM graph, BaseGroupVM group)
        {
            this.graph = graph;
            this.group = group;
        }

        public AddGroupCommand(BaseGraphVM graph, BaseGroup group)
        {
            this.graph = graph;
            this.group = ViewModelFactory.CreateViewModel(group) as BaseGroupVM;
        }

        public void Do()
        {
            graph.AddGroup(group);
        }

        public void Undo()
        {
            graph.RemoveGroup(group);
        }
    }

    public class RemoveGroupCommand : ICommand
    {
        public BaseGraphVM graph;
        public BaseGroupVM group;

        public RemoveGroupCommand(BaseGraphVM graph, BaseGroupVM group)
        {
            this.graph = graph;
            this.group = group;
        }

        public void Do()
        {
            graph.RemoveGroup(group);
        }

        public void Undo()
        {
            graph.AddGroup(group);
        }
    }

    public class MoveGroupsCommand : ICommand
    {
        Dictionary<BaseGroupVM, InternalVector2Int> oldPos = new Dictionary<BaseGroupVM, InternalVector2Int>();
        Dictionary<BaseGroupVM, InternalVector2Int> newPos = new Dictionary<BaseGroupVM, InternalVector2Int>();

        public MoveGroupsCommand(Dictionary<BaseGroupVM, InternalVector2Int> groups)
        {
            this.newPos = groups;
            foreach (var pair in groups)
            {
                oldPos[pair.Key] = pair.Key.Position;
            }
        }

        public void Do()
        {
            foreach (var pair in newPos)
            {
                pair.Key.Position = pair.Value;
            }
        }

        public void Undo()
        {
            foreach (var pair in oldPos)
            {
                pair.Key.Position = pair.Value;
            }
        }
    }

    public class RenameGroupCommand : ICommand
    {
        public BaseGroupVM group;
        public string oldName;
        public string newName;

        public RenameGroupCommand(BaseGroupVM group, string newName)
        {
            this.group = group;
            this.oldName = group.GroupName;
            this.newName = newName;
        }

        public void Do()
        {
            group.GroupName = newName;
        }

        public void Undo()
        {
            group.GroupName = oldName;
        }
    }

    public class AddPortCommand : ICommand
    {
        BaseNodeVM node;
        BasePortVM port;
        bool successed = false;

        public AddPortCommand(BaseNodeVM node, string name, BasePort.Orientation orientation, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.node = node;
            port = new BasePortVM(name, orientation, direction, capacity, type);
        }

        public void Do()
        {
            successed = false;
            if (!node.Ports.ContainsKey(port.Name))
            {
                node.AddPort(port);
                successed = true;
            }
        }

        public void Undo()
        {
            if (!successed)
            {
                return;
            }
            node.RemovePort(port);
        }
    }

    public class RemovePortCommand : ICommand
    {
        BaseNodeVM node;
        BasePortVM port;
        bool successed = false;

        public RemovePortCommand(BaseNodeVM node, BasePortVM port)
        {
            this.node = node;
            this.port = port;
        }

        public RemovePortCommand(BaseNodeVM node, string name)
        {
            this.node = node;
            node.Ports.TryGetValue(name, out port);
        }

        public void Do()
        {
            successed = false;
            if (node.Ports.ContainsKey(port.Name))
            {
                node.AddPort(port);
                successed = true;
            }
        }

        public void Undo()
        {
            if (!successed)
            {
                return;
            }
            node.RemovePort(port);
        }
    }

    public class ConnectCommand : ICommand
    {
        private readonly BaseGraphVM graph;

        BasePortVM from;
        BasePortVM to;
        BaseConnectionVM connectionVM;
        HashSet<BaseConnectionVM> replacedConnections = new HashSet<BaseConnectionVM>();

        public ConnectCommand(BaseGraphVM graph, BasePortVM from, BasePortVM to)
        {
            this.graph = graph;
            this.connectionVM = graph.NewConnection(from, to);

            this.from = from;
            this.to = to;
        }

        public ConnectCommand(BaseGraphVM graph, BaseConnection connection)
        {
            this.graph = graph;
            this.connectionVM = ViewModelFactory.CreateViewModel(connection) as BaseConnectionVM;
            this.from = graph.Nodes[connection.fromNode].Ports[connection.fromPort];
            this.to = graph.Nodes[connection.toNode].Ports[connection.toPort];
        }

        public ConnectCommand(BaseGraphVM graph, BaseConnectionVM connection)
        {
            this.graph = graph;
            this.connectionVM = connection;
            this.from = graph.Nodes[connection.FromNodeID].Ports[connection.FromPortName];
            this.to = graph.Nodes[connection.ToNodeID].Ports[connection.ToPortName];
        }

        public void Do()
        {
            replacedConnections.Clear();
            if (from.Capacity == BasePort.Capacity.Single)
            {
                foreach (var connection in from.Connections)
                {
                    replacedConnections.Add(connection);
                }
            }
            if (to.Capacity == BasePort.Capacity.Single)
            {
                foreach (var connection in to.Connections)
                {
                    replacedConnections.Add(connection);
                }
            }

            graph.Connect(connectionVM);
        }

        public void Undo()
        {
            graph.Disconnect(connectionVM);

            // 还原
            foreach (var connection in replacedConnections)
            {
                graph.Connect(connection);
            }
        }
    }

    public class ConnectionRedirectCommand : ICommand
    {
        BaseGraphVM graph;
        BaseConnectionVM connection;

        BasePortVM oldFromPort, oldToPort;
        BasePortVM newFromPort, newToPort;

        List<BaseConnectionVM> replacedConnections = new List<BaseConnectionVM>();

        public ConnectionRedirectCommand(BaseGraphVM graph, BaseConnectionVM connection, BasePortVM from, BasePortVM to)
        {
            this.graph = graph;
            this.connection = connection;

            newFromPort = from;
            newToPort = to;
        }

        public void Do()
        {
            oldFromPort = connection.FromPort;
            oldToPort = connection.ToPort;

            replacedConnections.Clear();
            if (connection.FromPort == newFromPort)
            {
                if (newToPort.Capacity == BasePort.Capacity.Single)
                    replacedConnections.AddRange(newToPort.Connections);
            }
            else
            {
                if (newFromPort.Capacity == BasePort.Capacity.Single)
                    replacedConnections.AddRange(newFromPort.Connections);
            }

            connection.Redirect(newFromPort, newToPort);
            graph.Connect(connection);
        }

        public void Undo()
        {
            graph.Disconnect(connection);
            connection.Redirect(oldFromPort, oldToPort);
            graph.Connect(connection);

            // 还原
            foreach (var connection in replacedConnections)
            {
                graph.Connect(connection);
            }
        }
    }

    public class DisconnectCommand : ICommand
    {
        BaseGraphVM graph;
        BaseConnectionVM connection;

        public DisconnectCommand(BaseGraphVM graph, BaseConnectionVM connection)
        {
            this.graph = graph;
            this.connection = connection;
        }

        public void Do()
        {
            graph.Disconnect(connection);
        }

        public void Undo()
        {
            graph.Connect(connection);
        }
    }
}

