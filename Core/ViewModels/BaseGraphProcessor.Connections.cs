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

                if (!m_Nodes.TryGetValue(connection.fromNode, out var fromNode) || !fromNode.Ports.TryGetValue(connection.fromPort, out var fromPort))
                {
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                if (!m_Nodes.TryGetValue(connection.toNode, out var toNode) || !toNode.Ports.TryGetValue(connection.toPort, out var toPort))
                {
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
            var connection = fromPort.Connections.FirstOrDefault(tmp => tmp.FromPort == fromPort && tmp.ToPort == toPort);
            if (connection != null)
                return connection;

            if (fromPort.Capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.Capacity == BasePort.Capacity.Single)
                Disconnect(toPort);
            connection = NewConnection(fromPort, toPort);
            connection.Owner = this;
            connection.Enable();
            m_Connections.Add(connection);
            m_Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            m_GraphEvents.Publish(new AddConnectionEventArgs(connection));
            return connection;
        }

        public void Connect(BaseConnectionProcessor connection)
        {
            var fromNode = Nodes[connection.FromNodeID];
            var fromPort = fromNode.Ports[connection.FromPortName];
            var toNode = Nodes[connection.ToNodeID];
            var toPort = toNode.Ports[connection.ToPortName];
            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToPort == toPort);
            if (tmpConnection != null)
                return;

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

            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.ToNodeID == connection.ToNodeID && tmp.ToPortName == connection.ToPortName);
            if (tmpConnection != null)
                return;

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

        #endregion
    }
}