using CZToolKit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace GraphProcessor.Editors
{
    public static class NodeEditorUtility
    {

        #region GraphViewTypeCache
        /// <summary> GraphEditorWindow类型缓存 Key:Graph类型 Value:Graph视图类型 </summary>
        static Dictionary<Type, Type> GRAPH_EDITOR_WINDOW_TYPE_CACHE;

        /// <summary> 根据Graph类型返回对应窗口类型 </summary>
        /// <param name="_graphDataType"> Graph类型 </param>
        /// <returns> 窗口类型 </returns>
        public static Type GetGraphWindowType(Type _graphDataType)
        {
            if (GRAPH_EDITOR_WINDOW_TYPE_CACHE == null)
            {
                GRAPH_EDITOR_WINDOW_TYPE_CACHE = new Dictionary<Type, Type>();

                //List<Type> graphWindowTypes = ChildrenTypeCache.GetChildrenTypes(typeof(BaseGraphWindow));
                //foreach (var type in graphWindowTypes)
                foreach (var type in TypeCache.GetTypesWithAttribute<CustomGraphWindowAttribute>())
                {
                    if (AttributeCache.TryGetTypeAttribute(type, out CustomGraphWindowAttribute attribute))
                        GRAPH_EDITOR_WINDOW_TYPE_CACHE[attribute.GraphType] = type;
                }
            }
            if (GRAPH_EDITOR_WINDOW_TYPE_CACHE.TryGetValue(_graphDataType, out Type graphWindowType))
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
                    if (AttributeCache.TryGetTypeAttribute(_nodeViewType, out CustomNodeViewAttribute attri))
                        NODE_VIEW_TYPE_CACHE[attri.NodeType] = _nodeViewType;
                }
            }
            if (NODE_VIEW_TYPE_CACHE.TryGetValue(_nodeDataType, out Type nodeViewType))
                return nodeViewType;

            return typeof(BaseNodeView);
        }
        #endregion

        #region StackNodeViewCache

        static Dictionary<Type, Type> StackNodeViewTypeCache;

        public static Type GetStackNodeCustomViewType(Type stackNodeType)
        {
            if (StackNodeViewTypeCache == null)
            {
                StackNodeViewTypeCache = new Dictionary<Type, Type>();
                foreach (var t in TypeCache.GetTypesWithAttribute<CustomStackNodeView>())
                {
                    if (AttributeCache.TryGetTypeAttribute(t, out CustomNodeViewAttribute attri))
                        StackNodeViewTypeCache[attri.NodeType] = t;
                }
            }
            if (StackNodeViewTypeCache.TryGetValue(stackNodeType, out Type stackNodeViewType))
                return stackNodeViewType;
            return typeof(BaseStackNodeView);
        }
        #endregion

        #region NodeNames
        public static string GetNodeDisplayName(Type _nodeType)
        {
            if (AttributeCache.TryGetTypeAttribute(_nodeType, out NodeMenuItemAttribute attri))
            {
                if (attri.Titles != null && attri.Titles.Length != 0)
                    return attri.Titles[attri.Titles.Length - 1];
            }
            return _nodeType.Name;
        }

        public static string GetDisplayName(string _fieldName)
        {
            return ObjectNames.NicifyVariableName(_fieldName);
        }
        #endregion

        public static MonoScript FindScriptFromType(Type _type)
        {
            var scriptGUIDs = AssetDatabase.FindAssets($"t:script {_type.Name}");

            if (scriptGUIDs.Length == 0)
                return null;

            foreach (var scriptGUID in scriptGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                if (script != null && String.Equals(_type.Name, Path.GetFileNameWithoutExtension(assetPath), StringComparison.OrdinalIgnoreCase))
                    return script;
            }

            return null;
        }
    }
}

