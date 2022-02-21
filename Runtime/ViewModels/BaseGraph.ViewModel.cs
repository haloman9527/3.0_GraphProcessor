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
using CZToolKit.Core.BindableProperty;
using CZToolKit.Core.SharedVariable;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public abstract partial class BaseGraph : IntegratedViewModel
    {
        #region Fields
        public event Action<BaseNode> onNodeAdded;
        public event Action<BaseNode> onNodeRemoved;

        public event Action<BaseConnection> onConnected;
        public event Action<BaseConnection> onDisconnected;

        [NonSerialized] internal List<SharedVariable> variables = new List<SharedVariable>();
        #endregion

        #region Properties
        public Vector3 Pan
        {
            get { return GetPropertyValue<Vector3>(PAN_NAME); }
            set { SetPropertyValue(PAN_NAME, value); }
        }
        public Vector3 Zoom
        {
            get { return GetPropertyValue<Vector3>(ZOOM_NAME); }
            set { SetPropertyValue(ZOOM_NAME, value); }
        }
        public IReadOnlyDictionary<string, BaseNode> Nodes
        {
            get { return nodes; }
        }
        public IReadOnlyList<BaseConnection> Connections
        {
            get { return connections; }
        }
        public IGraphOwner GraphOwner
        {
            get;
            private set;
        }
        public IVariableOwner VarialbeOwner
        {
            get { return GraphOwner; }
        }
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
            foreach (var node in Nodes.Values)
            {
                node.Enable(this);
            }
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection == null)
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!nodes.TryGetValue(connection.FromNodeGUID, out var fromNode))
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!nodes.TryGetValue(connection.ToNodeGUID, out var toNode))
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!fromNode.Ports.TryGetValue(connection.FromPortName, out var fromPort))
                {
                    connections.RemoveAt(i--);
                    continue;
                }
                if (!toNode.Ports.TryGetValue(connection.ToPortName, out var toPort))
                {
                    connections.RemoveAt(i--);
                    continue;
                }

                connection.Enable(this);

                fromPort.ConnectTo(connection);
                toPort.ConnectTo(connection);
            }

            this[PAN_NAME] = new BindableProperty<Vector3>(() => pan, v => pan = v);
            this[ZOOM_NAME] = new BindableProperty<Vector3>(() => zoom, v => zoom = v);

            OnEnabled();
        }

        public void Initialize(IGraphOwner graphOwner)
        {
            GraphOwner = graphOwner;
            InitializePropertyMapping();
            foreach (var node in nodes.Values)
            {
                node.Initialize();
            }
            OnInitialized();
        }

        private void InitializePropertyMapping()
        {
            if (variables == null)
                CollectionVariables();
            foreach (var variable in variables)
            {
                variable.InitializePropertyMapping(VarialbeOwner);
            }
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

        #region API
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
            if (node.Owner != null && node.Owner != this)
                throw new Exception("节点存在其它Graph中");
            if (node.ContainsKey(node.GUID))
                throw new Exception("节点添加失败，GUID重复");
            if (variables == null)
                CollectionVariables();

            node.Enable(this);
            nodes[node.GUID] = node;
            IEnumerable<SharedVariable> nodeVariables = SharedVariableUtility.CollectionObjectSharedVariables(node);
            variables.AddRange(nodeVariables);
            if (GraphOwner != null)
            {
                node.Initialize();
                foreach (var variable in nodeVariables)
                {
                    variable.InitializePropertyMapping(GraphOwner);
                }
            }
            onNodeAdded?.Invoke(node);
        }

        public T AddNode<T>(Vector2 position) where T : BaseNode
        {
            T node = BaseNode.CreateNew<T>(this, position);
            AddNode(node);
            return node;
        }

        public BaseNode AddNode(Type type, Vector2 position)
        {
            BaseNode node = BaseNode.CreateNew(this, type, position);
            AddNode(node);
            return node;
        }

        public void RemoveNode(BaseNode node)
        {
            if (node == null)
                throw new NullReferenceException("节点不能为空");
            Disconnect(node);
            nodes.Remove(node.GUID);
            onNodeRemoved?.Invoke(node);
        }

        public bool Connect(BaseConnection connection)
        {
            if (!Nodes.TryGetValue(connection.FromNodeGUID, out var fromNode))
                return false;
            if (!fromNode.Ports.TryGetValue(connection.FromPortName, out var fromPort))
                return false;

            if (!Nodes.TryGetValue(connection.ToNodeGUID, out var toNode))
                return false;
            if (!toNode.Ports.TryGetValue(connection.ToPortName, out var toPort))
                return false;

            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToNodeGUID == connection.ToNodeGUID && tmp.ToPortName == connection.ToPortName);
            if (tmpConnection != null)
                return false;

            if (fromPort.capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.capacity == BasePort.Capacity.Single)
                Disconnect(toPort);

            connection.Enable(this);
            connections.Add(connection);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            onConnected?.Invoke(connection);
            return true;
        }

        public BaseConnection Connect(BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            var connection = NewConnection(from, fromPortName, to, toPortName);
            if (!Connect(connection))
                return null;
            return connection;
        }

        public void Disconnect(BaseNode node)
        {
            foreach (var connection in Connections.ToArray())
            {
                if (connection.FromNodeGUID == node.GUID || connection.ToNodeGUID == node.GUID)
                    Disconnect(connection);
            }
        }

        public void Disconnect(BaseConnection connection)
        {
            if (!connections.Contains(connection)) return;

            connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePort fromPort);
            fromPort.DisconnectTo(connection);
            
            connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePort toPort);
            toPort.DisconnectTo(connection);

            connections.Remove(connection);
            onDisconnected?.Invoke(connection);
        }

        public void Disconnect(BasePort port)
        {
            if (port.Owner == null || !nodes.ContainsKey(port.Owner.GUID))
                return;
            foreach (var connection in port.Connections.ToArray())
            {
                Disconnect(connection);
            }
        }

        public void Disconnect(BaseNode node, string portName)
        {
            Disconnect(node.Ports[portName]);
        }
        #endregion

        #region Overrides
        protected virtual void OnEnabled() { }

        protected virtual void OnInitialized() { }

        public T NewNode<T>(Vector2 position) where T : BaseNode
        {
            return NewNode(typeof(T), position) as T;
        }

        public virtual BaseNode NewNode(Type type, Vector2 position)
        {
            return BaseNode.CreateNew(this, type, position);
        }

        public virtual BaseConnection NewConnection(BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            return BaseConnection.CreateNew<BaseConnection>(from, fromPortName, to, toPortName);
        }
        #endregion

        #region Static
        public const string PAN_NAME = nameof(pan);
        public const string ZOOM_NAME = nameof(zoom);
        #endregion
    }
}
