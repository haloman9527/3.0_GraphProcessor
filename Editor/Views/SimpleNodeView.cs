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

using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class SimpleNodeView<M> : BaseNodeView<M> where M : BaseNode
    {
        protected SimpleNodeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.SimpleNodeViewStyle);
            //titleContainer.Add(topContainer);
            //titleContainer.Insert(0, inputContainer);
            //titleContainer.Add(outputContainer);
        }
    }
}
