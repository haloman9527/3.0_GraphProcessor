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
 *  Blog: https://www.mindgear.net/
 *
 */

#endregion

#if UNITY_EDITOR
using CZToolKit.VM;
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using System.Collections.Generic;
using CZToolKit;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomView(typeof(FlowGraph))]
public class FlowGraphWindow : BaseGraphWindow
{
    protected override BaseGraphView NewGraphView()
    {
        var graphView = new FlowGraphView(Graph, this, new CommandDispatcher());
        graphView.RegisterCallback<KeyDownEvent>(KeyDownCallback);
        return graphView;
    }

    protected override void BuildToolBar()
    {
        base.BuildToolBar();

        ToolbarButton btnSave = new ToolbarButton();
        btnSave.text = "Save";
        btnSave.clicked += Save;
        btnSave.style.width = 80;
        btnSave.style.unityTextAlign = TextAnchor.MiddleCenter;
        ToolbarRight.Add(btnSave);
    }

    void KeyDownCallback(KeyDownEvent evt)
    {
        if (evt.commandKey || evt.ctrlKey)
        {
            switch (evt.keyCode)
            {
                case KeyCode.S:
                    Save();
                    evt.StopImmediatePropagation();
                    break;
                case KeyCode.D:
                    Duplicate();
                    evt.StopImmediatePropagation();
                    break;
            }
        }
    }

    private void Duplicate()
    {
        if (GraphView == null)
            return;
        // 收集所有节点，连线
        Dictionary<int, BaseNode> nodes = new Dictionary<int, BaseNode>();
        List<BaseConnection> connections = new List<BaseConnection>();
        List<BaseGroup> groups = new List<BaseGroup>();
        foreach (var item in GraphView.selection)
        {
            switch (item)
            {
                case BaseNodeView nodeView:
                    nodes.Add(nodeView.ViewModel.ID, nodeView.ViewModel.Model);
                    break;
                case BaseConnectionView connectionView:
                    connections.Add(connectionView.ViewModel.Model);
                    break;
                case BaseGroupView groupView:
                    groups.Add(groupView.ViewModel.Model);
                    break;
            }
        }

        GraphView.CommandDispatcher.BeginGroup();

        var nodesStr = Sirenix.Serialization.SerializationUtility.SerializeValue(nodes, DataFormat.Binary);
        var connectionsStr = Sirenix.Serialization.SerializationUtility.SerializeValue(connections, DataFormat.Binary);
        var groupsStr = Sirenix.Serialization.SerializationUtility.SerializeValue(groups, DataFormat.Binary);

        nodes = Sirenix.Serialization.SerializationUtility.DeserializeValue<Dictionary<int, BaseNode>>(nodesStr, DataFormat.Binary);
        connections = Sirenix.Serialization.SerializationUtility.DeserializeValue<List<BaseConnection>>(connectionsStr, DataFormat.Binary);
        groups = Sirenix.Serialization.SerializationUtility.DeserializeValue<List<BaseGroup>>(groupsStr, DataFormat.Binary);

        var graph = GraphView.ViewModel;
        var nodeMaps = new Dictionary<int, BaseNodeVM>();

        GraphView.ClearSelection();

        foreach (var pair in nodes)
        {
            pair.Value.id = graph.NewID();
            pair.Value.position += new InternalVector2Int(50, 50);
            var vm = ViewModelFactory.CreateViewModel(pair.Value) as BaseNodeVM;
            GraphView.CommandDispatcher.Do(new AddNodeCommand(graph, vm));
            nodeMaps[pair.Key] = vm;
            GraphView.AddToSelection(GraphView.NodeViews[vm.ID]);
        }

        foreach (var connection in connections)
        {
            if (nodeMaps.TryGetValue(connection.fromNode, out var from))
                connection.fromNode = from.ID;

            if (nodeMaps.TryGetValue(connection.toNode, out var to))
                connection.toNode = to.ID;

            var vm = ViewModelFactory.CreateViewModel(connection) as BaseConnectionVM;
            GraphView.CommandDispatcher.Do(new ConnectCommand(graph, vm));
            GraphView.AddToSelection(GraphView.ConnectionViews[vm]);
        }

        foreach (var group in groups)
        {
            for (int i = group.nodes.Count - 1; i >= 0; i--)
            {
                if (nodeMaps.TryGetValue(group.nodes[i], out var node))
                    group.nodes[i] = node.ID;
                else
                    group.nodes.RemoveAt(i);
            }

            group.id = graph.NewID();
            GraphView.CommandDispatcher.Do(new AddGroupCommand(graph, group));
            GraphView.AddToSelection(GraphView.GroupViews[group.id]);
        }

        GraphView.CommandDispatcher.EndGroup();
    }

    void Save()
    {
        if (GraphAsset is IGraphAsset graphSerialization)
            graphSerialization.SaveGraph(Graph.Model);
        GraphView.SetDirty();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GraphView.SetUnDirty();
    }
}
#endif