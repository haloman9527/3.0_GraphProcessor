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

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(BaseNode))]
    public class BaseNodeProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        private BaseNode m_Data;
        private Type m_DataType;
        private string m_Title;
        private string m_Tooltip;
        private InternalColor m_TitleColor;

        private readonly List<PortProcessor> m_InPorts;
        private readonly List<PortProcessor> m_OutPorts;
        private readonly Dictionary<string, PortProcessor> m_Ports;

        private BaseGraphProcessor m_Owner;
        private int m_Index;
        
        public BaseNodeProcessor(BaseNode model)
        {
            m_Data = model;
            m_Data.position = model.position == default ? InternalVector2Int.zero : model.position;
            m_DataType = model.GetType();
            
            m_InPorts = new List<PortProcessor>();
            m_OutPorts = new List<PortProcessor>();
            m_Ports = new Dictionary<string, PortProcessor>();

            var nodeStaticInfo = GraphProcessorUtil.GetNodeStaticInfo(m_DataType);
            m_Title = nodeStaticInfo.Title;
            m_Tooltip = nodeStaticInfo.Tooltip;
            m_TitleColor = nodeStaticInfo.CustomTitleColor.Active ? nodeStaticInfo.CustomTitleColor.Value : this.m_TitleColor;
        }
        
        public event Action<PortProcessor> onPortAdded;
        public event Action<PortProcessor> onPortRemoved;
        public event Action<int, int> onIndexChanged;

        public BaseNode Model
        {
            get { return m_Data; }
        }

        public Type ModelType
        {
            get { return m_DataType; }
        }

        object IGraphElementProcessor.Model
        {
            get { return m_Data; }
        }
        
        /// <summary> 唯一标识 </summary>
        public long ID
        {
            get { return Model.id; }
        }

        public virtual InternalVector2Int Position
        {
            get => Model.position;
            set => SetFieldValue(ref Model.position, value, nameof(BaseNode.position));
        }

        public virtual string Title
        {
            get => m_Title;
            set => SetFieldValue(ref m_Title, value, ConstValues.NODE_TITLE_NAME);
        }

        public virtual InternalColor TitleColor
        {
            get => m_TitleColor;
            set => SetFieldValue(ref m_TitleColor, value, ConstValues.NODE_TITLE_COLOR_NAME);
        }

        public virtual string Tooltip
        {
            get => m_Tooltip;
            set => SetFieldValue(ref m_Tooltip, value, ConstValues.NODE_TOOLTIP_NAME);
        }

        public IReadOnlyList<PortProcessor> InPorts
        {
            get { return m_InPorts; }
        }

        public IReadOnlyList<PortProcessor> OutPorts
        {
            get { return m_OutPorts; }
        }

        public IReadOnlyDictionary<string, PortProcessor> Ports
        {
            get { return m_Ports; }
        }

        public BaseGraphProcessor Owner
        {
            get => m_Owner;
            internal set => m_Owner = value;
        }

        public int Index
        {
            get => m_Index;
            set
            {
                if (m_Index == value)
                    return;

                var oldIndex = m_Index;
                m_Index = value;
                onIndexChanged?.Invoke(oldIndex, m_Index);
            }
        }

        internal void Enable()
        {
            foreach (var port in m_Ports.Values)
            {
                if (port.m_Connections.Count > 1)
                    port.Trim();
            }

            OnEnabled();
        }

        internal void Disable()
        {
            OnDisabled();
        }

        #region API

        public PortProcessor GetPort(string portName)
        {
            return m_Ports.GetValueOrDefault(portName);
        }

        public IEnumerable<BaseNodeProcessor> GetPortConnections(string portName)
        {
            var port = GetPort(portName);
            if (port == null)
            {
                yield break;
            }

            foreach (var connection in port.Connections)
            {
                yield return port.Direction == BasePort.Direction.Left ? connection.FromNode : connection.ToNode;
            }
        }

        public PortProcessor AddPort(BasePort port)
        {
            var portVM = ViewModelFactory.ProduceViewModel(port) as PortProcessor;
            AddPort(portVM);
            return portVM;
        }

        public void AddPort(PortProcessor port)
        {
            m_Ports.Add(port.Name, port);
            switch (port.Direction)
            {
                case BasePort.Direction.Left:
                case BasePort.Direction.Top:
                {
                    m_InPorts.Add(port);
                    break;
                }
                case BasePort.Direction.Right:
                case BasePort.Direction.Bottom:
                {
                    m_OutPorts.Add(port);
                    break;
                }
            }

            port.Owner = this;
            onPortAdded?.Invoke(port);
        }

        public void RemovePort(string portName)
        {
            if (!m_Ports.TryGetValue(portName, out var port))
                return;

            RemovePort(port);
        }

        public void RemovePort(PortProcessor port)
        {
            if (port.Owner != this)
                return;
            if (Owner != null)
                Owner.Disconnect(port);
            m_Ports.Remove(port.Name);
            switch (port.Direction)
            {
                case BasePort.Direction.Left:
                {
                    m_InPorts.Remove(port);
                    break;
                }
                case BasePort.Direction.Right:
                {
                    m_OutPorts.Remove(port);
                    break;
                }
            }

            onPortRemoved?.Invoke(port);
        }

        public void SortPort(Func<PortProcessor, PortProcessor, int> comparer)
        {
            m_InPorts.QuickSort(comparer);
            m_OutPorts.QuickSort(comparer);
        }

        #endregion

        #region Overrides

        protected virtual void OnEnabled()
        {
        }

        protected virtual void OnDisabled()
        {
        }

        #endregion
    }

    public class BaseNodeVM<T> : BaseNodeProcessor where T : BaseNode
    {
        public T T_Model { get; }

        public BaseNodeVM(BaseNode model) : base(model)
        {
            T_Model = model as T;
        }
    }
}