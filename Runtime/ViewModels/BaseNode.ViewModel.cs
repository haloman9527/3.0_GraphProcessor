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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public abstract partial class BaseNode : IntegratedViewModel, IGraphElement
    {
        [NonSerialized]
        BaseGraph owner;
        [NonSerialized]
        Dictionary<string, BasePort> ports;
        public BaseGraph Owner { get { return owner; } }
        public string GUID { get { return guid; } }
        public IReadOnlyDictionary<string, BasePort> Ports
        {
            get
            {
                return ports;
            }
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
        public Vector2 Position
        {
            get { return GetPropertyValue<Vector2>(POSITION_NAME); }
            internal set { SetPropertyValue(POSITION_NAME, value); }
        }

        protected virtual IEnumerable<BasePort> GetPorts()
        {
            yield break;
        }

        #region API
        public virtual void Enable(BaseGraph graph)
        {
            owner = graph;
            ports = new Dictionary<string, BasePort>();
            foreach (var port in GetPorts())
            {
                ports[port.name] = port;
                port.Enable(this);
            }
        }

        public virtual void Initialize(IGraphOwner graphOwner)
        {

        }

        protected override void BindProperties()
        {
            this[TITLE_NAME] = new BindableProperty<string>(GetType().Name);
            this[TITLE_COLOR_NAME] = new BindableProperty<Color>(new Color(0.2f, 0.2f, 0.2f, 0.8f));
            this[TOOLTIP_NAME] = new BindableProperty<string>();
            this[POSITION_NAME] = new BindableProperty<Vector2>(position, v => position = v);

            Type type = GetType();

            if (Util_Attribute.TryGetTypeAttribute(type, out NodeMenuItemAttribute displayName))
            {
                if (displayName.titles != null && displayName.titles.Length != 0)
                    Title = displayName.titles[displayName.titles.Length - 1];
            }
            else
                Title = type.Name;

            if (Util_Attribute.TryGetTypeAttribute(type, out NodeTitleColorAttribute nodeTitleColor))
                TitleColor = nodeTitleColor.color;

            if (Util_Attribute.TryGetTypeAttribute(type, out NodeTooltipAttribute tooltip))
                Tooltip = tooltip.Tooltip;
        }

        public IEnumerable<BaseNode> GetConnections(string portName)
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

        public object GetValue(string port)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helper

        public virtual void DrawGizmos(IGraphOwner graphOwner) { }
        #endregion

        #region 常量
        public const string TITLE_NAME = nameof(Title);
        public const string TITLE_COLOR_NAME = nameof(TitleColor);
        public const string TOOLTIP_NAME = nameof(Tooltip);
        public const string POSITION_NAME = nameof(Position);
        #endregion

        #region 静态
        /// <summary> 根据T创建一个节点，并设置位置 </summary>
        public static T CreateNew<T>(BaseGraph graph, Vector2 position) where T : BaseNode
        {
            return CreateNew(graph, typeof(T), position) as T;
        }

        /// <summary> 根据type创建一个节点，并设置位置 </summary>
        public static BaseNode CreateNew(BaseGraph graph, Type type, Vector2 position)
        {
            if (!type.IsSubclassOf(typeof(BaseNode)))
                return null;
            var node = Activator.CreateInstance(type) as BaseNode;
            node.position = position;
            IDAllocation(node, graph);
            return node;
        }

        /// <summary> 给节点分配一个GUID，这将会覆盖已有GUID </summary>
        public static void IDAllocation(BaseNode node, BaseGraph graph)
        {
            node.guid = graph.GenerateNodeGUID();
        }
        #endregion

    }
}
