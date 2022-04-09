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
using CZToolKit.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CZToolKit.GraphProcessor
{
    public partial class BasePort : ViewModel, IGraphElement
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
        public Type Type
        {
            get { return GetPropertyValue<Type>(nameof(Type)); }
            set { SetPropertyValue(nameof(Type), value); }
        }
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
            this[nameof(Type)] = new BindableProperty<Type>(() => type, v => type = v);
            OnEnabled();
        }

        #region API
        public void ConnectTo(BaseConnection connection)
        {
            connections.Add(connection);
            Resort();
            onConnected?.Invoke(connection);
        }

        public void DisconnectTo(BaseConnection connection)
        {
            connections.Remove(connection);
            Resort();
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
                    if (connection.FromNode is IGetValue fromPort)
                        yield return fromPort.GetValue(connection.FromPortName);
                }
            }
            else
            {
                foreach (var connection in Connections)
                {
                    if (connection.ToNode is IGetValue toPort)
                        yield return toPort.GetValue(connection.ToPortName);
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
                    if (connection.FromNode is IGetValue<T> fromPort)
                        yield return fromPort.GetValue(connection.FromPortName);
                }
            }
            else
            {
                foreach (var connection in Connections)
                {
                    if (connection.ToNode is IGetValue<T> toPort)
                        yield return toPort.GetValue(connection.ToPortName);
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
