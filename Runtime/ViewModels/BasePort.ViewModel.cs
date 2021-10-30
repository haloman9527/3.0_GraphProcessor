#region ◊¢  Õ
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: ∞Î÷ª¡˙œ∫»À
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public partial class BasePort : IntegratedViewModel, IGraphElement
    {
        [NonSerialized]
        BaseNode owner;
        [NonSerialized]
        internal SortedSet<BaseConnection> connections;

        public event Action<BaseConnection> onConnected;
        public event Action<BaseConnection> onDisconnected;

        public BaseNode Owner { get { return owner; } }
        public IReadOnlyCollection<BaseConnection> Connections { get { return connections; } }

        public void Enable(BaseNode node)
        {
            owner = node;
            switch (orientation)
            {
                case Orientation.Horizontal:
                    connections = new SortedSet<BaseConnection>(new ConnectionVerticalComparer());
                    break;
                case Orientation.Vertical:
                    connections = new SortedSet<BaseConnection>(new ConnectionHorizontalComparer());
                    break;
            }
        }

        protected override void BindProperties() { }

        public void ConnectTo(BaseConnection connection)
        {
            if (!connections.Add(connection))
                return;
            onConnected?.Invoke(connection);
        }

        public void DisconnectTo(BaseConnection connection)
        {
            if (!connections.Remove(connection))
                return;
            onDisconnected?.Invoke(connection);
        }

        internal class ConnectionHorizontalComparer : IComparer<BaseConnection>
        {
            public int Compare(BaseConnection x, BaseConnection y)
            {
                if (x.ToNode.Position.x < y.ToNode.Position.x)
                    return -1;
                if (x.ToNode.Position.x > y.ToNode.Position.x)
                    return 1;
                return 0;
            }
        }

        internal class ConnectionVerticalComparer : IComparer<BaseConnection>
        {
            public int Compare(BaseConnection x, BaseConnection y)
            {
                if (x.ToNode.Position.y < y.ToNode.Position.y)
                    return -1;
                if (x.ToNode.Position.y > y.ToNode.Position.y)
                    return 1;
                return 0;
            }
        }
    }
}
