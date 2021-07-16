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
using UnityEditor;

namespace CZToolKit.GraphProcessor.Editors
{
    public static class GraphProcessorEditorUtility
    {
        #region GraphViewTypeCache
        /// <summary> GraphEditorWindow类型缓存 Key:Graph类型 Value:Graph视图类型 </summary>
        static Dictionary<Type, Type> GRAPH_EDITOR_WINDOW_TYPE_CACHE;

        /// <summary> 根据Graph类型返回对应窗口类型 </summary>
        /// <param name="_graphType"> Graph类型 </param>
        /// <returns> 窗口类型 </returns>
        public static Type GetGraphWindowType(Type _graphType)
        {
            if (GRAPH_EDITOR_WINDOW_TYPE_CACHE == null)
            {
                GRAPH_EDITOR_WINDOW_TYPE_CACHE = new Dictionary<Type, Type>();

                foreach (var type in TypeCache.GetTypesWithAttribute<CustomGraphWindowAttribute>())
                {
                    if (Utility_Attribute.TryGetTypeAttribute(type, out CustomGraphWindowAttribute attribute))
                        GRAPH_EDITOR_WINDOW_TYPE_CACHE[attribute.graphType] = type;
                }
            }
            if (GRAPH_EDITOR_WINDOW_TYPE_CACHE.TryGetValue(_graphType, out Type graphWindowType))
                return graphWindowType;

            return typeof(BaseGraphWindow);
        }
        #endregion

        #region NodeViewTypeCache
        /// <summary> NodeView类型缓存 Key:节点类型，Value:节点视图类型 </summary>
        static Dictionary<Type, Type> NODE_VIEW_TYPE_CACHE;

        /// <summary> 根据节点类型返回对应节点视图类型 </summary>
        /// <param name="_nodeType"> 节点类型 </param>
        /// <returns> 节点视图类型 </returns>
        public static Type GetNodeViewType(Type _nodeDataType)
        {
            if (NODE_VIEW_TYPE_CACHE == null)
            {
                NODE_VIEW_TYPE_CACHE = new Dictionary<Type, Type>();
                foreach (var _nodeViewType in TypeCache.GetTypesWithAttribute<CustomNodeViewAttribute>())
                {
                    if (Utility_Attribute.TryGetTypeAttribute(_nodeViewType, out CustomNodeViewAttribute attri))
                        NODE_VIEW_TYPE_CACHE[attri.nodeType] = _nodeViewType;
                }
            }
            if (NODE_VIEW_TYPE_CACHE.TryGetValue(_nodeDataType, out Type nodeViewType))
                return nodeViewType;

            return null;
        }
        #endregion

        #region NodeNames
        public static string GetNodeDisplayName(Type _nodeType)
        {
            if (Utility_Attribute.TryGetTypeAttribute(_nodeType, out NodeMenuItemAttribute attri))
            {
                if (attri.titles != null && attri.titles.Length != 0)
                    return attri.titles[attri.titles.Length - 1];
            }
            return _nodeType.Name;
        }

        public static string GetDisplayName(string _fieldName)
        {
            return ObjectNames.NicifyVariableName(_fieldName);
        }
        #endregion
    }
}

