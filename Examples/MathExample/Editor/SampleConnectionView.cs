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

#if UNITY_EDITOR
using Jiange.GraphProcessor.Editors;
using UnityEditor.Experimental.GraphView;

public class SampleConnectionView : BaseConnectionView
{
    protected override EdgeControl CreateEdgeControl()
    {
        return new BetterEdgeControl(this)
        {
            capRadius = 4f,
            interceptWidth = 6f
        };
    }
}
#endif