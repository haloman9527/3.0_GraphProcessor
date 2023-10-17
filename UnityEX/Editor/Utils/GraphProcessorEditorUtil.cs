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
using CZToolKit.VM;
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
    }
}
#endif