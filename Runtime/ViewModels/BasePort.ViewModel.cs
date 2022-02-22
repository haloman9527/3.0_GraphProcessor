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
using System;
using System.Collections.Generic;
using System.Linq;

namespace CZToolKit.GraphProcessor
{
    public partial class BasePort : IntegratedViewModel, IGraphElement
    {
        #region Fields
        [NonSerialized] BaseNode owner;
        [NonSerialized] List<BaseConnection> connections;
        [NonSerialized] Func<BaseConnection, BaseConnection, int> comparer;

        public event Action<BaseConnection> onConnected;
        public event Action<BaseConnection> onDisconnected;
        public event Action onSorted;
        #endregion

        #region Properties
        public BaseNode Owner { get { return owner; } }
        public IReadOnlyCollection<BaseConnection> Connections { get { return connections; } }
        #endregion

        internal void Enable(BaseNode node)
        {
            owner = node;
            switch (orientation)
            {
                case Orientation.Horizontal:
                    connections = new List<BaseConnection>();
                    comparer = HorizontalComparer;
                    break;
                case Orientation.Vertical:
                    connections = new List<BaseConnection>();
                    comparer = VerticalComparer;
                    break;
            }
            OnEnabled();
        }

        #region API
        public void ConnectTo(BaseConnection connection)
        {
            connections.Add(connection);
            connections.QuickSort(comparer);
            onConnected?.Invoke(connection);
        }

        public void DisconnectTo(BaseConnection connection)
        {
            connections.Remove(connection);
            connections.QuickSort(comparer);
            onDisconnected?.Invoke(connection);
        }

        /// <summary> 强制重新排序 </summary>
        public void Resort()
        {
            connections.QuickSort(comparer);
            onSorted?.Invoke();
        }

        /// <summary> 获取连接的第一个接口的值 </summary>
        /// <returns></returns>
        public object GetConnectionValue()
        {
            return GetConnectionValues().FirstOrDefault();
        }

        /// <summary> 获取连接的接口的值 </summary>
        public IEnumerable<object> GetConnectionValues()
        {
            if (direction == Direction.Input)
            {
                foreach (var connection in Connections)
                {
                    yield return connection.FromNode.GetValue(connection.FromPortName);
                }
            }
            else
            {
                foreach (var connection in Connections)
                {
                    yield return connection.ToNode.GetValue(connection.ToPortName);
                }
            }
        }

        /// <summary> 获取连接的第一个接口的值 </summary>
        /// <returns></returns>
        public T GetConnectionValue<T>()
        {
            return GetConnectionValues<T>().FirstOrDefault();
        }

        /// <summary> 获取连接的接口的值 </summary>
        public IEnumerable<T> GetConnectionValues<T>()
        {
            if (direction == Direction.Input)
            {
                foreach (var connection in Connections)
                {
                    if (connection.FromNode is IGetValue<T> a)
                        yield return a.GetValue(connection.FromPortName);
                }
            }
            else
            {
                foreach (var connection in Connections)
                {
                    if (connection.ToNode is IGetValue<T> a)
                        yield return a.GetValue(connection.FromPortName);
                }
            }
        }
        #endregion

        #region Overrides
        protected virtual void OnEnabled() { }
        #endregion

        #region Static
        private int VerticalComparer(BaseConnection x, BaseConnection y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = direction == Direction.Input ? x.FromNode : x.ToNode;
            var nodeY = direction == Direction.Input ? y.FromNode : y.ToNode;

            // 则使用x坐标比较排序
            // 遵循从左到右
            if (nodeX.Position.x < nodeY.Position.x)
                return -1;
            if (nodeX.Position.x > nodeY.Position.x)
                return 1;

            // 若节点的x坐标相同，则使用y坐标比较排序
            // 遵循从上到下
            if (nodeX.Position.y < nodeY.Position.y)
                return -1;
            if (nodeX.Position.y > nodeY.Position.y)
                return 1;

            return 0;
        }

        private int HorizontalComparer(BaseConnection x, BaseConnection y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = direction == Direction.Input ? x.FromNode : x.ToNode;
            var nodeY = direction == Direction.Input ? y.FromNode : y.ToNode;

            // 则使用y坐标比较排序
            // 遵循从上到下
            if (nodeX.Position.y < nodeY.Position.y)
                return -1;
            if (nodeX.Position.y > nodeY.Position.y)
                return 1;

            // 若节点的y坐标相同，则使用x坐标比较排序
            // 遵循从左到右
            if (nodeX.Position.x < nodeY.Position.x)
                return -1;
            if (nodeX.Position.x > nodeY.Position.x)
                return 1;

            return 0;
        }
        #endregion
    }
}
