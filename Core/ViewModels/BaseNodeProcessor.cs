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
        #region Fields

        private BaseGraphProcessor owner;
        private BaseNode model;
        private int index;
        private Type modelType;
        private string title;
        private string tooltip;
        private InternalColor titleColor;

        private readonly List<BasePortProcessor> inPorts = new List<BasePortProcessor>();
        private readonly List<BasePortProcessor> outPorts = new List<BasePortProcessor>();
        private readonly Dictionary<string, BasePortProcessor> ports = new Dictionary<string, BasePortProcessor>();

        public event Action<BasePortProcessor> onPortAdded;
        public event Action<BasePortProcessor> onPortRemoved;
        public event Action<int, int> onIndexChanged;

        #endregion

        #region Properties

        public BaseNode Model => model;
        public Type ModelType => modelType;

        object IGraphElementProcessor.Model => model;

        Type IGraphElementProcessor.ModelType => modelType;

        /// <summary> 唯一标识 </summary>
        public int ID => Model.id;

        public virtual InternalVector2Int Position
        {
            get => Model.position;
            set => SetFieldValue(ref Model.position, value, nameof(BaseNode.position));
        }

        public virtual string Title
        {
            get => title;
            set => SetFieldValue(ref title, value, ConstValues.NODE_TITLE_NAME);
        }

        public virtual InternalColor TitleColor
        {
            get => titleColor;
            set => SetFieldValue(ref titleColor, value, ConstValues.NODE_TITLE_COLOR_NAME);
        }

        public virtual string Tooltip
        {
            get => tooltip;
            set => SetFieldValue(ref tooltip, value, ConstValues.NODE_TOOLTIP_NAME);
        }

        public IReadOnlyList<BasePortProcessor> InPorts => inPorts;

        public IReadOnlyList<BasePortProcessor> OutPorts => outPorts;

        public IReadOnlyDictionary<string, BasePortProcessor> Ports => ports;

        public BaseGraphProcessor Owner
        {
            get => owner;
            internal set => owner = value;
        }

        public int Index
        {
            get => index;
            set
            {
                if (index == value)
                    return;

                var oldIndex = index;
                index = value;
                onIndexChanged?.Invoke(oldIndex, index);
            }
        }

        #endregion

        public BaseNodeProcessor(BaseNode model)
        {
            this.model = model;
            this.model.position = model.position == default ? InternalVector2Int.zero : model.position;
            this.modelType = model.GetType();

            var nodeStaticInfo = GraphProcessorUtil.NodeStaticInfos[this.modelType];

            this.title = nodeStaticInfo.title;
            this.tooltip = nodeStaticInfo.tooltip;
            this.titleColor = nodeStaticInfo.customTitleColor.enable ? nodeStaticInfo.customTitleColor.value : this.titleColor;
        }

        internal void Enable()
        {
            foreach (var port in ports.Values)
            {
                if (port.connections.Count > 1)
                    port.Trim();
            }

            OnEnabled();
        }

        internal void Disable()
        {
            OnDisabled();
        }

        #region API

        public IEnumerable<BaseNodeProcessor> GetConnections(string portName)
        {
            if (!Ports.TryGetValue(portName, out var port))
                yield break;

            foreach (var connection in port.Connections)
            {
                yield return port.Direction == BasePort.Direction.Left ? connection.FromNode : connection.ToNode;
            }
        }

        public BasePortProcessor AddPort(BasePort port)
        {
            var portVM = ViewModelFactory.ProduceViewModel(port) as BasePortProcessor;
            AddPort(portVM);
            return portVM;
        }

        public void AddPort(BasePortProcessor port)
        {
            ports.Add(port.Name, port);
            switch (port.Direction)
            {
                case BasePort.Direction.Left:
                case BasePort.Direction.Top:
                {
                    inPorts.Add(port);
                    break;
                }
                case BasePort.Direction.Right:
                case BasePort.Direction.Bottom:
                {
                    outPorts.Add(port);
                    break;
                }
            }

            port.Owner = this;
            onPortAdded?.Invoke(port);
        }

        public void RemovePort(string portName)
        {
            if (!ports.TryGetValue(portName, out var port))
                return;

            RemovePort(port);
        }

        public void RemovePort(BasePortProcessor port)
        {
            if (port.Owner != this)
                return;
            if (Owner != null)
                Owner.Disconnect(port);
            ports.Remove(port.Name);
            switch (port.Direction)
            {
                case BasePort.Direction.Left:
                {
                    inPorts.Remove(port);
                    break;
                }
                case BasePort.Direction.Right:
                {
                    outPorts.Remove(port);
                    break;
                }
            }

            onPortRemoved?.Invoke(port);
        }

        public void SortPort(Func<BasePortProcessor, BasePortProcessor, int> comparer)
        {
            inPorts.QuickSort(comparer);
            outPorts.QuickSort(comparer);
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