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
using System.Reflection;

namespace CZToolKit.GraphProcessor
{
    public struct ToggleValue<T>
    {
        public bool enable;
        public T value;
    }

    public class NodeStaticInfo
    {
        public string path;
        public string[] menu;
        public bool hidden;
        public string title;
        public string tooltip;
        public ToggleValue<InternalColor> customTitleColor;
    }

    public static class GraphProcessorUtil
    {
        private static bool s_Initialized;
        private static Dictionary<Type, NodeStaticInfo> s_NodeStaticInfos = new Dictionary<Type, NodeStaticInfo>();

        public static Dictionary<Type, NodeStaticInfo> NodeStaticInfos
        {
            get { return s_NodeStaticInfos; }
        }

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

            foreach (var t in Util_TypeCache.GetTypesDerivedFrom<BaseNode>())
            {
                if (t.IsAbstract)
                    continue;

                var nodeStaticInfo = new NodeStaticInfo();
                nodeStaticInfo.title = t.Name;
                nodeStaticInfo.tooltip = string.Empty;
                nodeStaticInfo.customTitleColor = new ToggleValue<InternalColor>();
                NodeStaticInfos.Add(t, nodeStaticInfo);
                var nodeMenuAttribute = t.GetCustomAttribute(typeof(NodeMenuAttribute)) as NodeMenuAttribute;
                if (nodeMenuAttribute != null)
                {
                    if (!string.IsNullOrEmpty(nodeMenuAttribute.path))
                    {
                        nodeStaticInfo.path = nodeMenuAttribute.path;
                        nodeStaticInfo.menu = nodeMenuAttribute.path.Split('/');
                        nodeStaticInfo.title = nodeStaticInfo.menu[nodeStaticInfo.menu.Length - 1];
                    }
                    else
                    {
                        nodeStaticInfo.path = t.Name;
                        nodeStaticInfo.menu = new string[] { t.Name };
                        nodeStaticInfo.title = t.Name;
                    }

                    nodeStaticInfo.hidden = nodeMenuAttribute.hidden;
                }
                else
                {
                    nodeStaticInfo.path = t.Name;
                    nodeStaticInfo.menu = new string[] { t.Name };
                    nodeStaticInfo.title = t.Name;
                    nodeStaticInfo.hidden = false;
                }

                var titleAttribute = t.GetCustomAttribute(typeof(NodeTitleAttribute)) as NodeTitleAttribute;
                if (titleAttribute != null && !string.IsNullOrEmpty(titleAttribute.title))
                    nodeStaticInfo.title = titleAttribute.title;

                var tooltipAttribute = t.GetCustomAttribute(typeof(NodeTooltipAttribute)) as NodeTooltipAttribute;
                if (tooltipAttribute != null)
                    nodeStaticInfo.tooltip = tooltipAttribute.Tooltip;

                var titleColorAttribute = t.GetCustomAttribute(typeof(NodeTitleColorAttribute)) as NodeTitleColorAttribute;
                if (titleColorAttribute != null)
                {
                    nodeStaticInfo.customTitleColor.enable = true;
                    nodeStaticInfo.customTitleColor.value = titleColorAttribute.color;
                }
            }

            s_Initialized = true;
        }
    }
}