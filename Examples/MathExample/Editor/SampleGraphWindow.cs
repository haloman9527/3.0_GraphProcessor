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
using CZToolKit.Core.ViewModel;
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using OdinSerializer;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomView(typeof(SampleGraph))]
public class SampleGraphWindow : BaseGraphWindow
{
    protected override BaseGraphView NewGraphView(BaseGraphVM graph)
    {
        return new SampleGraphView();
    }

    protected override void OnGraphLoaded()
    {
        base.OnGraphLoaded();

        ToolbarButton btnSave = new ToolbarButton();
        btnSave.text = "Save";
        btnSave.clicked += Save;
        btnSave.style.width = 80;
        btnSave.style.unityTextAlign = TextAnchor.MiddleCenter;
        ToolbarRight.Add(btnSave);

        GraphView.RegisterCallback<KeyDownEvent>(KeyDownCallback);
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
        Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        List<BaseConnection> connections = new List<BaseConnection>();
        List<BaseGroup> groups = new List<BaseGroup>();
        foreach (var item in GraphView.selection)
        {
            switch (item)
            {
                case BaseNodeView nodeView:
                    nodes.Add(nodeView.ViewModel.GUID, nodeView.ViewModel.Model);
                    break;
                case BaseConnectionView connectionView:
                    connections.Add(connectionView.ViewModel.Model);
                    break;
                case BaseGroupView groupView:
                    groups.Add(groupView.ViewModel.Model);
                    break;
            }
        }

        GraphView.CommandDispacter.BeginGroup();

        var nodesStr = SerializationUtility.SerializeValue(nodes, DataFormat.Binary);
        var connectionsStr = SerializationUtility.SerializeValue(connections, DataFormat.Binary);
        var groupsStr = SerializationUtility.SerializeValue(groups, DataFormat.Binary);

        nodes = SerializationUtility.DeserializeValue<Dictionary<string, BaseNode>>(nodesStr, DataFormat.Binary);
        connections = SerializationUtility.DeserializeValue<List<BaseConnection>>(connectionsStr, DataFormat.Binary);
        groups = SerializationUtility.DeserializeValue<List<BaseGroup>>(groupsStr, DataFormat.Binary);

        var graph = GraphView.ViewModel;
        var nodeMaps = new Dictionary<string, BaseNodeVM>();

        GraphView.ClearSelection();

        foreach (var pair in nodes)
        {
            pair.Value.position += new InternalVector2(50, 50);
            var vm = ViewModelFactory.CreateViewModel(pair.Value) as BaseNodeVM;
            GraphView.CommandDispacter.Do(new AddNodeCommand(graph, vm));
            nodeMaps[pair.Key] = vm;
            GraphView.AddToSelection(GraphView.NodeViews[vm.GUID]);
        }

        foreach (var connection in connections)
        {
            if (nodeMaps.TryGetValue(connection.fromNode, out var from))
                connection.fromNode = from.GUID;

            if (nodeMaps.TryGetValue(connection.toNode, out var to))
                connection.toNode = to.GUID;

            var vm = ViewModelFactory.CreateViewModel(connection) as BaseConnectionVM;
            GraphView.CommandDispacter.Do(new ConnectCommand(graph, vm));
            GraphView.AddToSelection(GraphView.ConnectionViews[vm]);
        }

        foreach (var group in groups)
        {
            for (int i = group.nodes.Count - 1; i >= 0; i--)
            {
                if (nodeMaps.TryGetValue(group.nodes[i], out var node))
                    group.nodes[i] = node.GUID;
                else
                    group.nodes.RemoveAt(i);
            }
            var vm = ViewModelFactory.CreateViewModel(group) as BaseGroupVM;
            GraphView.CommandDispacter.Do(new AddGroupCommand(graph, vm));
            GraphView.AddToSelection(GraphView.GroupViews[vm]);
        }

        GraphView.CommandDispacter.EndGroup();
    }

    void Save()
    {
        if (GraphAsset is IGraphSerialization graphSerialization)
            graphSerialization.SaveGraph(Graph.Model);
        GraphView.SetDirty();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GraphView.SetUndirty();
    }
}
#endif