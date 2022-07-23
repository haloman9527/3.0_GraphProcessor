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
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseConnectionView
    {
        protected override EdgeControl CreateEdgeControl()
        {
            return new BetterEdgeControl(this);
        }

        protected virtual void OnInitialized() { }

        protected virtual void OnBindingProperties() { }

        protected virtual void OnUnbindingProperties() { }
    }
}
#endif