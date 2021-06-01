using CZToolKit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CZToolKit.GraphProcessor
{
    /// <summary> 节点端口数据缓存 </summary>
    public static class NodeDataCache
    {
        private static Dictionary<Type, List<NodePort>> PortCache = null;

        private static bool Initialized { get { return PortCache != null; } }

        private static void CachePorts(Type _nodeType)
        {
            List<FieldInfo> fieldInfos = Utility_Refelection.GetFieldInfos(_nodeType);

            foreach (var fieldInfo in fieldInfos)
            {
                // 获取接口特性
                if (!Utility_Attribute.TryGetFieldAttribute(_nodeType, fieldInfo.Name, out PortAttribute portAttribute)) continue;

                if (!PortCache.ContainsKey(_nodeType)) PortCache.Add(_nodeType, new List<NodePort>());

                PortCache[_nodeType].Add(new NodePort(fieldInfo, portAttribute));
            }

            //List<MethodInfo> methodInfos = GetNodeMethods(nodeType);
            //foreach (var methodInfo in methodInfos)
            //{
            //    // 获取接口特性
            //    if (!AttributeCache.TryGetMethodAttribute(nodeType, methodInfo.Name, out CustomPortBehaviourAttribute portBehaviourAttribute)) continue;
            //}
        }

        private static void BuildCache()
        {
            PortCache = new Dictionary<Type, List<NodePort>>();
            foreach (var nodeType in Utility_Refelection.GetChildrenTypes<BaseNode>())
            {
                if (nodeType.IsAbstract) continue;
                CachePorts(nodeType);
            }
        }

        /// <summary> 更新端口 </summary>
        public static void UpdateStaticPorts(BaseNode _node)
        {
            if (!Initialized) BuildCache();
            // Ports.Clear();
            Type nodeType = _node.GetType();

            Dictionary<string, NodePort> staticPorts = new Dictionary<string, NodePort>();
            if (PortCache.TryGetValue(nodeType, out List<NodePort> typePortCache))
            {
                foreach (var nodePort in typePortCache)
                {
                    staticPorts[nodePort.FieldName] = nodePort;
                }
            }
            // 清理端口，移除不存在的端口
            // 通过遍历当前节点的接口实现
            foreach (var port in _node.Ports.ToList())
            {
                if (staticPorts.TryGetValue(port.Key, out NodePort cachePort))
                {
                    // 如果端口特性发生了更改，则重载端口
                    if (port.Value.DisplayType != cachePort.DisplayType ||
                        port.Value.Direction != cachePort.Direction ||
                        port.Value.IsMulti != cachePort.IsMulti ||
                        port.Value.TypeConstraint != cachePort.TypeConstraint)
                    {
                        port.Value.Reload(cachePort);

                        foreach (var edge in port.Value.GetEdges().ToList())
                        {
                            if (edge == null) continue;
                            if (!NodePort.IsCompatible(edge.InputPort, edge.OutputPort))
                                edge.Owner.Disconnect(edge);
                        }
                    }
                    else
                    {
                        Type displayType = _node.PortDynamicType(port.Value.FieldName);
                        if (displayType != null)
                            port.Value.DisplayType = displayType;
                        else
                            port.Value.DisplayType = cachePort.DisplayType;
                    }
                }
                else
                {
                    // 如果端口特性已被移除，则把端口清理掉
                    // 先断开所有连接
                    // 移除端口
                    _node.Owner.Disconnect(port.Value);
                    _node.Ports.Remove(port.Key);
                }
            }

            // 添加缺失的接口
            foreach (NodePort staticPort in staticPorts.Values)
            {
                if (!_node.Ports.ContainsKey(staticPort.FieldName))
                    _node.Ports[staticPort.FieldName] = new NodePort(staticPort, _node);
            }
        }

        public static List<MethodInfo> GetNodeMethods(Type _nodeType)
        {
            List<MethodInfo> methodInfos =
                new List<MethodInfo>(
                    _nodeType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

            // 获取类包含的所有方法(包含私有)
            Type tempType = _nodeType;
            while ((tempType = tempType.BaseType) != typeof(BaseNode) && tempType != null)
            {
                methodInfos.AddRange(tempType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance));
            }

            return methodInfos;
        }

    }
}