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

public class SampleGraphTest : GraphAssetOwner<SampleGraphAsset, SampleGraph>
{
    private void Update()
    {
        foreach (var node in T_Graph.Nodes.Values)
        {
            if (node is DebugNode debugNode)
            {
                debugNode.DebugInput();
            }
        }
    }
}
