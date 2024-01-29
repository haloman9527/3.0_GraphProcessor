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
 *  Blog: https://www.mindgear.net/
 *
 */

#endregion

using CZToolKit;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public class AddNodeCommand : ICommand
    {
        BaseGraphProcessor graph;
        BaseNodeProcessor nodeVM;

        public AddNodeCommand(BaseGraphProcessor graph, Type nodeType, InternalVector2Int position)
        {
            this.graph = graph;
            this.nodeVM = graph.NewNode(nodeType, position);
        }

        public AddNodeCommand(BaseGraphProcessor graph, BaseNode node)
        {
            this.graph = graph;
            this.nodeVM = ViewModelFactory.CreateViewModel(node) as BaseNodeProcessor;
        }

        public AddNodeCommand(BaseGraphProcessor graph, BaseNodeProcessor node)
        {
            this.graph = graph;
            this.nodeVM = node;
        }

        public void Do()
        {
            graph.AddNode(nodeVM);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            graph.RemoveNode(nodeVM);
        }
    }

    public class RemoveNodeCommand : ICommand
    {
        BaseGraphProcessor graph;
        BaseNodeProcessor node;

        List<BaseConnectionProcessor> connections = new List<BaseConnectionProcessor>();

        public RemoveNodeCommand(BaseGraphProcessor graph, BaseNodeProcessor node)
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

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            graph.AddNode(node);
            // 还原
            foreach (var edge in connections)
            {
                graph.RevertDisconnect(edge);
            }

            connections.Clear();
        }
    }

    public class RemoveNodesCommand : ICommand
    {
        BaseGraphProcessor graph;
        BaseNodeProcessor[] nodes;

        HashSet<BaseConnectionProcessor> connections = new HashSet<BaseConnectionProcessor>();

        public RemoveNodesCommand(BaseGraphProcessor graph, BaseNodeProcessor[] nodes)
        {
            this.graph = graph;
            this.nodes = nodes;
        }

        public void Do()
        {
            connections.Clear();
            foreach (var node in nodes)
            {
                foreach (var port in node.Ports.Values)
                {
                    foreach (var connection in port.Connections)
                    {
                        connections.Add(connection);
                    }
                }
            }

            foreach (var node in nodes)
            {
                graph.RemoveNode(node);
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (var node in nodes)
            {
                graph.AddNode(node);
            }

            // 还原
            foreach (var edge in connections)
            {
                graph.RevertDisconnect(edge);
            }

            connections.Clear();
        }
    }

    public class MoveElementsCommand : ICommand
    {
        Dictionary<IGraphScopeViewModel, Rect> oldPos;
        Dictionary<IGraphScopeViewModel, Rect> newPos;

        public MoveElementsCommand(Dictionary<IGraphScopeViewModel, Rect> newPos)
        {
            this.newPos = newPos;
        }

        public void Do()
        {
            if (oldPos == null)
                oldPos = new Dictionary<IGraphScopeViewModel, Rect>();
            else
                oldPos.Clear();

            foreach (var pair in newPos)
            {
                if (pair.Key is StickNoteProcessor note)
                {
                    var rect = new Rect(pair.Key.Position.ToVector2(), pair.Key.Position.ToVector2());
                    oldPos[pair.Key] = rect;
                    note.Position = pair.Value.position.ToInternalVector2Int();
                    note.Size = pair.Value.size.ToInternalVector2Int();
                }
                else
                {
                    var rect = new Rect(pair.Key.Position.ToVector2(), Vector2.zero);
                    oldPos[pair.Key] = rect;
                    pair.Key.Position = pair.Value.position.ToInternalVector2Int();
                }
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (var pair in oldPos)
            {
                if (pair.Key is StickNoteProcessor note)
                {
                    note.Position = pair.Value.position.ToInternalVector2Int();
                    note.Size = pair.Value.size.ToInternalVector2Int();
                }
                else
                {
                    pair.Key.Position = pair.Value.position.ToInternalVector2Int();
                }
            }
        }
    }

    public class AddGroupCommand : ICommand
    {
        public BaseGraphProcessor graph;
        public BaseGroupProcessor group;

        public AddGroupCommand(BaseGraphProcessor graph, BaseGroupProcessor group)
        {
            this.graph = graph;
            this.group = group;
        }

        public AddGroupCommand(BaseGraphProcessor graph, BaseGroup group)
        {
            this.graph = graph;
            this.group = ViewModelFactory.CreateViewModel(group) as BaseGroupProcessor;
        }

        public void Do()
        {
            graph.AddGroup(group);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            graph.RemoveGroup(group);
        }
    }

    public class RemoveGroupCommand : ICommand
    {
        public BaseGraphProcessor graph;
        public BaseGroupProcessor group;

        public RemoveGroupCommand(BaseGraphProcessor graph, BaseGroupProcessor group)
        {
            this.graph = graph;
            this.group = group;
        }

        public void Do()
        {
            graph.RemoveGroup(group);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            graph.AddGroup(group);
        }
    }

    public class RemoveGroupsCommand : ICommand
    {
        public BaseGraphProcessor graph;
        public BaseGroupProcessor[] groups;

        public RemoveGroupsCommand(BaseGraphProcessor graph, BaseGroupProcessor[] groups)
        {
            this.graph = graph;
            this.groups = groups;
        }

        public void Do()
        {
            foreach (var group in groups)
            {
                graph.RemoveGroup(group);
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (var group in groups)
            {
                graph.AddGroup(group);
            }
        }
    }

    public class AddToGroupCommand : ICommand
    {
        private BaseGraphProcessor graph;
        private BaseGroupProcessor group;
        private BaseNodeProcessor[] nodes;

        public AddToGroupCommand(BaseGraphProcessor graph, BaseGroupProcessor group, BaseNodeProcessor[] nodes)
        {
            this.graph = graph;
            this.group = group;
            this.nodes = nodes;
        }

        public void Do()
        {
            foreach (var node in nodes)
            {
                graph.Groups.AddNodeToGroup(group, node);
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (var node in nodes)
            {
                graph.Groups.RemoveNodeFromGroup(node);
            }
        }
    }

    public class RemoveFromGroupCommand : ICommand
    {
        private BaseGraphProcessor graph;
        private BaseGroupProcessor group;
        private BaseNodeProcessor[] nodes;

        public RemoveFromGroupCommand(BaseGraphProcessor graph, BaseGroupProcessor group, BaseNodeProcessor[] nodes)
        {
            this.graph = graph;
            this.group = group;
            this.nodes = nodes;
        }

        public void Do()
        {
            foreach (var node in nodes)
            {
                graph.Groups.RemoveNodeFromGroup(node);
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (var node in nodes)
            {
                graph.Groups.AddNodeToGroup(group, node);
            }
        }
    }

    public class MoveGroupsCommand : ICommand
    {
        Dictionary<BaseGroupProcessor, InternalVector2Int> oldPos = new Dictionary<BaseGroupProcessor, InternalVector2Int>();
        Dictionary<BaseGroupProcessor, InternalVector2Int> newPos = new Dictionary<BaseGroupProcessor, InternalVector2Int>();

        public MoveGroupsCommand(Dictionary<BaseGroupProcessor, InternalVector2Int> groups)
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

        public void Redo()
        {
            Do();
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
        public BaseGroupProcessor group;
        public string oldName;
        public string newName;

        public RenameGroupCommand(BaseGroupProcessor group, string newName)
        {
            this.group = group;
            this.oldName = group.GroupName;
            this.newName = newName;
        }

        public void Do()
        {
            group.GroupName = newName;
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            group.GroupName = oldName;
        }
    }

    public class GroupAddNodesCommand : ICommand
    {
        private BaseNodeProcessor[] nodes;
        private Dictionary<BaseGroupProcessor, List<int>> cache = new Dictionary<BaseGroupProcessor, List<int>>();

        public GroupAddNodesCommand(BaseGroupProcessor group, BaseNodeProcessor[] nodes)
        {
            this.nodes.Where(item => item.Owner == group.Owner && !group.Nodes.Contains(item.ID));
        }

        public void Do()
        {
            // 记录从其他Group移动过来的节点，以便撤销时还原
            // 
            // foreach (var node in nodes)
            // {
            //     if (node.Owner != group.Owner)
            //         continue;
            //     if (group.Model.nodes.Contains(node.ID))
            //         continue;
            //     
            //     
            // }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            // 还原从其他Group移动过来的节点
        }
    }

    public class AddPortCommand : ICommand
    {
        BaseNodeProcessor node;
        BasePortProcessor port;
        bool successed = false;

        public AddPortCommand(BaseNodeProcessor node, string name, BasePort.Orientation orientation, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.node = node;
            port = new BasePortProcessor(name, orientation, direction, capacity, type);
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

        public void Redo()
        {
            Do();
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
        BaseNodeProcessor node;
        BasePortProcessor port;
        bool successed = false;

        public RemovePortCommand(BaseNodeProcessor node, BasePortProcessor port)
        {
            this.node = node;
            this.port = port;
        }

        public RemovePortCommand(BaseNodeProcessor node, string name)
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

        public void Redo()
        {
            Do();
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
        private readonly BaseGraphProcessor graph;

        BasePortProcessor from;
        BasePortProcessor to;
        BaseConnectionProcessor connectionVM;
        HashSet<BaseConnectionProcessor> replacedConnections = new HashSet<BaseConnectionProcessor>();

        public ConnectCommand(BaseGraphProcessor graph, BasePortProcessor from, BasePortProcessor to)
        {
            this.graph = graph;
            this.connectionVM = graph.NewConnection(from, to);

            this.from = from;
            this.to = to;
        }

        public ConnectCommand(BaseGraphProcessor graph, BaseConnectionProcessor connection)
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

            foreach (var connection in replacedConnections)
            {
                graph.Disconnect(connection);
            }

            graph.Connect(connectionVM);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            graph.Disconnect(connectionVM);

            // 还原
            foreach (var connection in replacedConnections)
            {
                graph.RevertDisconnect(connection);
            }
        }
    }

    public class ConnectionRedirectCommand : ICommand
    {
        BaseGraphProcessor graph;
        BaseConnectionProcessor connection;

        BasePortProcessor oldFromPort, oldToPort;
        BasePortProcessor newFromPort, newToPort;

        List<BaseConnectionProcessor> replacedConnections = new List<BaseConnectionProcessor>();

        public ConnectionRedirectCommand(BaseGraphProcessor graph, BaseConnectionProcessor connection, BasePortProcessor from, BasePortProcessor to)
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
            graph.RevertDisconnect(connection);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            graph.Disconnect(connection);
            connection.Redirect(oldFromPort, oldToPort);
            graph.RevertDisconnect(connection);

            // 还原
            foreach (var connection in replacedConnections)
            {
                graph.RevertDisconnect(connection);
            }
        }
    }

    public class DisconnectCommand : ICommand
    {
        BaseGraphProcessor graph;
        BaseConnectionProcessor connection;

        public DisconnectCommand(BaseGraphProcessor graph, BaseConnectionProcessor connection)
        {
            this.graph = graph;
            this.connection = connection;
        }

        public void Do()
        {
            graph.Disconnect(connection);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            graph.RevertDisconnect(connection);
        }
    }

    public class DisconnectsCommand : ICommand
    {
        BaseGraphProcessor graph;
        BaseConnectionProcessor[] connections;

        public DisconnectsCommand(BaseGraphProcessor graph, BaseConnectionProcessor[] connections)
        {
            this.graph = graph;
            this.connections = connections;
        }

        public void Do()
        {
            foreach (var connection in connections)
            {
                graph.Disconnect(connection);
            }
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            foreach (var connection in connections)
            {
                graph.RevertDisconnect(connection);
            }
        }
    }
}