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
using CZToolKit.Core.Editors;
using UnityEditor;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public abstract class BaseNodeView<M> : InternalBaseNodeView where M : BaseNode
    {
        public M T_Model { get { return Model as M; } }

        public override void Initialized() { }

        public override InternalBasePortView NewPortView(BaseSlot slot)
        {
            return new BasePortView(slot, typeof(object));
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (var script in EditorUtilityExtension.FindAllScriptFromType(Model.GetType()))
            {
                evt.menu.AppendAction($"Open Script/" + script.name, _ => { AssetDatabase.OpenAsset(script); });
            }
            evt.menu.AppendSeparator();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();
        }
    }

    /// <summary> 默认 </summary>
    public sealed class BaseNodeView : BaseNodeView<BaseNode> { }
}
