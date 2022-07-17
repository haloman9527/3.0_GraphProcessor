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
using CZToolKit.GraphProcessor;
using UnityEngine;

public class SampleGraphTest : GraphAssetOwner<SampleGraphAsset, SampleGraphVM>
{
    public override BaseGraph DeserializeGraph()
    {
        if (T_GraphAsset != null)
            return T_GraphAsset.DeserializeGraph();
        return null;
    }

    public override void SaveGraph(BaseGraph graph)
    {
        if (T_GraphAsset != null)
            T_GraphAsset.SaveGraph(graph);
    }

    private void Update()
    {
        foreach (var node in T_Graph.Nodes.Values)
        {
            if (node is DebugNodeVM debugNode)
            {
                debugNode.DebugInput();
            }
        }
    }
}
