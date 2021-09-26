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
using System.Linq;
using System.Reflection;

namespace CZToolKit.GraphProcessor
{
    /// <summary> 节点端口数据缓存 </summary>
    public static class GraphProcessorCache
    {
        static Dictionary<Type, List<NodePort>> portCache = null;

        public static IReadOnlyDictionary<Type, List<NodePort>> PortCache { get { return portCache; } }

        static GraphProcessorCache()
        {
            portCache = new Dictionary<Type, List<NodePort>>();
            foreach (var nodeType in Utility_Reflection.GetChildrenTypes<BaseNode>())
            {
                if (nodeType.IsAbstract) continue;
                CachePorts(nodeType);
            }
        }

        #region Ports
        static void CachePorts(Type _nodeType)
        {
            foreach (var fieldInfo in Utility_Reflection.GetFieldInfos(_nodeType))
            {
                // 获取接口特性
                if (!Utility_Attribute.TryGetFieldAttribute(_nodeType, fieldInfo.Name, out PortAttribute portAttribute)) continue;

                if (!portCache.ContainsKey(_nodeType)) portCache.Add(_nodeType, new List<NodePort>());

                portCache[_nodeType].Add(new NodePort(fieldInfo, portAttribute));
            }
        }

        /// <summary> 更新端口 </summary>
        public static void UpdateStaticPorts(BaseNode _node)
        {
            Type nodeType = _node.GetType();

            Dictionary<string, NodePort> staticPorts = new Dictionary<string, NodePort>();
            if (portCache.TryGetValue(nodeType, out List<NodePort> typePortCache))
            {
                foreach (var nodePort in typePortCache)
                {
                    staticPorts[nodePort.FieldName] = nodePort;
                }
            }

            // 清理端口，移除不存在的端口
            // 通过遍历当前节点的接口实现
            foreach (var portKV in _node.Ports.ToList())
            {
                if (staticPorts.TryGetValue(portKV.Key, out NodePort cachePort))
                {
                    if (portKV.Value.Direction != cachePort.Direction)
                    {
                        portKV.Value.EdgeGUIDs.Clear();
                        portKV.Value.Reload(cachePort);
                        continue;
                    }

                    // 如果端口特性发生了更改，则重载端口
                    if (portKV.Value.typeQualifiedName != cachePort.typeQualifiedName ||
                        portKV.Value.Multiple != cachePort.Multiple ||
                        portKV.Value.TypeConstraint != cachePort.TypeConstraint)
                    {
                        portKV.Value.Reload(cachePort);
                    }
                }
                else
                {
                    _node.Ports.Remove(portKV.Key);
                }
            }

            // 添加缺失的接口
            foreach (NodePort staticPort in staticPorts.Values)
            {
                if (!_node.Ports.ContainsKey(staticPort.FieldName))
                {
                    NodePort port = new NodePort(staticPort);
                    _node.Ports[staticPort.FieldName] = port;
                }
            }
        }
        #endregion
    }
}