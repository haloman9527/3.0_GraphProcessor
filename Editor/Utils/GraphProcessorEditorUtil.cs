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
using CZToolKit.Core;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace CZToolKit.GraphProcessor.Editors
{
    public static class GraphProcessorEditorUtil
    {
        #region GraphWindowTypeCache
        static Dictionary<Type, Type> WindowTypesCache;

        public static Type GetGraphWindowType(Type graphType)
        {
            if (WindowTypesCache == null)
            {
                WindowTypesCache = new Dictionary<Type, Type>();
                foreach (var type in TypeCache.GetTypesDerivedFrom<BaseGraphWindow>())
                {
                    if (type.IsAbstract) continue;
                    foreach (var attribute in graphType.GetCustomAttributes(true))
                    {
                        if (attribute is CustomGraphWindowAttribute customGraphWindowAttribute)
                            WindowTypesCache[customGraphWindowAttribute.targetGraphType] = type;
                    }
                }
            }
            if (WindowTypesCache.TryGetValue(graphType, out Type windowType))
                return windowType;
            if (graphType.BaseType != null)
                return GetGraphWindowType(graphType.BaseType);
            else
                return typeof(BaseGraphWindow);
        }

        #endregion

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
        public static string GetNodeDisplayName(Type nodeType)
        {
            if (Util_Attribute.TryGetTypeAttribute(nodeType, out NodeMenuItemAttribute attri))
            {
                if (attri.titles != null && attri.titles.Length != 0)
                    return attri.titles[attri.titles.Length - 1];
            }
            return nodeType.Name;
        }

        public static string GetDisplayName(string fieldName)
        {
            return ObjectNames.NicifyVariableName(fieldName);
        }
        #endregion
    }
}
#endif