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
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using CZToolKit.GraphProcessor;
using UnityEngine;

public class SampleGraphTest : GraphAssetOwner<SampleGraphAsset, SampleGraphVM>
{
    private void Update()
    {
        foreach (var node in T_Graph.Nodes.Values)
        {
            if (node is LogNodeVM debugNode)
            {
                debugNode.DebugInput();
            }
        }
    }
}
