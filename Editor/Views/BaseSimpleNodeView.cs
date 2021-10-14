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

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class BaseSimpleNodeView<M> : BaseNodeView<M> where M : BaseNode
    {
        protected BaseSimpleNodeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.SimpleNodeViewStyle);
        }
    }

    public sealed class BaseSimpleNodeView : BaseSimpleNodeView<BaseNode>
    {
        public override InternalBasePortView NewPortView(BaseSlot slot)
        {
            return new BasePortView(slot, typeof(object));
        }
    }
}
