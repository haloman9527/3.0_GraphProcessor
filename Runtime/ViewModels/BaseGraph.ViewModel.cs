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
using System.Linq;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public abstract partial class BaseGraph : IntegratedViewModel
    {
        public const string POSITION_NAME = nameof(panOffset);
        public const string SCALE_NAME = nameof(scale);

        #region 字段
        public event Action<BaseNode> onNodeAdded;
        public event Action<BaseNode> onNodeRemoved;

        public event Action<BaseConnection> onEdgeAdded;
        public event Action<BaseConnection> onEdgeRemoved;

        [NonSerialized] public List<SharedVariable> variables = new List<SharedVariable>();
        #endregion

        #region 属性
        public Vector3 Position
        {
            get { return GetPropertyValue<Vector3>(POSITION_NAME); }
            set { SetPropertyValue(POSITION_NAME, value); }
        }
        public Vector3 Scale
        {
            get { return GetPropertyValue<Vector3>(SCALE_NAME); }
            set { SetPropertyValue(SCALE_NAME, value); }
        }
        public IReadOnlyDictionary<string, BaseNode> Nodes { get { return nodes; } }
        public IReadOnlyList<BaseConnection> Connections { get { return connections; } }
        public IVariableOwner VarialbeOwner { get; private set; }
        public IReadOnlyList<SharedVariable> Variables
        {
            get
            {
                if (variables == null) CollectionVariables();
                return variables;
            }
        }
        #endregion

        public void Enable()
        {
            foreach (var node in nodes.Values)
            {
                node.Enable(this);
            }
            foreach (var edge in connections)
            {
                edge.Enable(this);
            }
        }

        public override void InitializeBindableProperties()
        {
            this[POSITION_NAME] = new BindableProperty<Vector3>(panOffset, v => panOffset = v);
            this[SCALE_NAME] = new BindableProperty<Vector3>(scale, v => scale = v);
        }

        #region API
        public virtual void Initialize(IGraphOwner graphOwner)
        {
            InitializePropertyMapping(graphOwner);
        }

        private void CollectionVariables()
        {
            if (variables == null)
                variables = new List<SharedVariable>();
            else
                variables.Clear();
            foreach (var node in nodes.Values)
            {
                variables.AddRange(SharedVariableUtility.CollectionObjectSharedVariables(node));
            }
        }

        public void InitializePropertyMapping(IVariableOwner variableOwner)
        {
            if (variables == null)
                CollectionVariables();
            VarialbeOwner = variableOwner;
            foreach (var variable in variables)
            {
                variable.InitializePropertyMapping(VarialbeOwner);
            }

            foreach (var node in Nodes.Values)
            {
                node.OnInitializedPropertyMapping(variableOwner);
            }
        }

        public string GenerateNodeGUID()
        {
            while (true)
            {
                string guid = Guid.NewGuid().ToString();
                if (!nodes.ContainsKey(guid)) return guid;
            }
        }

        public void AddNode(BaseNode node)
        {
            if (node.ContainsKey(node.GUID))
                return;
            node.Enable(this);
            nodes[node.GUID] = node;
            if (variables == null)
                CollectionVariables();
            IEnumerable<SharedVariable> nodeVariables = SharedVariableUtility.CollectionObjectSharedVariables(node);
            variables.AddRange(nodeVariables);
            if (VarialbeOwner != null)
            {
                foreach (var variable in nodeVariables)
                {
                    variable.InitializePropertyMapping(VarialbeOwner);
                }
            }
            onNodeAdded?.Invoke(node);
        }

        public void RemoveNode(BaseNode node)
        {
            if (node == null) return;
            Disconnect(node);
            nodes.Remove(node.GUID);
            onNodeRemoved?.Invoke(node);
        }

        public void Connect(BaseConnection connection)
        {
            BaseConnection tempConnection = connections.Find(item =>
            item.FromNodeGUID == connection.FromNodeGUID
            && item.FromSlotName == connection.FromSlotName
            && item.ToNodeGUID == connection.ToNodeGUID
            && item.ToSlotName == connection.ToSlotName
            );
            if (tempConnection != null)
                return;

            connection.Enable(this);

            BaseSlot fromSlot = connection.FromNode.GetSlots().FirstOrDefault(slot => slot.name == connection.FromSlotName);
            if (fromSlot.capacity == BaseSlot.Capacity.Single)
                Disconnect(connection.FromNode, fromSlot);

            BaseSlot toSlot = connection.ToNode.GetSlots().FirstOrDefault(slot => slot.name == connection.ToSlotName);
            if (toSlot.capacity == BaseSlot.Capacity.Single)
                Disconnect(connection.ToNode, toSlot);

            connection.Enable(this);
            connections.Add(connection);
            onEdgeAdded?.Invoke(connection);
        }

        public BaseConnection Connect(BaseNode from, string fromSlotName, BaseNode to, string toSlotName)
        {
            BaseConnection connection = connections.Find(edge => edge.FromNode == from && edge.FromSlotName == fromSlotName && edge.ToNode == to && edge.ToSlotName == toSlotName);
            if (connection != null)
                return connection;

            BaseSlot fromSlot = from.GetSlots().FirstOrDefault(slot => slot.name == fromSlotName);
            if (fromSlot.capacity == BaseSlot.Capacity.Single)
                Disconnect(from, fromSlot);

            BaseSlot toSlot = to.GetSlots().FirstOrDefault(slot => slot.name == toSlotName);
            if (toSlot.capacity == BaseSlot.Capacity.Single)
                Disconnect(to, toSlot);

            connection = NewConnection(from, fromSlotName, to, toSlotName);
            connection.Enable(this);
            connections.Add(connection);
            onEdgeAdded?.Invoke(connection);
            return connection;
        }

        public void Disconnect(BaseNode node)
        {
            // 断开节点所有连接
            foreach (var edge in Connections.ToArray())
            {
                if (edge.FromNode == node || edge.ToNode == node)
                    Disconnect(edge);
            }
        }

        public void Disconnect(BaseConnection edge)
        {
            if (!connections.Contains(edge)) return;
            connections.Remove(edge);
            onEdgeRemoved?.Invoke(edge);
        }

        public void Disconnect(BaseNode node, BaseSlot slot)
        {
            Disconnect(node, slot.name);
        }

        public void Disconnect(BaseNode node, string slotName)
        {
            foreach (var edge in connections.ToArray())
            {
                if ((edge.FromNode == node && edge.FromSlotName == slotName) || (edge.ToNode == node && edge.ToSlotName == slotName))
                    Disconnect(edge);
            }
        }
        #endregion

        #region Overrides
        public T NewNode<T>(Vector2 position) where T : BaseNode { return NewNode(typeof(T), position) as T; }
        public virtual BaseNode NewNode(Type type, Vector2 position)
        {
            return BaseNode.CreateNew(type, this, position);
        }
        public virtual BaseConnection NewConnection(BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            return BaseConnection.CreateNew<BaseConnection>(from, fromPortName, to, toPortName);
        }
        #endregion
    }
}
