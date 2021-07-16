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
    [CustomNodeView(typeof(RelayNode))]
    public class RelayNodeView : SimpleNodeView<RelayNode>
    {
        public RelayNodeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.RelayNodeViewStyle);
        }

        protected override void OnInitialized()
        {
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // 双击删除
            if (evt.clickCount == 2)
                Owner.Model.RemoveRelayNode(T_Model);
        }
    }
}