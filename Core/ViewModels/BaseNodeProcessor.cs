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

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseNode))]
    public class BaseNodeProcessor : ViewModel, IGraphScopeViewModel
    {
        #region Fields

        private List<BasePortProcessor> leftPorts;
        private List<BasePortProcessor> rightPorts;
        private Dictionary<string, BasePortProcessor> ports;

        public event Action<BasePortProcessor> onPortAdded;
        public event Action<BasePortProcessor> onPortRemoved;

        #endregion

        #region Properties

        public BaseNode Model { get; }
        public Type ModelType { get; }

        /// <summary> 唯一标识 </summary>
        public int ID
        {
            get { return Model.id; }
        }

        public virtual InternalVector2Int Position
        {
            get { return GetPropertyValue<InternalVector2Int>(nameof(BaseNode.position)); }
            set { SetPropertyValue(nameof(BaseNode.position), value); }
        }

        public virtual string Title
        {
            get { return GetPropertyValue<string>(TITLE_NAME); }
            set { SetPropertyValue(TITLE_NAME, value); }
        }

        public virtual InternalColor TitleColor
        {
            get { return GetPropertyValue<InternalColor>(TITLE_COLOR_NAME); }
            set { SetPropertyValue(TITLE_COLOR_NAME, value); }
        }

        public virtual string Tooltip
        {
            get { return GetPropertyValue<string>(TOOLTIP_NAME); }
            set { SetPropertyValue(TOOLTIP_NAME, value); }
        }

        public IReadOnlyList<BasePortProcessor> LeftPorts
        {
            get { return leftPorts; }
        }

        public IReadOnlyList<BasePortProcessor> RightPorts
        {
            get { return rightPorts; }
        }

        public IReadOnlyDictionary<string, BasePortProcessor> Ports
        {
            get { return ports; }
        }

        public BaseGraphProcessor Owner { get; internal set; }

        #endregion

        public BaseNodeProcessor(BaseNode model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.position = Model.position == default ? InternalVector2Int.zero : Model.position;
            leftPorts = new List<BasePortProcessor>();
            rightPorts = new List<BasePortProcessor>();
            ports = new Dictionary<string, BasePortProcessor>();

            var nodeStaticInfo = GraphProcessorUtil.NodeStaticInfos[ModelType];

            string title = nodeStaticInfo.title;
            this[TITLE_NAME] = new BindableProperty<string>(() => title, v => title = v);

            string tooltip = nodeStaticInfo.tooltip;
            this[TOOLTIP_NAME] = new BindableProperty<string>(() => tooltip, v => tooltip = v);

            if (nodeStaticInfo.customTitleColor.enable)
            {
                var titleColor = nodeStaticInfo.customTitleColor.value;
                this[TITLE_COLOR_NAME] = new BindableProperty<InternalColor>(() => titleColor, v => titleColor = v);
            }

            this[nameof(BaseNode.position)] = new BindableProperty<InternalVector2Int>(() => Model.position, v => Model.position = v);
        }

        internal void Enable()
        {
            foreach (var port in ports.Values)
            {
                if (port.connections.Count > 1)
                    port.ResortWithoutNotify();
            }

            OnEnabled();
        }

        internal void Disable()
        {
            OnDisabled();
        }

        #region API

        public T ModelAs<T>() where T : BaseNode
        {
            return Model as T;
        }

        public IEnumerable<BaseNodeProcessor> GetConnections(string portName)
        {
            if (!Ports.TryGetValue(portName, out var port))
                yield break;
            if (port.Direction == BasePort.Direction.Left)
            {
                foreach (var connection in port.Connections)
                {
                    yield return connection.FromNode;
                }
            }
            else
            {
                foreach (var connection in port.Connections)
                {
                    yield return connection.ToNode;
                }
            }
        }

        public BasePortProcessor AddPort(BasePort port)
        {
            var portVM = ViewModelFactory.CreateViewModel(port) as BasePortProcessor;
            AddPort(portVM);
            return portVM;
        }

        public void AddPort(BasePortProcessor port)
        {
            ports.Add(port.Name, port);
            switch (port.Direction)
            {
                case BasePort.Direction.Left:
                {
                    leftPorts.Add(port);
                    break;
                }
                case BasePort.Direction.Right:
                {
                    rightPorts.Add(port);
                    break;
                }
            }

            port.Owner = this;
            onPortAdded?.Invoke(port);
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
                    leftPorts.Remove(port);
                    break;
                }
                case BasePort.Direction.Right:
                {
                    rightPorts.Remove(port);
                    break;
                }
            }

            onPortRemoved?.Invoke(port);
        }

        public void RemovePort(string portName)
        {
            RemovePort(ports[portName]);
        }

        public void SortPort(Func<BasePortProcessor, BasePortProcessor, int> comparer)
        {
            leftPorts.QuickSort(comparer);
            rightPorts.QuickSort(comparer);
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

        #region Helper

        public virtual void DrawGizmos(IGraphOwner graphOwner)
        {
        }

        #endregion

        public const string TITLE_NAME = "title";
        public const string TITLE_COLOR_NAME = "titleColor";
        public const string TOOLTIP_NAME = "tooltip";
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