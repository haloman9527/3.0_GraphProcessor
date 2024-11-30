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
using UnityEditor.Experimental.GraphView;

namespace Moyo.GraphProcessor.Editors
{
    public partial class BaseConnectionView
    {
        protected override EdgeControl CreateEdgeControl()
        {
            return new BetterEdgeControl(this);
        }

        protected virtual void BindProperties() { }

        protected virtual void UnbindProperties() { }

        protected virtual void OnInitialized() { }
    }
}
#endif