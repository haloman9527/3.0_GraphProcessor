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
using CZToolKit.Core;
using CZToolKit.Core.ViewModel;
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseNode))]
    public class BaseNodeVM : ViewModel
    {
        #region Fields
        Dictionary<string, BasePortVM> ports;
        public event Action<BasePortVM> onPortAdded;
        public event Action<BasePortVM> onPortRemoved;
        #endregion

        #region Properties
        public BaseNode Model
        {
            get;
        }
        public Type ModelType
        {
            get;
        }
        /// <summary> 唯一标识 </summary>
        public string GUID
        {
            get;
            set;
        }
        public virtual InternalVector2 Position
        {
            get { return GetPropertyValue<InternalVector2>(nameof(BaseNode.position)); }
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
        public IReadOnlyDictionary<string, BasePortVM> Ports
        {
            get { return ports; }
        }
        public BaseGraphVM Owner
        {
            get;
            private set;
        }
        #endregion

        public BaseNodeVM(BaseNode model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.position = Model.position == default ? InternalVector2.zero : Model.position;
            ports = new Dictionary<string, BasePortVM>();

            {
                this[nameof(BaseNode.position)] = new BindableProperty<InternalVector2>(() => Model.position, v => Model.position = v);

                string title = string.Empty;
                if (Util_Attribute.TryGetTypeAttribute(ModelType, out NodeMenuItemAttribute displayName) && displayName.titles != null && displayName.titles.Length != 0)
                    title = displayName.titles[displayName.titles.Length - 1];
                else
                    title = ModelType.Name;
                this[TITLE_NAME] = new BindableProperty<string>(() => title, v => title = v);

                var titleColor = DefaultTitleColor;
                if (Util_Attribute.TryGetTypeAttribute(ModelType, out NodeTitleColorAttribute nodeTitleColorAttribute))
                    titleColor = nodeTitleColorAttribute.color;
                this[TITLE_COLOR_NAME] = new BindableProperty<InternalColor>(() => titleColor, v => titleColor = v);

                var tooltip = string.Empty;
                if (Util_Attribute.TryGetTypeAttribute(ModelType, out NodeTooltipAttribute tooltipAttribute))
                    tooltip = tooltipAttribute.Tooltip;
                this[TOOLTIP_NAME] = new BindableProperty<string>(() => tooltip, v => tooltip = v);
            }
        }

        internal void Enable(BaseGraphVM graph)
        {
            Owner = graph;
            OnEnabled();
        }

        internal void Disable()
        {
            OnDisabled();
        }

        #region API
        public IEnumerable<BaseNodeVM> GetConnections(string portName)
        {
            if (!Ports.TryGetValue(portName, out var port))
                yield break;
            if (port.Direction == BasePort.Direction.Input)
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

        public void AddPort(BasePortVM port)
        {
            ports.Add(port.Name, port);
            port.Enable(this);
            onPortAdded?.Invoke(port);
        }

        public void RemovePort(BasePortVM port)
        {
            if (port.Owner != this)
                return;
            Owner.Disconnect(port);
            ports.Remove(port.Name);
            onPortRemoved?.Invoke(port);
        }

        public void RemovePort(string portName)
        {
            RemovePort(ports[portName]);
        }
        #endregion

        #region Overrides
        protected virtual void OnEnabled() { }

        protected virtual void OnDisabled() { }
        #endregion

        #region Helper

        public virtual void DrawGizmos(IGraphOwner graphOwner) { }
        #endregion

        public const string TITLE_NAME = "title";
        public const string TITLE_COLOR_NAME = "titleColor";
        public const string TOOLTIP_NAME = "tooltip";
        public static readonly InternalColor DefaultTitleColor = new InternalColor(0.2f, 0.2f, 0.2f, 0.8f);
    }

    public class BaseNodeVM<T> : BaseNodeVM where T : BaseNode
    {
        public T T_Model
        {
            get;
        }

        public BaseNodeVM(BaseNode model) : base(model)
        {
            T_Model = model as T;
        }
    }
}
