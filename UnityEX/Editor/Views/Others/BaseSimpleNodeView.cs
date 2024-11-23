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

namespace Jiange.GraphProcessor.Editors
{
    public abstract class BaseSimpleNodeView<M> : BaseNodeView<M> where M : BaseNodeProcessor
    {
        protected BaseSimpleNodeView() : base()
        {
            this.AddToClassList("simple-node-view");
            styleSheets.Add(GraphProcessorStyles.SimpleNodeViewStyle);
            m_CollapseButton.style.display = DisplayStyle.None;
        }
    }

    public class BaseSimpleNodeView : BaseSimpleNodeView<BaseNodeProcessor> { }
}
#endif