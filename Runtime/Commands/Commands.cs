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

namespace CZToolKit.GraphProcessor
{
    public class AddNodeCommand : ICommand
    {
        BaseGraphVM graph;
        Type nodeType;
        InternalVector2 position;

        BaseNodeVM node;

        public AddNodeCommand(BaseGraphVM graph, Type nodeType, InternalVector2 position)
        {
            this.graph = graph;
            this.nodeType = nodeType;
            this.position = position;
        }

        public void Do()
        {
            if (node == null)
                node = graph.AddNode(nodeType, position);
            else
                graph.AddNode(node);
        }

        public void Undo()
        {
            graph.RemoveNode(node);
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
        InternalVector2 currentPosition;
        InternalVector2 targetPosition;

        public MoveNodeCommand(BaseNodeVM node, InternalVector2 position)
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
        Dictionary<BaseNodeVM, InternalVector2> oldPos = new Dictionary<BaseNodeVM, InternalVector2>();
        Dictionary<BaseNodeVM, InternalVector2> newPos = new Dictionary<BaseNodeVM, InternalVector2>();

        public MoveNodesCommand(Dictionary<BaseNodeVM, InternalVector2> newPos)
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
        Dictionary<BaseGroupVM, InternalVector2> oldPos = new Dictionary<BaseGroupVM, InternalVector2>();
        Dictionary<BaseGroupVM, InternalVector2> newPos = new Dictionary<BaseGroupVM, InternalVector2>();

        public MoveGroupsCommand(Dictionary<BaseGroupVM, InternalVector2> groups)
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

        private readonly BasePortVM from;
        private readonly BasePortVM to;
        BaseConnectionVM connection;

        HashSet<BaseConnectionVM> replacedConnections = new HashSet<BaseConnectionVM>();

        public ConnectCommand(BaseGraphVM graph, BasePortVM from, BasePortVM to)
        {
            this.graph = graph;
            this.from = from;
            this.to = to;
        }

        public ConnectCommand(BaseGraphVM graph, BaseConnectionVM connection)
        {
            this.graph = graph;
            this.connection = connection;
            this.from = connection.FromPort;
            this.to = connection.ToPort;
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

            if (connection == null)
            {
                connection = graph.Connect(from, to);
            }
            else
            {
                graph.Connect(connection);
            }
        }

        public void Undo()
        {
            graph.Disconnect(connection);

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

