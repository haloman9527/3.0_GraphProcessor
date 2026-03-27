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

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Atom;
using UnityEditor;

namespace Atom.GraphProcessor.Editors
{
    public static class GraphProcessorEditorUtil
    {
        private static Dictionary<Type, Type> s_ViewTypesCache;

        static GraphProcessorEditorUtil()
        {
            Init();
        }

        private static void Init()
        {
            s_ViewTypesCache = new Dictionary<Type, Type>();
            foreach (var type in TypeCache.GetTypesWithAttribute<CustomViewAttribute>())
            {
                if (type.IsAbstract) continue;
                foreach (var attribute in type.GetCustomAttributes(false))
                {
                    if (!(attribute is CustomViewAttribute customViewAttribute))
                        continue;
                    s_ViewTypesCache[customViewAttribute.targetType] = type;
                }
            }
        }

        public static Type GetViewType(Type targetType)
        {
            // 直接命中缓存（含已解析过的继承链结果）
            if (s_ViewTypesCache.TryGetValue(targetType, out var cached))
                return cached;

            var originalType = targetType;
            var viewType = (Type)null;
            while (viewType == null)
            {
                s_ViewTypesCache.TryGetValue(targetType, out viewType);
                if (viewType != null) break;
                if (targetType.BaseType == null) break;
                targetType = targetType.BaseType;
            }

            // 将原始请求类型 → 结果写入缓存，避免下次再遍历继承链
            if (viewType != null && originalType != targetType)
                s_ViewTypesCache[originalType] = viewType;

            return viewType;
        }

        public static BaseGraph Clone(this BaseGraph graph)
        {
            var cloneGraph = Activator.CreateInstance(graph.GetType()) as BaseGraph;
            EditorUtility.CopySerializedManagedFieldsOnly(graph, cloneGraph);
            return cloneGraph;
        }
    }
}
#endif