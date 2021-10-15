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
    public partial class BaseNodeView
    {
        public virtual void Initialized() { }

        public virtual BasePortView NewPortView(BaseSlot slot)
        {
            return new BasePortView(slot, typeof(object), new EdgeConnectorListener());
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

    public class BaseNodeView<M> : BaseNodeView where M : BaseNode
    {
        public M T_Model { get { return Model as M; } }
    }
}
