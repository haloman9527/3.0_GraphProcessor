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

using System;
using System.Collections.Generic;
using CZToolKit;

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
        private static Dictionary<Type, NodeStaticInfo> s_NodeStaticInfos = new Dictionary<Type, NodeStaticInfo>();

        public static Dictionary<Type, NodeStaticInfo> NodeStaticInfos
        {
            get { return s_NodeStaticInfos; }
        }

        static GraphProcessorUtil()
        {
            Init();
        }

        private static void Init()
        {
            s_NodeStaticInfos = new Dictionary<Type, NodeStaticInfo>();
            foreach (var t in Util_TypeCache.GetTypesDerivedFrom<BaseNode>())
            {
                if (t.IsAbstract)
                    continue;

                var nodeStaticInfo = new NodeStaticInfo();
                nodeStaticInfo.title = t.Name;
                nodeStaticInfo.tooltip = string.Empty;
                nodeStaticInfo.customTitleColor = new ToggleValue<InternalColor>();
                NodeStaticInfos.Add(t, nodeStaticInfo);

                if (Util_Reflection.TryGetTypeAttribute(t, true, out NodeMenuAttribute nodeMenu))
                {
                    if (!string.IsNullOrEmpty(nodeMenu.path))
                    {
                        nodeStaticInfo.path = nodeMenu.path;
                        nodeStaticInfo.menu = nodeMenu.path.Split('/');
                        nodeStaticInfo.title = nodeStaticInfo.menu[nodeStaticInfo.menu.Length - 1];
                    }
                    else
                    {
                        nodeStaticInfo.path = t.Name;
                        nodeStaticInfo.menu = new string[] { t.Name };
                        nodeStaticInfo.title = t.Name;
                    }

                    nodeStaticInfo.hidden = nodeMenu.hidden;
                }
                else
                {
                    nodeStaticInfo.path = t.Name;
                    nodeStaticInfo.menu = new string[] { t.Name };
                    nodeStaticInfo.title = t.Name;
                    nodeStaticInfo.hidden = false;
                }

                if (Util_Reflection.TryGetTypeAttribute(t, true, out NodeTitleAttribute titleAttr))
                {
                    if (!string.IsNullOrEmpty(titleAttr.title))
                        nodeStaticInfo.title = titleAttr.title;
                }

                if (Util_Reflection.TryGetTypeAttribute(t, true, out NodeTooltipAttribute tooltipAttr))
                {
                    nodeStaticInfo.tooltip = tooltipAttr.Tooltip;
                }

                if (Util_Reflection.TryGetTypeAttribute(t, true, out NodeTitleColorAttribute titleColorAttr))
                {
                    nodeStaticInfo.customTitleColor.enable = true;
                    nodeStaticInfo.customTitleColor.value = titleColorAttr.color;
                }
            }
        }
    }
}