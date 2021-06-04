using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Examples
{
    [CreateAssetMenu(menuName = "GraphProcessor Examples/New Math", fileName = "New Math Graph")]
    [NodeIcon("Assets/CZToolKit/0.1_GOAP/Icons/Wander.png", width = 25, height = 25)]

    public class MathGraphAsset : BaseGraphAsset<MathGraph> { }

    [Serializable]
    public class MathGraph : BaseGraph { }
}
