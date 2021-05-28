using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Examples
{
    [CreateAssetMenu(menuName = "GraphProcessor Examples/New Math", fileName = "New Math Graph")]
    public class MathGraphAsset : BaseGraphAsset<MathGraph>
    {
    }

    [Serializable]
    public class MathGraph : BaseGraph
    {

    }
}
