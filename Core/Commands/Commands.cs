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

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Moyo.GraphProcessor
{
    public class MoveElementsCommand : ICommand
    {
        Dictionary<IGraphElementProcessor_Scope, Rect> oldPos;
        Dictionary<IGraphElementProcessor_Scope, Rect> newPos;

        public MoveElementsCommand(Dictionary<IGraphElementProcessor_Scope, Rect> newPos)
        {
            this.newPos = newPos;
        }

        public void Do()
        {
            if (oldPos == null)
                oldPos = new Dictionary<IGraphElementProcessor_Scope, Rect>();
            else
                oldPos.Clear();

            foreach (var pair in newPos)
            {
                if (pair.Key is StickyNoteProcessor note)
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
                if (pair.Key is StickyNoteProcessor note)
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

    public class RemoveElementsCommand : ICommand
    {
        private BaseGraphProcessor graph;
        private List<IGraphElementProcessor> graphElements;
        private HashSet<IGraphElementProcessor> graphElementsSet = new HashSet<IGraphElementProcessor>();
        private Dictionary<BaseNodeProcessor, BaseGroupProcessor> nodeGroups = new Dictionary<BaseNodeProcessor, BaseGroupProcessor>();

        public RemoveElementsCommand(BaseGraphProcessor graph, IGraphElementProcessor[] graphElements)
        {
            this.graph = graph;
            this.graphElements = new List<IGraphElementProcessor>(graphElements);
            foreach (var graphElement in this.graphElements)
            {
                graphElementsSet.Add(graphElement);
            }

            for (int i = 0; i < graphElements.Length; i++)
            {
                var graphElement = graphElements[i];
                switch (graphElement)
                {
                    case BaseNodeProcessor node:
                    {
                        if (graph.Groups.NodeGroupMap.TryGetValue(node.ID, out var groupProcessor))
                        {
                            nodeGroups[node] = groupProcessor;
                        }

                        foreach (var connection in node.Ports.Values.SelectMany(port => port.connections))
                        {
                            if (this.graphElementsSet.Add(connection))
                            {
                                this.graphElements.Add(connection);
                            }
                        }

                        break;
                    }
                }
            }

            this.graphElements.QuickSort((a, b) => { return GetPriority(a).CompareTo(GetPriority(b)); });
        }

        public void Do()
        {
            // 正向移除
            for (int i = 0; i < graphElements.Count; i++)
            {
                var graphElement = graphElements[i];
                switch (graphElement)
                {
                    case BaseConnectionProcessor connection:
                    {
                        graph.Disconnect(connection);
                        break;
                    }
                    case BaseGroupProcessor group:
                    {
                        graph.RemoveGroup(group);
                        break;
                    }
                    case BaseNodeProcessor node:
                    {
                        graph.RemoveNode(node);
                        break;
                    }
                    case StickyNoteProcessor stickNote:
                    {
                        graph.RemoveNote(stickNote.ID);
                        break;
                    }
                }
            }
        }

        public void Undo()
        {
            // 反向添加
            for (int i = graphElements.Count - 1; i >= 0; i--)
            {
                var graphElement = graphElements[i];
                switch (graphElement)
                {
                    case BaseNodeProcessor node:
                    {
                        graph.AddNode(node);
                        break;
                    }
                    case StickyNoteProcessor stickNote:
                    {
                        graph.AddNote(stickNote);
                        break;
                    }
                    case BaseConnectionProcessor connection:
                    {
                        graph.RevertDisconnect(connection);
                        break;
                    }
                    case BaseGroupProcessor group:
                    {
                        graph.AddGroup(group);
                        break;
                    }
                }
            }

            foreach (var pair in nodeGroups)
            {
                graph.Groups.AddNodeToGroup(pair.Value, pair.Key);
            }
        }

        public void Redo()
        {
            Do();
        }

        public int GetPriority(IGraphElementProcessor graphElement)
        {
            switch (graphElement)
            {
                case BaseConnectionProcessor:
                case BaseGroupProcessor:
                {
                    return 1;
                }
                case BaseNodeProcessor:
                case StickyNoteProcessor:
                {
                    return 2;
                }
            }

            return int.MaxValue;
        }
    }

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
}