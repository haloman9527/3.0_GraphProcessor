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
using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{

    public abstract partial class BaseNode : IntegratedViewModel
    {
        public const string TITLE_NAME = nameof(Title);
        public const string TITLE_COLOR_NAME = nameof(TitleColor);
        public const string TOOLTIP_NAME = nameof(Tooltip);
        public const string POSITION_NAME = nameof(Position);

        #region 静态
        /// <summary> 根据T创建一个节点，并设置位置 </summary>
        public static T CreateNew<T>(BaseGraph graph, Vector2 position) where T : BaseNode
        {
            return CreateNew(typeof(T), graph, position) as T;
        }

        /// <summary> 根据type创建一个节点，并设置位置 </summary>
        public static BaseNode CreateNew(Type type, BaseGraph graph, Vector2 position)
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
        [NonSerialized]
        BaseGraph owner;
        public string GUID { get { return guid; } }
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
            set { SetPropertyValue(POSITION_NAME, value); }
        }

        public virtual void Enable(BaseGraph graph)
        {
            owner = graph;
        }

        public virtual IEnumerable<Slot> GetSlots()
        {
            yield break;
        }

        public override void InitializeBindableProperties()
        {
            this[TITLE_NAME] = new BindableProperty<string>();
            this[TITLE_COLOR_NAME] = new BindableProperty<Color>(new Color(0.2f, 0.2f, 0.2f, 0.8f));
            this[TOOLTIP_NAME] = new BindableProperty<string>();
            this[POSITION_NAME] = new BindableProperty<Vector2>(position, v => position = v);

            Type type = GetType();

            if (Utility_Attribute.TryGetTypeAttribute(type, out NodeMenuItemAttribute displayName))
            {
                if (displayName.titles != null && displayName.titles.Length != 0)
                    Title = displayName.titles[displayName.titles.Length - 1];
            }
            else
                Title = type.Name;

            if (Utility_Attribute.TryGetTypeAttribute(type, out NodeTitleColorAttribute nodeTitleColor))
                TitleColor = nodeTitleColor.color;

            if (Utility_Attribute.TryGetTypeAttribute(type, out NodeTooltipAttribute tooltip))
                Tooltip = tooltip.Tooltip;
        }

        public virtual void OnInitializedPropertyMapping(IVariableOwner variableOwner) { }

        #region Overrides
        public virtual void Initialize(IGraphOwner graphOwner) { }

        public virtual void DrawGizmos(GraphAssetOwner graphOwner) { }
        #endregion

        #region API
        public IEnumerable<BaseNode> GetParentNodes()
        {
            foreach (var edge in owner.Connections)
            {
                if (edge.ToNode == this)
                    yield return edge.FromNode;
            }
        }

        public IEnumerable<BaseNode> GetChildNodes()
        {
            foreach (var edge in owner.Connections)
            {
                if (edge.FromNode == this)
                    yield return edge.ToNode;
            }
        }
        #endregion
    }
}
