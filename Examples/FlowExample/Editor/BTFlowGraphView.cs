#if UNITY_EDITOR
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

using Atom;
using Atom.GraphProcessor;
using Atom.GraphProcessor.Editors;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
/// <summary>
/// FlowGraphView：基于 BaseGraphView 的自定义图视图，用于 Flow 示例的编辑器界面。
/// 主要职责：
/// - 在节点菜单中列出可用的 FlowNode 类型供用户创建节点
/// - 使用自定义的连线视图（SampleConnectionView）来绘制连接线
/// </summary>
public class BTGraphView : BaseGraphView
{
    /// <summary>
    /// 构建节点创建菜单（右键菜单或工具栏使用）。
    /// 遍历注册的节点信息（通过 GraphProcessorUtil），并将符合条件的节点加入菜单。
    /// 过滤条件：
    /// - 节点未被标记为 Hidden
    /// - 节点类型必须是 FlowNode 的子类（只在 Flow 示例中显示相关节点）
    /// </summary>
    /// <param name="nodeMenu">节点菜单对象，向其添加 NodeEntry 条目</param>
    protected override void BuildNodeMenu(NodeMenuWindow nodeMenu)
    {
        foreach (var nodeInfo in GraphProcessorUtil.GetNodeStaticInfos())
        {
            if (nodeInfo.Hidden)
                continue;
            
            if (!typeof(BTBaseNode).IsAssignableFrom(nodeInfo.NodeType))
                continue;
            
            var path = nodeInfo.Path;
            var menu = nodeInfo.Menu;
            nodeMenu.entries.Add(new NodeMenuWindow.NodeEntry(path, menu, nodeInfo.NodeType));
        }
    }
    /// <summary>
    /// 创建并返回自定义的连接视图（连线的视觉表现和交互逻辑）。
    /// 在此示例中，统一使用 SampleConnectionView 来替代默认连线样式，
    /// 便于对连线进行自定义绘制或交互扩展（例如在连线上显示标签、动画等）。
    /// </summary>
    /// <param name="connection">连接处理器（未直接使用，但可用于根据连接类型返回不同视图）</param>
    /// <returns>用于显示连接的 BaseConnectionView 实例</returns>
    protected override BaseConnectionView NewConnectionView(BaseConnectionProcessor connection)
    {
        return new SampleConnectionView();
    }
}
#endif