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

using CZToolKit;
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
            Left,
            Right
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

        public BasePort(string name, BasePort.Orientation orientation, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.name = name;
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            this.type = type;
        }
    }

    [ViewModel(typeof(BasePort))]
    public class BasePortProcessor : ViewModel, IGraphElementViewModel
    {
        #region Fields

        private bool hideLabel;
        internal List<BaseConnectionProcessor> connections;
        internal Func<BaseConnectionProcessor, BaseConnectionProcessor, int> comparer;

        public event Action<BaseConnectionProcessor> onBeforeConnected;
        public event Action<BaseConnectionProcessor> onAfterConnected;
        public event Action<BaseConnectionProcessor> onBeforeDisconnected;
        public event Action<BaseConnectionProcessor> onAfterDisconnected;
        public event Action onConnectionChanged;

        #endregion

        #region Properties

        public BasePort Model { get; }
        public Type ModelType { get; }
        public BaseNodeProcessor Owner { get; internal set; }

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
            get
            {
                var t = GetPropertyValue<Type>(nameof(BasePort.type));
                return t == null ? typeof(object) : t;
            }
            set { SetPropertyValue(nameof(BasePort.type), value); }
        }

        public bool HideLabel
        {
            get { return GetPropertyValue<bool>(nameof(hideLabel)); }
            set { SetPropertyValue(nameof(hideLabel), value); }
        }

        public IReadOnlyList<BaseConnectionProcessor> Connections
        {
            get { return connections; }
        }

        #endregion

        public BasePortProcessor(BasePort model)
        {
            this.Model = model;
            this.ModelType = typeof(BasePort);
            this.connections = new List<BaseConnectionProcessor>();
            if (Model.orientation == BasePort.Orientation.Horizontal)
                this.comparer = HorizontalComparer;
            else
                this.comparer = VerticalComparer;
            this[nameof(BasePort.type)] = new BindableProperty<Type>(() => Model.type, v => Model.type = v);
            this[nameof(hideLabel)] = new BindableProperty<bool>(() => hideLabel, v => hideLabel = v);
        }

        public BasePortProcessor(string name, BasePort.Orientation orientation, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.Model = new BasePort(name, orientation, direction, capacity, type)
            {
                name = name,
                orientation = orientation,
                direction = direction,
                capacity = capacity
            };
            this.ModelType = typeof(BasePort);
            this.connections = new List<BaseConnectionProcessor>();
            if (Model.orientation == BasePort.Orientation.Horizontal)
                this.comparer = HorizontalComparer;
            else
                this.comparer = VerticalComparer;
            this[nameof(BasePort.type)] = new BindableProperty<Type>(() => Model.type, v => Model.type = v);
            this[nameof(hideLabel)] = new BindableProperty<bool>(() => hideLabel, v => hideLabel = v);
        }

        #region API

        public T ModelAs<T>() where T : BasePort
        {
            return Model as T;
        }

        public void ConnectTo(BaseConnectionProcessor connection)
        {
            onBeforeConnected?.Invoke(connection);
            connections.Add(connection);
            onAfterConnected?.Invoke(connection);
            onConnectionChanged?.Invoke();
        }

        public void DisconnectTo(BaseConnectionProcessor connection)
        {
            onBeforeDisconnected?.Invoke(connection);
            connections.Remove(connection);
            onAfterDisconnected?.Invoke(connection);
            onConnectionChanged?.Invoke();
        }

        /// <summary> 强制重新排序 </summary>
        public void Resort()
        {
            var changed = connections.QuickSort(comparer);
            if (changed)
                onConnectionChanged?.Invoke();
        }

        /// <summary> 强制重新排序，但不触发排序事件 </summary>
        public void ResortWithoutNotify()
        {
            connections.QuickSort(comparer);
        }

        /// <summary> 获取连接的第一个接口的值 </summary>
        public object GetConnectionValue()
        {
            return GetConnectionValues().FirstOrDefault();
        }

        /// <summary> 获取连接的接口的值 </summary>
        public IEnumerable<object> GetConnectionValues()
        {
            if (Model.direction == BasePort.Direction.Left)
            {
                foreach (var connection in Connections)
                {
                    if (connection.FromNode is IGetPortValue fromPort)
                        yield return fromPort.GetValue(connection.FromPortName);
                }
            }
            else
            {
                foreach (var connection in Connections)
                {
                    if (connection.ToNode is IGetPortValue toPort)
                        yield return toPort.GetValue(connection.ToPortName);
                }
            }
        }

        /// <summary> 获取连接的第一个接口的值 </summary>
        public T GetConnectionValue<T>()
        {
            return GetConnectionValues<T>().FirstOrDefault();
        }

        /// <summary> 获取连接的接口的值 </summary>
        public IEnumerable<T> GetConnectionValues<T>()
        {
            if (Model.direction == BasePort.Direction.Left)
            {
                foreach (var connection in Connections)
                {
                    if (connection.FromNode is IGetPortValue<T> fromPort)
                        yield return fromPort.GetValue(connection.FromPortName);
                }
            }
            else
            {
                foreach (var connection in Connections)
                {
                    if (connection.ToNode is IGetPortValue<T> toPort)
                        yield return toPort.GetValue(connection.ToPortName);
                }
            }
        }

        #endregion

        #region Helper

        private int VerticalComparer(BaseConnectionProcessor x, BaseConnectionProcessor y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = Model.direction == BasePort.Direction.Left ? x.FromNode : x.ToNode;
            var nodeY = Model.direction == BasePort.Direction.Left ? y.FromNode : y.ToNode;

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

        private int HorizontalComparer(BaseConnectionProcessor x, BaseConnectionProcessor y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = Model.direction == BasePort.Direction.Left ? x.FromNode : x.ToNode;
            var nodeY = Model.direction == BasePort.Direction.Left ? y.FromNode : y.ToNode;

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