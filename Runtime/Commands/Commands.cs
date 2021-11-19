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
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public class AddNodeCommand : ICommand
    {
        BaseGraph graph;
        BaseNode node;
        public AddNodeCommand(BaseGraph graph, BaseNode node)
        {
            this.graph = graph;
            this.node = node;
        }

        public void Do()
        {
            graph.AddNode(node);
        }

        public void Undo()
        {
            graph.RemoveNode(node);
        }
    }

    public class RemoveNodeCommand : ICommand
    {
        BaseGraph graph;
        BaseNode node;

        List<BaseConnection> connections = new List<BaseConnection>();
        public RemoveNodeCommand(BaseGraph graph, BaseNode node)
        {
            this.graph = graph;
            this.node = node;
        }

        public void Do()
        {
            foreach (var edge in graph.Connections.ToArray())
            {
                if (edge.FromNode == node || edge.ToNode == node)
                {
                    connections.Add(edge);
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

    public class ConnectCommand : ICommand
    {
        BaseGraph graph;
        BaseNode from;
        string fromPortName;
        BaseNode to;
        string toPortName;

        BaseConnection connection;

        public ConnectCommand(BaseGraph graph, BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            this.graph = graph;
            this.from = from;
            this.fromPortName = fromPortName;
            this.to = to;
            this.toPortName = toPortName;
        }

        public ConnectCommand(BaseGraph graph, BaseConnection connection)
        {
            this.graph = graph;
            this.connection = connection;
        }

        public void Do()
        {
            if (connection == null)
            {
                connection = graph.Connect(from, fromPortName, to, toPortName);
            }
            else
            {
                graph.Connect(connection);
            }
        }

        public void Undo()
        {
            graph.Disconnect(connection);
        }
    }

    public class ConnectionRedirectCommand : ICommand
    {
        BaseGraph graph;
        BaseConnection connection;

        BaseNode oldFrom, oldTo;
        string oldFromPortName, oldToPortName;

        BaseNode newFrom, newTo;
        string newFromPortName, newToPortName;

        public ConnectionRedirectCommand(BaseGraph graph, BaseConnection connection, BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            this.graph = graph;
            this.connection = connection;

            oldFrom = connection.FromNode;
            oldFromPortName = connection.FromPortName;
            oldTo = connection.ToNode;
            oldToPortName = connection.ToPortName;

            newFrom = from;
            newFromPortName = fromPortName;
            newTo = to;
            newToPortName = toPortName;
        }

        public void Do()
        {
            connection.Redirect(newFrom, newFromPortName, newTo, newToPortName);
            graph.Connect(connection);
        }

        public void Undo()
        {
            graph.Disconnect(connection);
            connection.Redirect(oldFrom, oldFromPortName, oldTo, oldToPortName);
            graph.Connect(connection);
        }
    }

    public class DisconnectCommand : ICommand
    {
        BaseGraph graph;

        BaseNode from;
        BaseNode to;

        BaseConnection edge;
        bool edgeControl;

        public DisconnectCommand(BaseGraph graph, BaseConnection connection)
        {
            this.graph = graph;
            this.edge = connection;
            edgeControl = true;
        }

        public DisconnectCommand(BaseGraph graph, BaseNode from, BaseNode to)
        {
            this.graph = graph;
            this.from = from;
            this.to = to;
            edgeControl = false;
        }

        public void Do()
        {
            if (edgeControl)
            {
                graph.Disconnect(edge);
            }
            else
            {
                edge = graph.Connections.First(edge => edge.FromNode == from && edge.ToNode == to);
                graph.Disconnect(edge);
            }
        }

        public void Undo()
        {
            graph.Connect(edge);
        }
    }

    public class MoveNodeCommand : ICommand
    {
        BaseNode node;
        Vector2 currentPosition;
        Vector2 targetPosition;

        public MoveNodeCommand(BaseNode node, Vector2 position)
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

    public class ChangeValueCommand : ICommand
    {
        object target;
        System.Reflection.FieldInfo field;
        object oldValue, newValue;

        public ChangeValueCommand(object target, System.Reflection.FieldInfo field, object newValue)
        {
            this.target = target;
            this.field = field;
            this.newValue = newValue;
        }

        public void Do()
        {
            oldValue = field.GetValue(target);
            field.SetValue(target, newValue);
        }

        public void Undo()
        {
            field.SetValue(target, oldValue);
        }
    }
}

