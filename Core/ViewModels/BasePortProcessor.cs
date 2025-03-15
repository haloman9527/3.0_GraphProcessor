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

namespace Atom.GraphProcessor
{
    public class BasePort
    {
        #region Define

        public enum Direction
        {
            Left,
            Right,
            Top,
            Bottom,
        }

        public enum Capacity
        {
            Single,
            Multi
        }

        #endregion

        public string name;
        public Direction direction;
        public Capacity capacity;
        public Type portType;

        public BasePort(string name, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.name = name;
            this.direction = direction;
            this.capacity = capacity;
            this.portType = type;
        }
    }

    [ViewModel(typeof(BasePort))]
    public class BasePortProcessor : ViewModel, IGraphElementProcessor
    {
        #region Fields

        private BasePort model;
        private Type modelType;
        internal List<BaseConnectionProcessor> connections = new List<BaseConnectionProcessor>();
        private bool hideLabel;

        public event Action<BaseConnectionProcessor> onBeforeConnected;
        public event Action<BaseConnectionProcessor> onAfterConnected;
        public event Action<BaseConnectionProcessor> onBeforeDisconnected;
        public event Action<BaseConnectionProcessor> onAfterDisconnected;
        public event Action onConnectionChanged;

        #endregion

        #region Properties

        public BasePort Model => model;
        public Type ModelType => modelType;

        object IGraphElementProcessor.Model => model;

        Type IGraphElementProcessor.ModelType => modelType;

        public string Name => model.name;

        public BasePort.Direction Direction => model.direction;

        public BasePort.Capacity Capacity => model.capacity;

        public Type portType
        {
            get => model.portType == null ? typeof(object) : model.portType;
            set => SetFieldValue(ref model.portType, value, nameof(BasePort.portType));
        }

        public bool HideLabel
        {
            get => hideLabel;
            set => SetFieldValue(ref hideLabel, value, nameof(hideLabel));
        }

        public IReadOnlyList<BaseConnectionProcessor> Connections => connections;
        
        public BaseNodeProcessor Owner { get; internal set; }

        #endregion

        public BasePortProcessor(BasePort model)
        {
            this.model = model;
            this.modelType = typeof(BasePort);
        }

        public BasePortProcessor(string name, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.model = new BasePort(name, direction, capacity, type)
            {
                name = name,
                direction = direction,
                capacity = capacity
            };
            this.modelType = typeof(BasePort);
        }

        #region API
        public void ConnectTo(BaseConnectionProcessor connection)
        {
            onBeforeConnected?.Invoke(connection);
            connections.Add(connection);

            switch (this.Direction)
            {
                case BasePort.Direction.Left:
                {
                    connections.QuickSort(ConnectionProcessorHorizontalComparer.ToPortSortDefault);
                    break;
                }
                case BasePort.Direction.Right:
                {
                    connections.QuickSort(ConnectionProcessorHorizontalComparer.FromPortSortDefault);
                    break;
                }
                case BasePort.Direction.Top:
                {
                    connections.QuickSort(ConnectionProcessorVerticalComparer.InPortSortDefault);
                    break;
                }
                case BasePort.Direction.Bottom:
                {
                    connections.QuickSort(ConnectionProcessorVerticalComparer.OutPortSortDefault);
                    break;
                }
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
            var removeNum = connections.RemoveAll(ConnectionProcessorComparer.EmptyComparer);

            switch (Direction)
            {
                case BasePort.Direction.Left:
                    return removeNum != 0 && connections.QuickSort(ConnectionProcessorHorizontalComparer.ToPortSortDefault);
                case BasePort.Direction.Right:
                    return removeNum != 0 && connections.QuickSort(ConnectionProcessorHorizontalComparer.FromPortSortDefault);
                case BasePort.Direction.Top:
                    return removeNum != 0 && connections.QuickSort(ConnectionProcessorVerticalComparer.InPortSortDefault);
                case BasePort.Direction.Bottom:
                    return removeNum != 0 && connections.QuickSort(ConnectionProcessorVerticalComparer.OutPortSortDefault);
            }

            return removeNum != 0;
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