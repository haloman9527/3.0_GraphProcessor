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
    public class BasePort
    {
        #region Define
        public enum Direction
        {
            Input,
            Output
        }
        public enum Orientation
        {
            Horizontal,
            Vertical
        }
        public enum Capacity
        {
            Single,
            Multi
        }
        #endregion

        public string name;
        public Orientation orientation;
        public Direction direction;
        public Capacity capacity;
        public Type type;
    }

    [ViewModel(typeof(BasePort))]
    public class BasePortVM : ViewModel
    {
        #region Fields

        [NonSerialized] List<BaseConnectionVM> connections;
        [NonSerialized] Func<BaseConnectionVM, BaseConnectionVM, int> comparer;

        public event Action<BaseConnectionVM> onConnected;
        public event Action<BaseConnectionVM> onDisconnected;
        public event Action onSorted;
        #endregion

        #region Properties
        public BasePort Model
        {
            get;
        }
        public Type ModelType
        {
            get;
        }
        public BaseNodeVM Owner
        {
            get;
            private set;
        }
        public string Name
        {
            get { return Model.name; }
        }
        public BasePort.Direction Direction
        {
            get { return Model.direction; }
        }
        public BasePort.Orientation Orientation
        {
            get { return Model.orientation; }
        }
        public BasePort.Capacity Capacity
        {
            get { return Model.capacity; }
        }
        public Type Type
        {
            get { return GetPropertyValue<Type>(nameof(BasePort.type)); }
            set { SetPropertyValue(nameof(BasePort.type), value); }
        }
        public IReadOnlyCollection<BaseConnectionVM> Connections
        {
            get { return connections; }
        }
        #endregion

        public BasePortVM(string name, BasePort.Orientation orientation, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.Model = new BasePort()
            {
                name = name,
                orientation = orientation,
                direction = direction,
                capacity = capacity,
                type = type == null ? typeof(object) : type
            };
            this.ModelType = typeof(BasePort);
            this[nameof(BasePort.type)] = new BindableProperty<Type>(() => Model.type, v => Model.type = v);
            switch (Model.orientation)
            {
                case BasePort.Orientation.Horizontal:
                    connections = new List<BaseConnectionVM>();
                    comparer = HorizontalComparer;
                    break;
                case BasePort.Orientation.Vertical:
                    connections = new List<BaseConnectionVM>();
                    comparer = VerticalComparer;
                    break;
            }
        }

        internal void Enable(BaseNodeVM node)
        {
            Owner = node;
        }

        #region API
        public void ConnectTo(BaseConnectionVM connection)
        {
            connections.Add(connection);
            Resort();
            onConnected?.Invoke(connection);
        }

        public void DisconnectTo(BaseConnectionVM connection)
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
            if (Model.direction == BasePort.Direction.Input)
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
            if (Model.direction == BasePort.Direction.Input)
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

        #region Helper
        private int VerticalComparer(BaseConnectionVM x, BaseConnectionVM y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = Model.direction == BasePort.Direction.Input ? x.FromNode : x.ToNode;
            var nodeY = Model.direction == BasePort.Direction.Input ? y.FromNode : y.ToNode;

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

        private int HorizontalComparer(BaseConnectionVM x, BaseConnectionVM y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = Model.direction == BasePort.Direction.Input ? x.FromNode : x.ToNode;
            var nodeY = Model.direction == BasePort.Direction.Input ? y.FromNode : y.ToNode;

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
