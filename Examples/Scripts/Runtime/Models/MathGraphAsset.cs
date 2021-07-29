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
using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Examples
{
    [CreateAssetMenu(menuName = "Graph Processor/Examples/New Math", fileName = "New Math Graph")]
    public class MathGraphAsset : BaseGraphAsset<MathGraph> { }

    [Serializable]
    public class MathGraph : BaseGraph { }
}
