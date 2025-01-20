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
using UnityEngine.UIElements;

namespace Moyo.GraphProcessor.Editors
{
    [CustomView(typeof(Group))]
    public partial class BaseGroupView
    {
        protected void BuildContextualMenu(ContextualMenuPopulateEvent obj) { }
    }
}
#endif