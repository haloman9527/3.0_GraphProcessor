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

namespace Moyo.GraphProcessor
{
    public partial class BaseGraphProcessor
    {
        #region Fields

        private List<BaseConnectionProcessor> connections;

        public event Action<BaseConnectionProcessor> OnConnected;
        public event Action<BaseConnectionProcessor> OnDisconnected;

        #endregion

        #region Properties

        public IReadOnlyList<BaseConnectionProcessor> Connections => connections;

        #endregion

        private void BeginInitConnections()
        {
            this.connections = new List<BaseConnectionProcessor>(Model.connections.Count);

            for (int i = 0; i < Model.connections.Count; i++)
            {
                var connection = Model.connections[i];

                if (!nodes.TryGetValue(connection.fromNode, out var fromNode) || !fromNode.Ports.TryGetValue(connection.fromPort, out var fromPort))
                {
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                if (!nodes.TryGetValue(connection.toNode, out var toNode) || !toNode.Ports.TryGetValue(connection.toPort, out var toPort))
                {
                    Model.connections.RemoveAt(i--);
                    continue;
                }

                var connectionVM = (BaseConnectionProcessor)ViewModelFactory.CreateViewModel(connection);
                connectionVM.Owner = this;
                fromPort.connections.Add(connectionVM);
                toPort.connections.Add(connectionVM);
                connections.Add(connectionVM);
            }
        }

        private void EndInitConnections()
        {
            foreach (var connection in connections)
            {
                connection.Enable();
            }
        }

        #region API

        public BaseConnectionProcessor Connect(BasePortProcessor fromPort, BasePortProcessor toPort)
        {
            var tmpConnection = fromPort.Connections.FirstOrDefault(tmp => tmp.FromNode == fromPort.Owner && tmp.ToPortName == toPort.Name);
            if (tmpConnection != null)
                return tmpConnection;

            if (fromPort.Capacity == BasePort.Capacity.Single)
                Disconnect(fromPort);
            if (toPort.Capacity == BasePort.Capacity.Single)
                Disconnect(toPort);
            var connection = NewConnection(fromPort, toPort);
            connection.Owner = this;
            connection.Enable();
            connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            OnConnected?.Invoke(connection);
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
            connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            OnConnected?.Invoke(connection);
        }

        public void Disconnect(BaseConnectionProcessor connection)
        {
            if (connection.FromNode.Ports.TryGetValue(connection.FromPortName, out BasePortProcessor fromPort))
            {
                fromPort.DisconnectTo(connection);
            }

            if (connection.ToNode.Ports.TryGetValue(connection.ToPortName, out BasePortProcessor toPort))
            {
                toPort.DisconnectTo(connection);
            }

            connections.Remove(connection);
            Model.connections.Remove(connection.Model);
            connection.Owner = null;
            OnDisconnected?.Invoke(connection);
        }

        public void Disconnect(BaseNodeProcessor node)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection.FromNodeID == node.ID || connection.ToNodeID == node.ID)
                {
                    Disconnect(connection);
                    i--;
                }
            }
        }

        public void Disconnect(BasePortProcessor port)
        {
            for (int i = 0; i < port.connections.Count; i++)
            {
                Disconnect(port.connections[i--]);
            }
        }

        public void RevertDisconnect(BaseConnectionProcessor connection)
        {
            var fromNode = nodes[connection.FromNodeID];
            var fromPort = fromNode.Ports[connection.FromPortName];

            var toNode = nodes[connection.ToNodeID];
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
            connections.Add(connection);
            Model.connections.Add(connection.Model);

            fromPort.ConnectTo(connection);
            toPort.ConnectTo(connection);

            OnConnected?.Invoke(connection);
        }

        public virtual BaseConnectionProcessor NewConnection(BasePortProcessor from, BasePortProcessor to)
        {
            var connection = new BaseConnection()
            {
                fromNode = from.Owner.ID,
                fromPort = from.Name,
                toNode = to.Owner.ID,
                toPort = to.Name
            };
            return ViewModelFactory.CreateViewModel(connection) as BaseConnectionProcessor;
        }

        #endregion
    }
}