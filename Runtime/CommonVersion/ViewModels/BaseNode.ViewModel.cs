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
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public abstract partial class BaseNode : ViewModel, INode
    {
        #region Fields
        /// <summary> 唯一标识 </summary>
        [NonSerialized] internal string guid;
        [NonSerialized] internal Dictionary<string, BasePort> ports;

        public event Action<BasePort> onPortAdded;
        public event Action<BasePort> onPortRemoved;
        #endregion

        #region Properties
        public IGraph Owner
        {
            get;
            internal set;
        }
        public string GUID
        {
            get { return guid; }
        }
        public Vector2 Position
        {
            get { return GetPropertyValue<Vector2>(POSITION_NAME); }
            set { SetPropertyValue(POSITION_NAME, value); }
        }
        public string Title
        {
            get { return GetPropertyValue<string>(TITLE_NAME); }
            set { SetPropertyValue(TITLE_NAME, value); }
        }
        public Color TitleColor
        {
            get { return GetPropertyValue<Color>(TITLE_COLOR_NAME); }
            set { SetPropertyValue(TITLE_COLOR_NAME, value); }
        }
        public string Tooltip
        {
            get { return GetPropertyValue<string>(TOOLTIP_NAME); }
            set { SetPropertyValue(TOOLTIP_NAME, value); }
        }
        public IReadOnlyDictionary<string, BasePort> Ports
        {
            get { return ports; }
        }
        #endregion

        internal void Enable(BaseGraph graph)
        {
            Owner = graph;
            ports = new Dictionary<string, BasePort>();

            this[POSITION_NAME] = new BindableProperty<Vector2>(() => position, v => position = v);

            Type type = GetType();
            string title = string.Empty;
            if (Util_Attribute.TryGetTypeAttribute(type, out NodeMenuItemAttribute displayName) && displayName.titles != null && displayName.titles.Length != 0)
                title = displayName.titles[displayName.titles.Length - 1];
            else
                title = type.Name;
            this[TITLE_NAME] = new BindableProperty<string>(() => title, v => title = v);

            var titleColor = DefaultTitleColor;
            if (Util_Attribute.TryGetTypeAttribute(type, out NodeTitleColorAttribute nodeTitleColorAttribute))
                titleColor = nodeTitleColorAttribute.color;
            this[TITLE_COLOR_NAME] = new BindableProperty<Color>(() => titleColor, v => titleColor = v);

            var tooltip = string.Empty;
            if (Util_Attribute.TryGetTypeAttribute(type, out NodeTooltipAttribute tooltipAttribute))
                tooltip = tooltipAttribute.Tooltip;
            this[TOOLTIP_NAME] = new BindableProperty<string>(() => tooltip, v => tooltip = v);

            OnEnabled();
        }

        internal void Initialize()
        {
            OnInitialized();
        }

        #region API
        public IEnumerable<INode> GetConnections(string portName)
        {
            if (!Ports.TryGetValue(portName, out var port))
                yield break;
            if (port.direction == BasePort.Direction.Input)
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

        public void AddPort(BasePort port)
        {
            ports.Add(port.name, port);
            port.Enable(this);
            onPortAdded?.Invoke(port);
        }

        public void RemovePort(BasePort port)
        {
            if (port.Owner != this)
            {
                return;
            }
            Owner.Disconnect(port);
            ports.Remove(port.name);
            onPortRemoved?.Invoke(port);
        }

        public void RemovePort(string portName)
        {
            RemovePort(ports[portName]);
        }
        #endregion

        #region Overrides
        protected virtual void OnEnabled()
        {

        }

        protected virtual void OnInitialized()
        {

        }
        #endregion

        #region Helper

        public virtual void DrawGizmos(IGraphOwner graphOwner) { }
        #endregion


        public const string POSITION_NAME = nameof(position);
        public const string TITLE_NAME = "title";
        public const string TITLE_COLOR_NAME = "titleColor";
        public const string TOOLTIP_NAME = "tooltip";

        public static Color DefaultTitleColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    }
}
