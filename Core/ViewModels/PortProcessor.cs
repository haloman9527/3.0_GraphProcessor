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
    [ViewModel(typeof(BasePort))]
    public class PortProcessor : ViewModel, IGraphElementProcessor
    {
        #region Fields

        private BasePort m_Model;
        private Type m_ModelType;
        private bool m_HideLabel;

        private BaseNodeProcessor m_Owner;

        internal List<BaseConnectionProcessor> m_Connections = new List<BaseConnectionProcessor>();
        
        public event Action<BaseConnectionProcessor> OnConnected;
        public event Action<BaseConnectionProcessor> onDisconnected;
        public event Action OnConnectionChanged;

        #endregion

        public PortProcessor(BasePort model)
        {
            this.m_Model = model;
            this.m_ModelType = typeof(BasePort);
        }
        
        #region Properties

        public BasePort Model
        {
            get { return m_Model; }
        }

        object IGraphElementProcessor.Model
        {
            get { return m_Model; }
        }

        public Type ModelType
        {
            get { return m_ModelType; }
        }

        public string Name
        {
            get { return m_Model.name; }
        }

        public BasePort.Direction Direction
        {
            get { return m_Model.direction; }
        }

        public BasePort.Capacity Capacity
        {
            get { return m_Model.capacity; }
        }

        public Type PortType
        {
            get => m_Model.portType == null ? typeof(object) : m_Model.portType;
            set => SetFieldValue(ref m_Model.portType, value, nameof(BasePort.portType));
        }

        public bool HideLabel
        {
            get => m_HideLabel;
            set => SetFieldValue(ref m_HideLabel, value, nameof(m_HideLabel));
        }

        public IReadOnlyList<BaseConnectionProcessor> Connections
        {
            get { return m_Connections; }
        }

        public BaseNodeProcessor Owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }

        #endregion

        public PortProcessor(string name, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.m_Model = new BasePort(name, direction, capacity, type)
            {
                name = name,
                direction = direction,
                capacity = capacity
            };
            this.m_ModelType = typeof(BasePort);
        }

        #region API

        public void ConnectTo(BaseConnectionProcessor connection)
        {
            m_Connections.Add(connection);

            switch (this.Direction)
            {
                case BasePort.Direction.Left:
                {
                    m_Connections.QuickSort(ConnectionProcessorHorizontalComparer.ToPortSortDefault);
                    break;
                }
                case BasePort.Direction.Right:
                {
                    m_Connections.QuickSort(ConnectionProcessorHorizontalComparer.FromPortSortDefault);
                    break;
                }
                case BasePort.Direction.Top:
                {
                    m_Connections.QuickSort(ConnectionProcessorVerticalComparer.InPortSortDefault);
                    break;
                }
                case BasePort.Direction.Bottom:
                {
                    m_Connections.QuickSort(ConnectionProcessorVerticalComparer.OutPortSortDefault);
                    break;
                }
            }

            OnConnected?.Invoke(connection);
            OnConnectionChanged?.Invoke();
        }

        public void DisconnectTo(BaseConnectionProcessor connection)
        {
            m_Connections.Remove(connection);
            onDisconnected?.Invoke(connection);
            OnConnectionChanged?.Invoke();
        }

        /// <summary>
        /// 整理
        /// </summary>
        public bool Trim()
        {
            var removeNum = m_Connections.RemoveAll(ConnectionProcessorComparer.EmptyComparer);

            switch (Direction)
            {
                case BasePort.Direction.Left:
                    return removeNum != 0 && m_Connections.QuickSort(ConnectionProcessorHorizontalComparer.ToPortSortDefault);
                case BasePort.Direction.Right:
                    return removeNum != 0 && m_Connections.QuickSort(ConnectionProcessorHorizontalComparer.FromPortSortDefault);
                case BasePort.Direction.Top:
                    return removeNum != 0 && m_Connections.QuickSort(ConnectionProcessorVerticalComparer.InPortSortDefault);
                case BasePort.Direction.Bottom:
                    return removeNum != 0 && m_Connections.QuickSort(ConnectionProcessorVerticalComparer.OutPortSortDefault);
            }

            return removeNum != 0;
        }

        /// <summary>
        /// 获取连接的第一个接口的值
        /// </summary>
        public object GetConnectionValue()
        {
            return GetConnectionValues().FirstOrDefault();
        }

        /// <summary>
        /// 获取连接的接口的值
        /// </summary>
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

        /// <summary>
        /// 获取连接的第一个接口的值
        /// </summary>
        public T GetConnectionValue<T>()
        {
            return GetConnectionValues<T>().FirstOrDefault();
        }

        /// <summary>
        /// 获取连接的接口的值
        /// </summary>
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