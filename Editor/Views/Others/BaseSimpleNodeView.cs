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
    public abstract class BaseSimpleNodeView<M> : BaseNodeView<M> where M : BaseNodeVM
    {
        protected BaseSimpleNodeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.SimpleNodeViewStyle);
            m_CollapseButton.style.display = DisplayStyle.None;
        }
    }

    public class BaseSimpleNodeView : BaseSimpleNodeView<BaseNodeVM> { }
}
#endif