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

namespace CZToolKit.GraphProcessor
{
    public static partial class GraphProcessorUtil
    {
        static Dictionary<Type, Type> ViewModelTypeCache;

        public static Type GetViewModelType(Type modelType)
        {
            if (ViewModelTypeCache == null)
            {
                ViewModelTypeCache = new Dictionary<Type, Type>();
                foreach (var type in Util_TypeCache.GetTypesWithAttribute<ViewModelAttribute>())
                {
                    if (type.IsAbstract) continue;
                    foreach (var attribute in type.GetCustomAttributes(false))
                    {
                        if (!(attribute is ViewModelAttribute viewModelAttribute))
                            continue;
                        ViewModelTypeCache[viewModelAttribute.targetType] = type;
                        break;
                    }
                }
            }

            var viewModelType = (Type)null;
            while (viewModelType == null)
            {
                ViewModelTypeCache.TryGetValue(modelType, out viewModelType);
                if (modelType.BaseType == null)
                    break;
                modelType = modelType.BaseType;
            }
            return viewModelType;
        }

        public static object CreateViewModel(object model)
        {
            return Activator.CreateInstance(GetViewModelType(model.GetType()), model);
        }
    }
}
