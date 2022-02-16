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
    public static class GraphProcessorEditorUtility
    {
        #region GraphWindowTypeCache
        static Dictionary<Type, Type> WindowTypeCache;

        public static Type GetGraphWindowType(Type graphType)
        {
            if (WindowTypeCache == null)
            {
                WindowTypeCache = new Dictionary<Type, Type>();
                foreach (var type in TypeCache.GetTypesDerivedFrom<BaseGraphWindow>())
                {
                    if (type.IsAbstract) continue;

                    foreach (var att in Util_Attribute.GetTypeAttributes(type, true))
                    {
                        if (att is CustomGraphWindowAttribute sAtt)
                            WindowTypeCache[sAtt.targetGraphType] = type;
                    }
                }
            }
            if (WindowTypeCache.TryGetValue(graphType, out Type windowType))
                return windowType;
            if (graphType.BaseType != null)
                return GetGraphWindowType(graphType.BaseType);
            else
                return typeof(BaseGraphWindow);
        }

        #endregion

        #region NodeViewTypeCache
        static Dictionary<Type, Type> NodeViewTypeCache;

        public static Type GetNodeViewType(Type nodeType)
        {
            if (NodeViewTypeCache == null)
            {
                NodeViewTypeCache = new Dictionary<Type, Type>();
                foreach (var type in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
                {
                    if (type.IsAbstract) continue;
                    foreach (var att in Util_Attribute.GetTypeAttributes(type, true))
                    {
                        if (att is CustomNodeViewAttribute sAtt)
                            NodeViewTypeCache[sAtt.targetNodeType] = type;
                    }
                }
            }
            if (NodeViewTypeCache.TryGetValue(nodeType, out Type nodeViewType))
                return nodeViewType;
            if (nodeType.BaseType != null)
                return GetNodeViewType(nodeType.BaseType);
            else
                return typeof(BaseNodeView);
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