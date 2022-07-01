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
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public interface IGraph
    {
        event Action<BaseConnection> OnConnected;
        event Action<BaseConnection> OnDisconnected;
        event Action<Group> OnGroupAdded;
        event Action<Group> OnGroupRemoved;

        InternalVector3 Pan { get; set; }
        InternalVector3 Zoom { get; set; }
        IReadOnlyDictionary<string, INode> Nodes { get; }
        IReadOnlyList<BaseConnection> Connections { get; }
        IReadOnlyList<Group> Groups { get; }

        void Enable();
        bool Connect(BaseConnection connection);
        BaseConnection Connect(INode from, string fromPortName, INode to, string toPortName);
        void Disconnect(INode node);
        void Disconnect(BaseConnection connection);
        void Disconnect(INode node, string portName);
        void Disconnect(BasePort port);
        BaseConnection NewConnection(Type type, INode from, string fromPortName, INode to, string toPortName);
        BaseConnection NewConnection(INode from, string fromPortName, INode to, string toPortName);
        void AddGroup(Group group);
        void RemoveGroup(Group group);
    }

    public interface IGraph<NodeType> where NodeType : INode
    {
        event Action<NodeType> OnNodeAdded;
        event Action<NodeType> OnNodeRemoved;

        IReadOnlyDictionary<string, NodeType> Nodes { get; }

        void AddNode(NodeType node);
        NodeType AddNode(Type type, InternalVector2 position);
        T AddNode<T>(InternalVector2 position) where T : NodeType;
        void RemoveNode(NodeType node);
        NodeType NewNode(Type type, InternalVector2 position);
        T NewNode<T>(InternalVector2 position) where T : NodeType;
    }
}
