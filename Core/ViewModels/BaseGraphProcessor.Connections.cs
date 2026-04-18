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
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    public partial class BaseGraphProcessor
    {
        #region Fields

        private List<BaseConnectionProcessor> m_Connections;

        #endregion

        #region Properties

        public IReadOnlyList<BaseConnectionProcessor> Connections
        {
            get { return m_Connections; }
        }

        #endregion

        private void BeginInitConnections()
        {
            this.m_Connections = new List<BaseConnectionProcessor>(Model.connections.Count);

            for (int i = 0; i < Model.connections.Count; i++)
            {
                var connection = Model.connections[i];
                if (connection == null)
                {
                    ReportDiagnostic($"[MissingConnection] Null connection at index {i} removed.");
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                if (!m_Nodes.TryGetValue(connection.fromNode, out var fromNode))
                {
                    ReportDiagnostic($"[MissingConnection] fromNode={connection.fromNode} not found, removed ({connection.fromPort}->{connection.toNode}:{connection.toPort}).");
                    Model.connections.RemoveAt(i--);
                    continue;
                }
                
                if (!fromNode.Ports.TryGetValue(connection.fromPort, out var fromPort))
                {
                    ReportDiagnostic($"[MissingConnection] fromPort '{connection.fromPort}' missing on node {connection.fromNode}, connection removed.");
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                if (!m_Nodes.TryGetValue(connection.toNode, out var toNode))
                {
                    ReportDiagnostic($"[MissingConnection] toNode={connection.toNode} not found, removed ({connection.fromNode}:{connection.fromPort}->{connection.toPort}).");
                    Model.connections.RemoveAt(i--);
                    continue;
                }
                
                if (!toNode.Ports.TryGetValue(connection.toPort, out var toPort))
                {
                    ReportDiagnostic($"[MissingConnection] toPort '{connection.toPort}' missing on node {connection.toNode}, connection removed.");
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                if (!TryValidateConnection(fromPort, toPort, out var error))
                {
                    ReportDiagnostic($"[InvalidConnection] {error}, removed ({connection.fromNode}:{connection.fromPort}->{connection.toNode}:{connection.toPort}).");
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                // 去重：from+to 端口完全一致的重复连接只保留一条
                var duplicated = false;
                for (int c = 0; c < m_Connections.Count; c++)
                {
                    var exist = m_Connections[c];
                    if (exist.FromNodeID == connection.fromNode &&
                        exist.FromPortName == connection.fromPort &&
                        exist.ToNodeID == connection.toNode &&
                        exist.ToPortName == connection.toPort)
                    {
                        duplicated = true;
                        break;
                    }
                }
                if (duplicated)
                {
                    ReportDiagnostic($"[DuplicateConnection] Duplicate edge {connection.fromNode}:{connection.fromPort}->{connection.toNode}:{connection.toPort} removed.");
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                var connectionVM = (BaseConnectionProcessor)ViewModelFactory.ProduceViewModel(connection);
                connectionVM.Owner = this;
                fromPort.m_Connections.Add(connectionVM);
                toPort.m_Connections.Add(connectionVM);
                m_Connections.Add(connectionVM);
            }
        }

        private void EndInitConnections()
        {
            foreach (var connection in m_Connections)
            {
                connection.Enable();
            }
        }

        #region API

        public BaseConnectionProcessor Connect(PortProcessor fromPort, PortProcessor toPort)
        {
            if (!TryValidateConnection(fromPort, toPort, out _))
                return null;

            // 直接查找存在的连接，避免 LINQ
            foreach (var conn in fromPort.Connections)
            {
                if (conn.FromPort == fromPort && conn.ToPort == toPort)
                    return conn;
            }

            var connection = NewConnection(fromPort, toPort);
            InternalConnect(fromPort, toPort, connection);
            return connection;
        }

        public void Connect(BaseConnectionProcessor connection)
        {
            var fromNode = Nodes[connection.FromNodeID];
            var fromPort = fromNode.Ports[connection.FromPortName];
            var toNode = Nodes[connection.ToNodeID];
            var toPort = toNode.Ports[connection.ToPortName];

            if (!TryValidateConnection(fromPort, toPort, out _))
                return;
            
            // 直接查找存在的连接，避免 LINQ
            foreach (var conn in fromPort.Connections)
            {
                if (conn.ToPort == toPort)
                    return;
            }

            InternalConnect(fromPort, toPort, connection);
        }

        public void Disconnect(BaseConnectionProcessor connection)
        {
            if (connection.FromNode.Ports.TryGetValue(connection.FromPortName, out PortProcessor fromPort))
            {
                fromPort.DisconnectTo(connection);
            }

            if (connection.ToNode.Ports.TryGetValue(connection.ToPortName, out PortProcessor toPort))
            {
                toPort.DisconnectTo(connection);
            }

            m_Connections.Remove(connection);
            Model.connections.Remove(connection.Model);
            connection.Owner = null;
            m_GraphEvents.Publish(new RemoveConnectionEventArgs(connection));
        }

        public void Disconnect(BaseNodeProcessor node)
        {
            for (int i = 0; i < m_Connections.Count; i++)
            {
                var connection = m_Connections[i];
                if (connection.FromNodeID == node.ID || connection.ToNodeID == node.ID)
                {
                    Disconnect(connection);
                    i--;
                }
            }
        }

        public void Disconnect(PortProcessor port)
        {
            for (int i = 0; i < port.m_Connections.Count; i++)
            {
                Disconnect(port.m_Connections[i--]);
            }
        }

        public void RevertDisconnect(BaseConnectionProcessor connection)
        {
            var fromNode = m_Nodes[connection.FromNodeID];
            var fromPort = fromNode.Ports[connection.FromPortName];

            var toNode = m_Nodes[connection.ToNodeID];
            var toPort = toNode.Ports[connection.ToPortName];

            // 直接查找存在的连接，避免 LINQ
            foreach (var conn in fromPort.Connections)
            {
                if (conn.ToNodeID == connection.ToNodeID && conn.ToPortName == connection.ToPortName)
                    return;
            }

            InternalConnect(fromPort, toPort, connection);
        }

        /// <summary> 提取公共连接逻辑，消除 Connect/RevertDisconnect 代码重复 </summary>
        private void InternalConnect(PortProcessor fromPort, PortProcessor toPort, BaseConnectionProcessor connection)
        {
            if (!TryValidateConnection(fromPort, toPort, out var error))
                throw new InvalidOperationException(error);

            if (fromPort.Capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.Capacity == BasePort.Capacity.Single)
                Disconnect(toPort);

            connection.Owner = this;
            connection.Enable();
            m_Connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            m_GraphEvents.Publish(new AddConnectionEventArgs(connection));
        }

        public virtual BaseConnectionProcessor NewConnection(PortProcessor from, PortProcessor to)
        {
            var connection = new BaseConnection()
            {
                fromNode = from.Owner.ID,
                fromPort = from.Name,
                toNode = to.Owner.ID,
                toPort = to.Name
            };
            return ViewModelFactory.ProduceViewModel(connection) as BaseConnectionProcessor;
        }

        private static bool TryValidateConnection(PortProcessor fromPort, PortProcessor toPort, out string error)
        {
            if (fromPort == null || toPort == null)
            {
                error = "Port is null";
                return false;
            }

            if (fromPort.Owner == null || toPort.Owner == null)
            {
                error = "Port owner is null";
                return false;
            }

            if (ReferenceEquals(fromPort, toPort))
            {
                error = "Cannot connect a port to itself";
                return false;
            }

            if (fromPort.Owner == toPort.Owner && fromPort.Name == toPort.Name)
            {
                error = "Cannot connect the same node port to itself";
                return false;
            }

            if (!IsOutputDirection(fromPort.Direction) || !IsInputDirection(toPort.Direction))
            {
                error = $"Invalid direction pair {fromPort.Direction}->{toPort.Direction}";
                return false;
            }

            if (!ArePortTypesCompatible(fromPort.PortType, toPort.PortType))
            {
                error = $"Incompatible port types {fromPort.PortType?.Name ?? "Any"}->{toPort.PortType?.Name ?? "Any"}";
                return false;
            }

            error = null;
            return true;
        }

        private static bool ArePortTypesCompatible(Type fromType, Type toType)
        {
            fromType ??= typeof(object);
            toType ??= typeof(object);

            if (fromType == typeof(object) || toType == typeof(object))
                return true;

            return toType.IsAssignableFrom(fromType) || fromType.IsAssignableFrom(toType);
        }

        private static bool IsInputDirection(BasePort.Direction direction)
        {
            return direction == BasePort.Direction.Left || direction == BasePort.Direction.Top;
        }

        private static bool IsOutputDirection(BasePort.Direction direction)
        {
            return direction == BasePort.Direction.Right || direction == BasePort.Direction.Bottom;
        }

        #endregion
    }
}
