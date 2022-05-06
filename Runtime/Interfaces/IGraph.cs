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
using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public interface IGraph
    {
        event Action<BaseConnection> onConnected;
        event Action<BaseConnection> onDisconnected;
        event Action<Group> onGroupAdded;
        event Action<Group> onGroupRemoved;

        Vector3 Pan { get; set; }
        Vector3 Zoom { get; set; }
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
        event Action<NodeType> onNodeAdded;
        event Action<NodeType> onNodeRemoved;

        IReadOnlyDictionary<string, NodeType> Nodes { get; }

        void AddNode(NodeType node);
        NodeType AddNode(Type type, Vector2 position);
        T AddNode<T>(Vector2 position) where T : NodeType;
        void RemoveNode(NodeType node);
        NodeType NewNode(Type type, Vector2 position);
        T NewNode<T>(Vector2 position) where T : NodeType;
    }
}
