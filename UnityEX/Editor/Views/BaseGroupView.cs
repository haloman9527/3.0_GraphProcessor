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
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BaseGroup))]
    public partial class BaseGroupView
    {
        protected void BuildContextualMenu(ContextualMenuPopulateEvent obj) { }
    }
}
#endif