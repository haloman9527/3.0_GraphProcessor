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
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace CZToolKit.GraphProcessor.Editors
{
    public static class GraphProcessorEditorUtil
    {
        #region ViewTypeCache
        static Dictionary<Type, Type> ViewTypesCache;

        public static Type GetViewType(Type targetType)
        {
            if (ViewTypesCache == null)
            {
                ViewTypesCache = new Dictionary<Type, Type>();
                foreach (var type in TypeCache.GetTypesWithAttribute<CustomViewAttribute>())
                {
                    if (type.IsAbstract) continue;
                    foreach (var attribute in type.GetCustomAttributes(false))
                    {
                        if (!(attribute is CustomViewAttribute customViewAttribute))
                            continue;
                        ViewTypesCache[customViewAttribute.targetType] = type;
                        break;
                    }
                }
            }

            var viewType = (Type)null;
            while (viewType == null)
            {
                ViewTypesCache.TryGetValue(targetType, out viewType);
                if (targetType.BaseType == null)
                    break;
                targetType = targetType.BaseType;
            }
            return viewType;
        }
        #endregion

        #region NodeNames
        static Dictionary<Type, NodeMenuItemAttribute> NodeMenuItemsCache;

        public static NodeMenuItemAttribute GetNodeMenu(Type nodeType)
        {
            if (NodeMenuItemsCache == null)
            {
                NodeMenuItemsCache = new Dictionary<Type, NodeMenuItemAttribute>();
                foreach (var type in TypeCache.GetTypesWithAttribute<NodeMenuItemAttribute>())
                {
                    if (type.IsAbstract) continue;
                    foreach (var attribute in type.GetCustomAttributes(false))
                    {
                        if (!(attribute is NodeMenuItemAttribute nodeMenuItemAttribute))
                            continue;
                        if (nodeMenuItemAttribute.titles == null || nodeMenuItemAttribute.titles.Length == 0)
                            continue;
                        NodeMenuItemsCache[type] = nodeMenuItemAttribute;
                        break;
                    }
                }
            }
            if (NodeMenuItemsCache.TryGetValue(nodeType, out var menu))
                return menu;
            return null;
        }
        #endregion
    }
}
#endif