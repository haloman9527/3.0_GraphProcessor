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
using System.Linq;

namespace Jiange.GraphProcessor
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
    public class BasePortProcessor : ViewModel
    {
        #region Fields

        private bool hideLabel;
        internal List<BaseConnectionProcessor> connections;

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

        public string Name => Model.name;

        public BasePort.Direction Direction => Model.direction;

        public BasePort.Orientation Orientation => Model.orientation;

        public BasePort.Capacity Capacity => Model.capacity;

        public Type Type
        {
            get => Model.type == null ? typeof(object) : Model.type;
            set => SetFieldValue(ref Model.type, value, nameof(BasePort.type));
        }

        public bool HideLabel
        {
            get => hideLabel;
            set => SetFieldValue(ref hideLabel, value, nameof(hideLabel));
        }

        public IReadOnlyList<BaseConnectionProcessor> Connections => connections;

        #endregion

        public BasePortProcessor(BasePort model)
        {
            this.Model = model;
            this.ModelType = typeof(BasePort);
            this.connections = new List<BaseConnectionProcessor>();
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
            if (this == connection.FromPort)
            {
                connections.QuickSort(Model.orientation == BasePort.Orientation.Horizontal ? ConnectionProcessorHorizontalComparer.FromPortSortDefault : ConnectionProcessorVerticalComparer.ToPortSortDefault);
            }
            else
            {
                connections.QuickSort(Model.orientation == BasePort.Orientation.Horizontal ? ConnectionProcessorHorizontalComparer.ToPortSortDefault : ConnectionProcessorVerticalComparer.FromPortSortDefault);
            }

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

        /// <summary> 整理 </summary>
        public bool Trim()
        {
            return connections.QuickSort(Model.orientation == BasePort.Orientation.Horizontal ? ConnectionProcessorHorizontalComparer.ToPortSortDefault : ConnectionProcessorVerticalComparer.FromPortSortDefault);
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
    }
}