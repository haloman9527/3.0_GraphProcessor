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
using System.Runtime.CompilerServices;

namespace Atom.GraphProcessor
{
    public struct ToggleValue<T>
    {
        public bool Active;
        public T Value;
    }

    public struct NodeStaticInfo
    {
        public Type NodeType;
        public bool Hidden;
        public string Path;
        public string[] Menu;
        public string Title;
        public string Tooltip;
        public ToggleValue<InternalColor> CustomTitleColor;
    }

    public static class GraphProcessorUtil
    {
        private static bool s_Initialized;
        private static Dictionary<Type, NodeStaticInfo> s_NodeStaticInfos = new Dictionary<Type, NodeStaticInfo>();
        private static Snowflake s_IDGenerator = new Snowflake(0, new Snowflake.UtcMSDateTimeProvider(2020, 1, 1));

        static GraphProcessorUtil()
        {
            Init(true);
        }

        public static void Init(bool force)
        {
            if (!force && s_Initialized)
                return;

            if (s_NodeStaticInfos == null)
                s_NodeStaticInfos = new Dictionary<Type, NodeStaticInfo>();
            else
                s_NodeStaticInfos.Clear();

            foreach (var nodeType in TypesCache.GetTypesDerivedFrom<BaseNode>())
            {
                if (nodeType.IsAbstract)
                    continue;

                var nodeStaticInfo = new NodeStaticInfo();
                s_NodeStaticInfos.Add(nodeType, nodeStaticInfo);
                nodeStaticInfo.NodeType = nodeType;
                nodeStaticInfo.Title = nodeType.Name;
                nodeStaticInfo.Tooltip = string.Empty;
                nodeStaticInfo.CustomTitleColor = new ToggleValue<InternalColor>();
                // 一次性获取所有特性，避免多次反射调用
                var attributes = nodeType.GetCustomAttributes(false);
                var nodeMenuAttribute = (NodeMenuAttribute)null;
                var titleAttribute = (NodeTitleAttribute)null;
                var tooltipAttribute = (NodeTooltipAttribute)null;
                var titleColorAttribute = (NodeTitleColorAttribute)null;
                
                foreach (var attr in attributes)
                {
                    switch (attr)
                    {
                        case NodeMenuAttribute menu:
                            nodeMenuAttribute = menu;
                            break;
                        case NodeTitleAttribute title:
                            titleAttribute = title;
                            break;
                        case NodeTooltipAttribute tooltip:
                            tooltipAttribute = tooltip;
                            break;
                        case NodeTitleColorAttribute color:
                            titleColorAttribute = color;
                            break;
                    }
                }
                
                if (nodeMenuAttribute != null)
                {
                    if (!string.IsNullOrEmpty(nodeMenuAttribute.path))
                    {
                        nodeStaticInfo.Path = nodeMenuAttribute.path;
                        nodeStaticInfo.Menu = nodeMenuAttribute.path.Split('/');
                        nodeStaticInfo.Title = nodeStaticInfo.Menu[^1];
                    }
                    else
                    {
                        nodeStaticInfo.Path = nodeType.Name;
                        nodeStaticInfo.Menu = new string[] { nodeType.Name };
                        nodeStaticInfo.Title = nodeType.Name;
                    }

                    nodeStaticInfo.Hidden = nodeMenuAttribute.hidden;
                }
                else
                {
                    nodeStaticInfo.Path = nodeType.Name;
                    nodeStaticInfo.Menu = new string[] { nodeType.Name };
                    nodeStaticInfo.Title = nodeType.Name;
                    nodeStaticInfo.Hidden = false;
                }

                if (titleAttribute != null && !string.IsNullOrEmpty(titleAttribute.title))
                    nodeStaticInfo.Title = titleAttribute.title;

                if (tooltipAttribute != null)
                    nodeStaticInfo.Tooltip = tooltipAttribute.Tooltip;

                if (titleColorAttribute != null)
                {
                    nodeStaticInfo.CustomTitleColor.Active = true;
                    nodeStaticInfo.CustomTitleColor.Value = titleColorAttribute.color;
                }
            }

            s_Initialized = true;
        }

        public static NodeStaticInfo GetNodeStaticInfo(Type nodeDataType)
        {
            return s_NodeStaticInfos.GetValueOrDefault(nodeDataType);
        }
        
        public static IEnumerable<NodeStaticInfo> GetNodeStaticInfos()
        {
            return s_NodeStaticInfos.Values;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ModelAs<T>(this IGraphElementProcessor graphElement) where T : class
        {
            return graphElement.Model as T;
        }

        public static T Model<T>(this IGraphElementProcessor<T> graphElement) where T : class
        {
            return graphElement.Model as T;
        }

        public static long GenerateId()
        {
            return s_IDGenerator.GenerateId();
        }
    }
}