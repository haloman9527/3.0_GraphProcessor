#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using CZToolKit.Core.Blackboards;
using CZToolKit.Core.SharedVariable;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public interface IBaseGraph
    {
        Vector3 Position { get; set; }
        Vector3 Scale { get; set; }
        bool BlackboardVisible { get; set; }
        Rect BlackboardPosition { get; set; }
        CZBlackboard Blackboard { get; }
        IVariableOwner VarialbeOwner { get; }
        IReadOnlyList<BaseGroup> Groups { get; }
        IReadOnlyDictionary<string, BaseNode> NodesGUIDMapping { get; }
        IReadOnlyDictionary<string, SerializableEdge> EdgesGUIDMapping { get; }
        IReadOnlyDictionary<string, BaseStack> StackNodesGUIDMapping { get; }
        IReadOnlyList<SharedVariable> Variables { get; }

        void Initialize(IGraphOwner _graphOwner);

        void InitializePropertyMapping(IVariableOwner _variableOwner);

        /// <summary> 刷新及修复数据 </summary>
        void Flush();

        /// <summary> 根据类型添加一个节点 </summary>
        T AddNode<T>(Vector2 _nodePosition) where T : BaseNode;

        /// <summary> 添加节点 </summary>
        void AddNode(BaseNode _node);

        /// <summary> 移除指定节点 </summary>
        void RemoveNode(BaseNode _node);

        /// <summary> 连接两个端口 </summary>
        SerializableEdge Connect(NodePort _inputPort, NodePort _outputPort);

        /// <summary> 断开指定连接 </summary>
        void Disconnect(SerializableEdge _edge);

        /// <summary> 根据连接的GUID断开连接 </summary>
        void Disconnect(string _edgeGUID);

        /// <summary> 断开指定端口的所有连接 </summary>
        void Disconnect(NodePort _nodePort);

        /// <summary> 断开指定节点的所有连接 </summary>
        void Disconnect(BaseNode _node);

        /// <summary> 添加一个栈 </summary>
        void AddStackNode(BaseStack _stack);

        /// <summary> 移除一个栈 </summary>
        void RemoveStackNode(BaseStack _stack);

        /// <summary> 添加一个Group </summary>
        void AddGroup(BaseGroup _group);

        /// <summary> 移除一个Group </summary>
        void RemoveGroup(BaseGroup _group);
    }
}