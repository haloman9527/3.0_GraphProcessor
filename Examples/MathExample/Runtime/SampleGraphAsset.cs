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

using Moyo.GraphProcessor;
using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class SampleGraphAsset : ScriptableObject, IGraphAsset
{
    [SerializeField]
    private SampleGraph data;
    
    public Type GraphType => typeof(SampleGraph);

    public void SaveGraph(BaseGraph graph) => this.data = (SampleGraph)graph;

    public BaseGraph LoadGraph() => data;

    [Button]
    public void Reset()
    {
        SaveGraph(new SampleGraph());
    }
}